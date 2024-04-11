using System;
using System.Net.Sockets;

namespace Code2.Services.Syslog.Internals
{
	internal class NetFactory : INetFactory
	{
		public UdpClient CreateUdpClient(ushort port)
			=> new UdpClient(port);

		public AsyncCallback CreateAsyncCallback(Action<IAsyncResult> action)
			=> new AsyncCallback(action);
	}
}
