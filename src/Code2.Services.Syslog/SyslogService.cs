using Code2.Services.Syslog.Internals;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Code2.Services.Syslog
{
	public class SyslogService<Treceive> : ISyslogService<Treceive>, IDisposable where Treceive : notnull
	{
		public SyslogService() : this(syslog_default_port) { }
		public SyslogService(ushort udpPort) : this(udpPort, new MessageMapper<Treceive>()) { }
		public SyslogService(ushort udpPort, MessageMapper<Treceive> messageMapper) : this(udpPort, messageMapper, new NetFactory()) { }
		internal SyslogService(ushort udpPort, MessageMapper<Treceive> messageMapper, INetFactory netFactory)
		{
			_udpPort = udpPort;
			_netFactory = netFactory;
			_messageMapper = messageMapper;
		}

		private readonly MessageMapper<Treceive> _messageMapper;
		private readonly INetFactory _netFactory;
		private readonly ushort _udpPort;
		private readonly object _lock = new object();

		private const ushort syslog_default_port = 514;
		private UdpClient? _udpClient;


		public event EventHandler<MessageEventArgs<Treceive>>? MessageReceived;

		public void Start()
		{
			_udpClient = _netFactory.CreateUdpClient(_udpPort);
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
