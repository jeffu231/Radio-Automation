using Radio_Automation.Models;
using System;
using Xunit;

namespace RadioAutomation.Tests.Models
{
	public sealed class EventTests
	{
		[Fact]
		public void Ctor_Default_SetsDefaults()
		{
			var e = new Event();

			Assert.NotEqual(Guid.Empty, e.Id);
			Assert.Equal(Trigger.Cron, e.Trigger);
			Assert.NotNull(e.MqttExpression);
			Assert.False(e.Enabled);
			Assert.Equal(Demand.Immediate, e.Demand);
			Assert.Equal(EventType.Play, e.EventType);
		}

		[Fact]
		public void Ctor_Default_EndDateIsOneYearAfterStart()
		{
			var e = new Event();

			var expected = e.StartDateTime.AddYears(1) - e.StartDateTime;

			Assert.InRange((e.EndDateTime - e.StartDateTime - expected).Duration(), TimeSpan.Zero, TimeSpan.FromSeconds(1));
		}

		[Fact]
		public void Ctor_Default_GeneratesUniqueIds()
		{
			Assert.NotEqual(new Event().Id, new Event().Id);
		}

		[Fact]
		public void Ctor_WithEventType_SetsEventTypeAndDefaults()
		{
			var e = new Event(EventType.Time);

			Assert.Equal(EventType.Time, e.EventType);
			Assert.NotEqual(Guid.Empty, e.Id);
			Assert.Equal(Trigger.Cron, e.Trigger);
		}
	}
}
