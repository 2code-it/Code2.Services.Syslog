using Code2.Services.Syslog.Internals;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Code2.Services.Syslog
{
	public class SyslogService<Treceive> : ISyslogService<Treceive>, IDisposable where Treceive : notnull
	{
		public SyslogService() : this(new SyslogServiceOptions()) { }
		public SyslogService(SyslogServiceOptions options) : this(options, new MessageMapper<Treceive>()) { }
		public SyslogService(SyslogServiceOptions options, IMessageMapper<Treceive> messageMapper) : this(options, messageMapper, new NetFactory()) { }
		internal SyslogService(SyslogServiceOptions options, IMessageMapper<Treceive> messageMapper, INetFactory netFactory)
		{
			_options = options;
			_netFactory = netFactory;
			_messageMapper = messageMapper;
			if (_options.AutoStart) Start();
		}

		private readonly IMessageMapper<Treceive> _messageMapper;
		private readonly INetFactory _netFactory;
		private readonly object _lock = new object();

		private readonly SyslogServiceOptions _options;
		private UdpClient? _udpClient;


		public event EventHandler<MessageEventArgs<Treceive>>? MessageReceived;

		public void Start()
		{
			IPAddress address = IPAddress.Parse(_options.ListenAddress);
			IPEndPoint endpoint = new IPEndPoint(address, _options.ListenPort);
			_udpClient = _netFactory.CreateUdpClient(endpoint);
			_udpClient.BeginReceive(_netFactory.CreateAsyncCallback(OnDataReceived), null);
		}

		public void Stop()
		{
			lock (_lock)
			{
				if (_udpClient is null) return;
				_udpClient.Close();
				_udpClient.Dispose();
				_udpClient = null;
			}
		}

		public async Task SendAsync<T>(T message, IPEndPoint remoteEndPoint) where T : notnull
		{
			if (_udpClient is null) throw new InvalidOperationException("Service not started");
			byte[] data = _messageMapper.GetBytes(message);
			await _udpClient.SendAsync(data, data.Length, remoteEndPoint);
		}

		private void OnDataReceived(IAsyncResult ar)
		{
			IPEndPoint? remoteEndPoint = null;
			byte[]? data = null;
			lock (_lock)
			{
				if (_udpClient is null) return;
				data = _udpClient.EndReceive(ar, ref remoteEndPoint);
				_udpClient.BeginReceive(_netFactory.CreateAsyncCallback(OnDataReceived), null);
			}
			if (data is null) return;
			Treceive message = _messageMapper.GetMessage(data);
			MessageReceived?.Invoke(this, new MessageEventArgs<Treceive>(message, remoteEndPoint ?? new IPEndPoint(0, 0)));
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
