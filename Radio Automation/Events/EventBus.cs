using System;
using Radio_Automation.Models;

namespace Radio_Automation.Events
{
	public class EventBus
	{
		public static Action<Event> EventTriggered;

		public static void OnEventTriggered(Event e)
		{
			EventTriggered?.Invoke(e);
		}
	}
}
