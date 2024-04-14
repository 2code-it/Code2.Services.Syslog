namespace Code2.Services.Syslog
{
	public interface IMessageMapper<T>
	{
		byte[] GetBytes(object message);
		T GetMessage(byte[] bytes);
	}
}