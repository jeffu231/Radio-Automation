using Radio_Automation.Events;
using Radio_Automation.Models;
using System;
using Xunit;

namespace RadioAutomation.Tests.Events
{
	public sealed class PendingEventTests
	{
		private static readonly DateTime Time = new DateTime(2026, 10, 26, 14, 0, 0);

		[Fact]
		public void Equals_SameEventAndTime_IsTrue()
		{
			var e = new Event();
			var a = new PendingEvent(e, Time);
			var b = new PendingEvent(e, Time);

			Assert.True(a.Equals(b));
			Assert.True(a.Equals((object)b));
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
		}

		[Fact]
		public void Equals_DifferentTime_IsFalse()
		{
			var e = new Event();

			Assert.False(new PendingEvent(e, Time).Equals(new PendingEvent(e, Time.AddMinutes(1))));
		}

		[Fact]
		public void Equals_DifferentEvent_IsFalse()
		{
			Assert.False(new PendingEvent(new Event(), Time).Equals(new PendingEvent(new Event(), Time)));
		}

		[Fact]
		public void Equals_Null_IsFalse()
		{
			var pending = new PendingEvent(new Event(), Time);

			Assert.False(pending.Equals((PendingEvent)null));
			Assert.False(pending.Equals((object)null));
		}

		[Fact]
		public void Equals_OtherType_IsFalse()
		{
			Assert.False(new PendingEvent(new Event(), Time).Equals("string"));
		}

		[Fact]
		public void Equals_SameReference_IsTrue()
		{
			var pending = new PendingEvent(new Event(), Time);

			Assert.True(pending.Equals(pending));
		}
	}
}
