using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Code2.Services.Syslog
{
	public class MessageMapper<T>
	{
		public MessageMapper() : this(new MessageReaderFactory()) { }
		public MessageMapper(IMessageReaderFactory messageReaderFactory)
		{
			_messageReaderFactory = messageReaderFactory;
		}

		private readonly IMessageReaderFactory _messageReaderFactory;
		private readonly string[] _rfc3164DateFormats = new[] { "MMM dd HH:mm:ss" };
		private readonly string[] _rfc5424DateFormats = new[] { "yyyy-MM-ddTHH:mm:ss.ffffffzzz", "yyyy-MM-ddTHH:mm:sszzz" };

		public virtual byte[] GetBytes(object message)
		{
			if (typeof(T) == typeof(Rfc5424Message)) return Rfc5424MessageToByteArray((Rfc5424Message)message);
			return Rfc3164MessageToByteArray((Rfc3164Message)message);
		}

		public virtual T GetMessage(byte[] bytes)
		{
			if (typeof(T) == typeof(Rfc5424Message)) return (T)ByteArrayToRfc5424Message(bytes);
			return (T)ByteArrayToRfc3164Message(bytes);
		}

		protected object ByteArrayToRfc3164Message(byte[] data)
		{
			using MessageReader reader = _messageReaderFactory.Create(data);

			var message = new Rfc3164Message();
			var codes = reader.ReadPri();

			message.SeverityCode = codes.severityCode;
			message.FacilityCode = codes.facilityCode;
			message.TimeStamp = reader.ReadUntilAsDate(' ', _rfc3164DateFormats);
			message.Host = reader.ReadUntil(' ');
			message.Text = reader.ReadToEnd();

			return message;
		}

		protected byte[] Rfc3164MessageToByteArray(Rfc3164Message message)
		{
			byte pri = (byte)(message.FacilityCode * 8 + message.SeverityCode);
			string[] parts = new[]
			{
				$"<{pri}>",
				message.TimeStamp.ToString(_rfc3164DateFormats[0]),
				message.Host,
				message.Text
			};
			return Encoding.UTF8.GetBytes(string.Join(" ", parts));
		}

		protected object ByteArrayToRfc5424Message(byte[] data)
		{
			using MessageReader reader = _messageReaderFactory.Create(data);

			var message = new Rfc5424Message();

			message.AppName = reader.ReadUntil(' ');
			message.ProcId = reader.ReadUntil(' ');
			message.MsgId = reader.ReadUntil(' ');
			reader.Read();
			message.StructuredData = reader.ReadStructuredData();
			message.Text = reader.ReadToEnd();

			return message;
		}

		protected byte[] Rfc5424MessageToByteArray(Rfc5424Message message)
		{
			byte pri = (byte)(message.FacilityCode * 8 + message.SeverityCode);

			string[] parts = new[]
			{
				$"<{pri}>",
				message.Version.ToString(),
				message.TimeStamp.ToString(_rfc5424DateFormats[0]),
				message.Host,
				message.AppName,
				message.ProcId ?? string.Empty,
				message.MsgId ?? string.Empty,
				string.Join(string.Empty, message.StructuredData.Select(x => $"[{x.Id} {GetParametersString(x.Parameters)}]")),
				message.Text
			};
			return Encoding.UTF8.GetBytes(string.Join(" ", parts));
		}

		private string GetParametersString(StringDictionary parameters)
		{
			return string.Join(" ", parameters.Keys.Cast<string>().Select(x => GetParameterString(x, parameters[x]!)));
		}

		private string GetParameterString(string key, string value)
		{
			foreach (char c in MessageReader.ESCAPABLE_CHARS)
			{
				value = value.Replace($"{c}", $"\\{c}");
			}
			return $"{key}=\"{value}\"";
		}
	}
}
