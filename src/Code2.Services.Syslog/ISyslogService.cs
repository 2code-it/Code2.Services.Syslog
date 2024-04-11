using System;
using System.Net;
using System.Threading.Tasks;

namespace Code2.Services.Syslog
{
	public interface ISyslogService<Treceive> where Treceive : notnull
	{
		event EventHandler<MessageEventArgs<Treceive>>? MessageReceived;

		Task SendAsync<T>(T message, IPEndPoint remoteEndPoint) where T : notnull;
		void Start();
		void Stop();
	}
}