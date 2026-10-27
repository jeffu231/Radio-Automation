using Radio_Automation.Utils;
using System;
using System.Threading;
using Xunit;

namespace RadioAutomation.Tests.Utils
{
	public sealed class ThreadSafeRandomTests
	{
		[Fact]
		public void Instance_IsNotNull()
		{
			Assert.NotNull(ThreadSafeRandom.Instance);
		}

		[Fact]
		public void Instance_SameThread_ReturnsSameInstance()
		{
			Assert.Same(ThreadSafeRandom.Instance, ThreadSafeRandom.Instance);
		}

		[Fact]
		public void Instance_DifferentThreads_ReturnDifferentInstances()
		{
			Random other = null;
			var thread = new Thread(() => other = ThreadSafeRandom.Instance);
			var mine = ThreadSafeRandom.Instance;

			thread.Start();
			thread.Join();

			Assert.NotNull(other);
			Assert.NotSame(mine, other);
		}
	}
}
