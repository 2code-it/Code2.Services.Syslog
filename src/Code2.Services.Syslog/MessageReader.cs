using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Code2.Services.Syslog
{
	public class MessageReader : IDisposable
	{
		public MessageReader(byte[] data)
		{
			_memoryStream = new MemoryStream(data);
			_streamReader = new StreamReader(_memoryStream);
		}

		private readonly StreamReader _streamReader;
		private readonly MemoryStream _memoryStream;

		public const string ESCAPABLE_CHARS = "\\\"]";

		public (byte facilityCode, byte severityCode) ReadPri()
		{
			_streamReader.Read();
			byte pri = ReadUntilAsByte('>');
			_streamReader.Read();
			return ((byte)Math.Floor(pri / 8.0), (byte)(pri % 8));
		}

		public byte ReadUntilAsByte(char search)
		{
			string byteString = ReadUntil(search);
			return byte.TryParse(byteString, out byte result) ? result : default;
		}

		public DateTime ReadUntilAsDate(char search, string[] dateFormats)
		{
			int minReads = dateFormats.Min(x => x.Length);
			string dateString = ReadUntil(search, minReads);
			return DateTime.TryParseExact(dateString, dateFormats, Thread.CurrentThread.CurrentCulture, System.Globalization.DateTimeStyles.None, out DateTime result)
				? result : default;
		}

		public string ReadUntil(char search, int minReads = 0)
		{
			int read;
			StringBuilder sb = new StringBuilder();
			while ((read = _streamReader.Read()) != -1)
			{
				if (read == search && sb.Length >= minReads) break;
				sb.Append((char)read);
			}
			return read == search ? sb.ToString() : string.Empty;
		}

		public string ReadToEnd()
			=> _streamReader.ReadToEnd();

		public int Peek()
			=> _streamReader.Peek();

		public int Read()
			=> _streamReader.Read();

		public void Dispose()
		{
			_streamReader.Close();
			_streamReader.Dispose();
			_memoryStream.Dispose();
		}

		public StructuredData[] ReadStructuredData()
		{
			List<StructuredData> list = new List<StructuredData>();

			while (true)
			{
				StructuredData? data = ReadStructuredDataBlock();
				if (data is null) break;
				list.Add(data);
			}
			return list.ToArray();
		}

		private StructuredData? ReadStructuredDataBlock()
		{
			int chr = Read();
			if (chr != '[') return null;
			StructuredData structuredData = new StructuredData();
			structuredData.Id = ReadUntil(' ');

			(string key, string value)? parameter;
			while ((parameter = ReadStructuredParameter()) is not null)
			{
				structuredData.Parameters[parameter.Value.key] = parameter.Value.value;
			}

			return structuredData;
		}

		private (string key, string value)? ReadStructuredParameter()
		{
			if (Peek() == ']')
			{
				Read();
				return null;
			}

			string paramName = ReadUntil('=');
			char quote = (char)Read();
			if (quote != '"')
			{
				ReadUntil(']');
				return null;
			}
			int read;
			StringBuilder sb = new StringBuilder();
			while ((read = Read()) != -1)
			{
				if (read == '"') break;
				if (read == '\\' && ESCAPABLE_CHARS.IndexOf((char)Peek()) != -1)
				{
					read = Read();
				}
				sb.Append((char)read);
			}

			return (paramName, sb.ToString());
		}
	}
}
