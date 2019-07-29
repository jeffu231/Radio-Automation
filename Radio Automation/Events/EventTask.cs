using System;
using JobToolkit.Core;
using Radio_Automation.Models;

namespace Radio_Automation.Events
{
	[Serializable]
	public class EventTask:JobTask
	{
		public EventTask(Event e)
		{
			Event = e;
		}

		/// <inheritdoc />
		public override string Title => Event.Name;

		public Event Event { get; }

		#region Overrides of JobTask

		/// <inheritdoc />
		public override void Execute()
		{
			//Console.Out.WriteLineAsync($"{DateTime.Now.ToString()} - {Event.EventType}");
			if (Event.Enabled)
			{
				EventBus.OnEventTriggered(Event);
			}
		}

		#endregion
	}
}
