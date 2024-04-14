using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Code2.Services.Syslog
{
	public class SyslogServiceOptions
	{
		public string ListenAddress { get; set; } = "0.0.0.0";
		public ushort ListenPort { get; set; } = 514;
		public bool AutoStart { get; set; }
	}
}
