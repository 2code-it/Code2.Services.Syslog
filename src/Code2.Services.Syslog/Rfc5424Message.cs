using System;

namespace Code2.Services.Syslog
{
	public class Rfc5424Message : Rfc3164Message
	{
		public byte Version { get; set; }
		public string AppName { get; set; } = string.Empty;
		public string? ProcId { get; set; }
		public string? MsgId { get; set; }
		public StructuredData[] StructuredData { get; set; } = Array.Empty<StructuredData>();
	}
}
