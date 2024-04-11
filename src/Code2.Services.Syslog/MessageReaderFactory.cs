namespace Code2.Services.Syslog
{
	public class MessageReaderFactory : IMessageReaderFactory
	{
		public MessageReader Create(byte[] data)
			=> new MessageReader(data);
	}
}
