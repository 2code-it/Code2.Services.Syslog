using System.Collections.Specialized;

namespace Code2.Services.Syslog
{
	public class StructuredData
	{
		public string Id { get; set; } = string.Empty;
		public StringDictionary Parameters { get; private set; } = new StringDictionary();
	}
}
