using Radio_Automation.Events;
using Radio_Automation.Models;
using RadioAutomation.Tests.TestUtilities;
using System;
using Xunit;

namespace RadioAutomation.Tests.Events
{
	[Collection(NonParallelCollection.Name)]
	public sealed class EventBusTests : IDisposable
	{
		private readonly Action<Event> _saved;

		public EventBusTests()
		{
			_saved = EventBus.EventTriggered;
		}

		public void Dispose()
		{
			EventBus.EventTriggered = _saved;
		}

		[Fact]
		public void OnEventTriggered_NoSubscriber_DoesNotThrow()
		{
			EventBus.EventTriggered = null;

			EventBus.OnEventTriggered(new Event());
		}

		[Fact]
		public void OnEventTriggered_WithSubscriber_InvokesWithSameEvent()
		{
			Event captured = null;
			EventBus.EventTriggered = x => captured = x;
			var e = new Event();

			EventBus.OnEventTriggered(e);

			Assert.Same(e, captured);
		}

		[Fact]
		public void OnEventTriggered_MultipleSubscribers_InvokedInSubscriptionOrder()
		{
			var order = new System.Collections.Generic.List<int>();
			EventBus.EventTriggered = null;
			EventBus.EventTriggered += _ => order.Add(1);
			EventBus.EventTriggered += _ => order.Add(2);

			EventBus.OnEventTriggered(new Event());

			Assert.Collection(order, first => Assert.Equal(1, first), second => Assert.Equal(2, second));
		}

		[Fact]
		public void OnEventTriggered_UnsubscribedHandler_IsNotInvoked()
		{
			var calls = 0;
			Action<Event> handler = _ => calls++;
			EventBus.EventTriggered = null;
			EventBus.EventTriggered += handler;
			EventBus.EventTriggered -= handler;

			EventBus.OnEventTriggered(new Event());

			Assert.Equal(0, calls);
		}
	}
}
