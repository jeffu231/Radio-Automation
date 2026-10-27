using Radio_Automation.Models;
using RadioAutomation.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Serialization;
using Xunit;

namespace RadioAutomation.Tests.Models
{
	public sealed class JsonSerializationTests
	{
		private static Track CreateTrack()
		{
			return new Track("Name", "Artist", "Album", 2020, TimeSpan.FromMinutes(3), new List<Tag>(), new List<Genre>()) { Path = @"C:\a.mp3" };
		}

		private static HashSet<string> TopLevelKeys(string json)
		{
			using var doc = JsonDocument.Parse(json);
			return doc.RootElement.EnumerateObject().Select(p => p.Name).ToHashSet();
		}

		[Fact]
		public void Serialize_Track_OmitsXmlIgnoredCatelProperties()
		{
			var ignored = typeof(Track).GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.IsDefined(typeof(XmlIgnoreAttribute), inherit: false))
				.Select(p => p.Name)
				.ToList();
			Assert.NotEmpty(ignored);

			var keys = TopLevelKeys(JsonSerializer.Serialize(CreateTrack(), TestJsonOptions.Create()));

			Assert.Empty(keys.Intersect(ignored));
		}

		[Fact]
		public void Serialize_Track_OmitsJsonIgnoredProperties()
		{
			var keys = TopLevelKeys(JsonSerializer.Serialize(CreateTrack(), TestJsonOptions.Create()));

			Assert.DoesNotContain("FormattedName", keys);
			Assert.DoesNotContain("IsValid", keys);
		}

		[Fact]
		public void Serialize_Track_IncludesDataProperties()
		{
			var keys = TopLevelKeys(JsonSerializer.Serialize(CreateTrack(), TestJsonOptions.Create()));

			foreach (var key in new[] { "Name", "Artist", "Album", "Duration", "Path", "Id" })
			{
				Assert.Contains(key, keys);
			}
		}

		[Fact]
		public void RoundTrip_Event_PreservesValues()
		{
			var original = new Event
			{
				Name = "Top of hour",
				Trigger = Trigger.Mqtt,
				EventType = EventType.Time,
				Demand = Demand.Delayed,
				Enabled = true,
				Expires = true,
				CronExpression = "0 * * * *",
				MqttExpression = new MqttExpression { Topic = "radio/cmd", Message = "time" },
				File = @"C:\a.mp3"
			};

			var json = JsonSerializer.Serialize(original, TestJsonOptions.Create());
			var result = JsonSerializer.Deserialize<Event>(json, TestJsonOptions.Create());

			Assert.Equal(original.Id, result.Id);
			Assert.Equal("Top of hour", result.Name);
			Assert.Equal(Trigger.Mqtt, result.Trigger);
			Assert.Equal(EventType.Time, result.EventType);
			Assert.Equal(Demand.Delayed, result.Demand);
			Assert.True(result.Enabled);
			Assert.True(result.Expires);
			Assert.Equal("0 * * * *", result.CronExpression);
			Assert.Equal("radio/cmd", result.MqttExpression.Topic);
			Assert.Equal("time", result.MqttExpression.Message);
			Assert.Equal(@"C:\a.mp3", result.File);
		}

		[Fact]
		public void RoundTrip_EventWithDefaultEnums_PreservesValues()
		{
			var original = new Event { Trigger = Trigger.Cron, Demand = Demand.Immediate, EventType = EventType.Play };

			var json = JsonSerializer.Serialize(original, TestJsonOptions.Create());
			var result = JsonSerializer.Deserialize<Event>(json, TestJsonOptions.Create());

			Assert.Equal(Trigger.Cron, result.Trigger);
			Assert.Equal(Demand.Immediate, result.Demand);
			Assert.Equal(EventType.Play, result.EventType);
		}
	}
}
