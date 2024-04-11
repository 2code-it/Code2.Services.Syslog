using System;
using System.Net.Sockets;

namespace Code2.Services.Syslog.Internals
{
	internal interface INetFactory
	{
		AsyncCallback CreateAsyncCallback(Action<IAsyncResult> action);
		UdpClient CreateUdpClient(ushort port);
	}
}