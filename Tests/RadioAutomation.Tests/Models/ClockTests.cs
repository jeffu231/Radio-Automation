using Radio_Automation.Models;
using System;
using System.Threading;
using Xunit;

namespace RadioAutomation.Tests.Models
{
	public sealed class ClockTests
	{
		private static readonly TimeSpan TickTimeout = TimeSpan.FromSeconds(5);

		private static bool WaitForChange(Clock clock, DateTime previous)
		{
			var deadline = DateTime.UtcNow + TickTimeout;
			while (DateTime.UtcNow < deadline)
			{
				if (clock.CurrentDateTime != previous)
				{
					return true;
				}

				Thread.Sleep(50);
			}

			return false;
		}

		[Fact]
		public void Ctor_SetsCurrentDateTimeToNow()
		{
			var before = DateTime.Now;

			var clock = new Clock();
			clock.Stop();

			Assert.InRange(clock.CurrentDateTime, before, DateTime.Now);
		}

		[Fact]
		public void Running_UpdatesCurrentDateTime()
		{
			var clock = new Clock();
			try
			{
				Assert.True(WaitForChange(clock, clock.CurrentDateTime));
			}
			finally
			{
				clock.Stop();
			}
		}

		[Fact]
		public void Dispose_HaltsUpdates()
		{
			var clock = new Clock();
			clock.Dispose();
			var disposed = clock.CurrentDateTime;

			Thread.Sleep(1500);

			Assert.Equal(disposed, clock.CurrentDateTime);
		}

		[Fact]
		public void Dispose_CalledTwiceOrAfterStop_DoesNotThrow()
		{
			var clock = new Clock();
			clock.Stop();

			clock.Dispose();
			clock.Dispose();
			clock.Stop();
		}

		[Fact]
		public void Stop_HaltsUpdates()
		{
			var clock = new Clock();
			clock.Stop();
			var stopped = clock.CurrentDateTime;

			Thread.Sleep(1500);

			Assert.Equal(stopped, clock.CurrentDateTime);
		}
	}
}
