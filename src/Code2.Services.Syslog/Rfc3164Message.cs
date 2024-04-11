using System;

namespace Code2.Services.Syslog
{
	public class Rfc3164Message
	{
		public byte FacilityCode { get; set; }
		public byte SeverityCode { get; set; }
		public DateTime TimeStamp { get; set; }
		public string Host { get; set; } = string.Empty;
		public string Text { get; set; } = string.Empty;
	}
}
