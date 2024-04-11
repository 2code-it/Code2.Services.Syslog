namespace Code2.Services.Syslog
{
	public interface IMessageReaderFactory
	{
		MessageReader Create(byte[] data);
	}
}