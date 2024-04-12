using Microsoft.VisualStudio.TestTools.UnitTesting;
using Code2.Services.Syslog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code2.Services.Syslog.Tests
{
	[TestClass]
	public class MessageMapperTests
	{
		private const string _rfc3164DateFormat = "MMM dd HH:mm:ss";
		private const string _rfc5424DateFormat = "yyyy-MM-ddTHH:mm:ss.ffffffzzz";

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void GetMessage_When_TypeMappingNotDefined_Expect_NotSupportedException()
		{
			MessageMapper<TestItem> mapper = new MessageMapper<TestItem>();

			mapper.GetMessage(Array.Empty<byte>());
		}

		[TestMethod]
		public void GetMessage_When_TypeMappingIsDefined_Expect_MappedMessage()
		{
			MessageMapperTestItem mapper = new MessageMapperTestItem();

			var message = mapper.GetMessage(Array.Empty<byte>());

			Assert.IsNotNull(message);
		}

		[TestMethod]
		public void GetMessage_When_Rfc3164Message_Expect_CorrectMapping()
		{
			byte severityCode = 5;
			byte facilityCode = 12;
			byte pri = (byte)(facilityCode * 8 + severityCode);
			DateTime timestamp = new DateTime(DateTime.Now.Year, 2,3,4,5,6);
			string host = "host1";
			string text = "message text";
			string messageString = $"<{pri}>{timestamp.ToString(_rfc3164DateFormat)} {host} {text}";

			MessageMapper<Rfc3164Message> messageMapper = new MessageMapper<Rfc3164Message>();
			var message = messageMapper.GetMessage(Encoding.UTF8.GetBytes(messageString));

			Assert.AreEqual(severityCode, message.SeverityCode);
			Assert.AreEqual(facilityCode, message.FacilityCode);
			Assert.AreEqual(timestamp, message.TimeStamp);
			Assert.AreEqual(host, message.Host);
			Assert.AreEqual(text, message.Text);
		}

		[TestMethod]
		public void GetMessage_When_Rfc5424Message_Expect_CorrectMapping()
		{
			byte severityCode = 5;
			byte facilityCode = 12;
			byte pri = (byte)(facilityCode * 8 + severityCode);
			byte version = 1;
			DateTime timestamp = new DateTime(2000, 2, 3, 4, 5, 6);
			string host = "host1";
			string appName = "app1";
			string procId = "1002";
			string msgId = "1223";
			string dataId = "dataId";
			string key1 = "key1";
			string value1 = "value1";
			string key2 = "key2";
			string value2 = "value2";
			string text = "message text";
			string messageString = $"<{pri}>{version} {timestamp.ToString(_rfc5424DateFormat)} {host} {appName} {procId} {msgId} [{dataId} {key1}=\"{value1}\" {key2}=\"{value2}\"] {text}";

			MessageMapper<Rfc5424Message> messageMapper = new MessageMapper<Rfc5424Message>();
			var message = messageMapper.GetMessage(Encoding.UTF8.GetBytes(messageString));

			Assert.AreEqual(severityCode, message.SeverityCode);
			Assert.AreEqual(facilityCode, message.FacilityCode);
			Assert.AreEqual(version, message.Version);
			Assert.AreEqual(timestamp, message.TimeStamp);
			Assert.AreEqual(host, message.Host);
			Assert.AreEqual(appName, message.AppName);
			Assert.AreEqual(procId, message.ProcId);
			Assert.AreEqual(msgId, message.MsgId);
			Assert.AreEqual(dataId, message.StructuredData[0].Id);
			Assert.AreEqual(value1, message.StructuredData[0].Parameters[key1]);
			Assert.AreEqual(value2, message.StructuredData[0].Parameters[key2]);

			Assert.AreEqual(text, message.Text);
		}



		private class MessageMapperTestItem: MessageMapper<TestItem>
		{
			public override TestItem GetMessage(byte[] bytes)
			{
				return new TestItem();
			}
		}

		private class TestItem 
		{ 
			public DateTime Created { get; set; }
			public string? Text { get; set; }
		}

	}
}