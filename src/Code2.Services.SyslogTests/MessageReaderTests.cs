using System;
using System.Text;

namespace Code2.Services.Syslog.Tests
{
	[TestClass]
	public class MessageReaderTests
	{
		[TestMethod]
		public void ReadPri_When_PriValueSet_Expect_FacilityAndSeverityCodes()
		{
			byte facilityCode = 10;
			byte severityCode = 5;
			byte pri = (byte)(facilityCode * 8 + severityCode);
			byte[] data = Encoding.UTF8.GetBytes($"<{pri}>");
			using MessageReader reader = new MessageReader(data);

			var codes = reader.ReadPri();

			Assert.AreEqual(facilityCode, codes.facilityCode);
			Assert.AreEqual(severityCode, codes.severityCode);
		}

		[TestMethod]
		public void ReadUntil_When_SearchCharNotFound_Expect_StringEmpty()
		{
			byte[] data = Encoding.UTF8.GetBytes("abcdefg");
			using MessageReader reader = new MessageReader(data);

			string result = reader.ReadUntil(' ');

			Assert.AreEqual(string.Empty, result);
		}

		[TestMethod]
		public void ReadUntil_When_SearchCharFound_Expect_StringUntilSearchChar()
		{
			char search = '5';
			byte[] data = Encoding.UTF8.GetBytes("0123456789");
			using MessageReader reader = new MessageReader(data);

			string result = reader.ReadUntil(search);

			Assert.AreEqual("01234", result);
		}

		[TestMethod]
		public void ReadUntilAsByte_When_SearchCharFound_Expect_Byte()
		{
			char search = ' ';
			byte value = 123;
			byte[] data = Encoding.UTF8.GetBytes($"{value}{search}");
			using MessageReader reader = new MessageReader(data);

			byte result = reader.ReadUntilAsByte(search);

			Assert.AreEqual(value, result);
		}

		[TestMethod]
		public void ReadUntilAsByte_When_SearchCharNotFound_Expect_ByteDefault()
		{
			char search = 'a';
			byte value = 123;
			byte defaultValue = default;
			byte[] data = Encoding.UTF8.GetBytes($"{value}");
			using MessageReader reader = new MessageReader(data);

			byte result = reader.ReadUntilAsByte(search);

			Assert.AreEqual(defaultValue, result);
		}

		[TestMethod]
		public void ReadUntilAsByte_When_InvalidByteValue_Expect_ByteDefault()
		{
			char search = 'a';
			byte defaultValue = default;
			byte[] data = Encoding.UTF8.GetBytes($"999{search}");
			using MessageReader reader = new MessageReader(data);

			byte result = reader.ReadUntilAsByte(search);

			Assert.AreEqual(defaultValue, result);
		}

		[TestMethod]
		public void ReadUntilAsDate_When_InvalidDateValue_Expect_DateDefault()
		{
			char search = 'a';
			DateTime defaultValue = default;
			string dateFormat = "yyyyMMddTHH:mm:ss";
			byte[] data = Encoding.UTF8.GetBytes($"12-12-2003{search}");
			using MessageReader reader = new MessageReader(data);

			DateTime result = reader.ReadUntilAsDate(search, new[] { dateFormat });

			Assert.AreEqual(defaultValue, result);
		}

		[TestMethod]
		public void ReadUntilAsDate_When_DateValueAccordingToFormat_Expect_DateValue()
		{
			char search = 'a';
			DateTime value = new DateTime(2000, 1, 1, 1, 1, 1);
			string dateFormat = "yyyyMMddTHH:mm:ss";
			byte[] data = Encoding.UTF8.GetBytes($"{value.ToString(dateFormat)}{search}");
			using MessageReader reader = new MessageReader(data);

			DateTime result = reader.ReadUntilAsDate(search, new[] { dateFormat });

			Assert.AreEqual(value, result);
		}

		[TestMethod]
		public void ReadStructuredData_When_AccordingToSpec_Expect_StructuredData()
		{
			string id = "id";
			string parameterName = "parameter1";
			string parameterValue = "value1";
			byte[] data = Encoding.UTF8.GetBytes($"[{id} {parameterName}=\"{parameterValue}\"]");
			using MessageReader reader = new MessageReader(data);

			StructuredData[] result = reader.ReadStructuredData();

			Assert.AreEqual(id, result[0].Id);
			Assert.AreEqual(parameterValue, result[0].Parameters[parameterName]);
		}

		[TestMethod]
		public void ReadStructuredData_When_ParameterNotAccordingToSpec_Expect_ParameterSkipped()
		{
			string id = "id";
			string parameterName = "parameter1";
			string parameterValue = "value1";
			byte[] data = Encoding.UTF8.GetBytes($"[{id} {parameterName}={parameterValue}]");
			using MessageReader reader = new MessageReader(data);

			StructuredData[] result = reader.ReadStructuredData();

			Assert.AreEqual(0, result[0].Parameters.Count);
		}
	}
}