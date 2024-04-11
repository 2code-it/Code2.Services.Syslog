using System;
using System.Net;

namespace Code2.Services.Syslog
{
	public class MessageEventArgs<T> : EventArgs
	{
		public MessageEventArgs(T message, IPEndPoint remoteEndPoint)
		{
			Message = message;
			RemoteEndPoint = remoteEndPoint;
		}

		public T Message { get; private set; }
		public IPEndPoint RemoteEndPoint { get; private set; }
	}
}
