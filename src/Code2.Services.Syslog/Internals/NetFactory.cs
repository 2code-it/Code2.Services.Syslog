using System;
using System.Net;
using System.Net.Sockets;

namespace Code2.Services.Syslog.Internals
{
	internal class NetFactory : INetFactory
	{
		public UdpClient CreateUdpClient(IPEndPoint endpoint)
			=> new UdpClient(endpoint);

		public AsyncCallback CreateAsyncCallback(Action<IAsyncResult> action)
			=> new AsyncCallback(action);
	}
}
