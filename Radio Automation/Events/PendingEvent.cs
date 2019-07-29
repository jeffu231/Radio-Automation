using System;
using Radio_Automation.Models;

namespace Radio_Automation.Events
{
	public class PendingEvent: IEquatable<PendingEvent>
	{
		public PendingEvent(Event e, DateTime nextScheduledTime)
		{
			Event = e;
			NextScheduledTime = nextScheduledTime;
		}

		public Event Event { get;}

		public DateTime NextScheduledTime { get;}

		#region Equality members

		/// <inheritdoc />
		public bool Equals(PendingEvent other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Event, other.Event) && NextScheduledTime.Equals(other.NextScheduledTime);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((PendingEvent) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((Event != null ? Event.GetHashCode() : 0) * 397) ^ NextScheduledTime.GetHashCode();
			}
		}

		#endregion
	}
}
