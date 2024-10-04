using System.Collections.ObjectModel;
using Catel.Data;

namespace Radio_Automation.Models
{
	public class EventSchedule:ModelBase
	{
		public EventSchedule()
		{
			Events = new ObservableCollection<Event>();
		}
		#region Name property

		/// <summary>
		/// Gets or sets the Name value.
		/// </summary>
		public string Name
		{
			get { return GetValue<string>(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		/// <summary>
		/// Name property data.
		/// </summary>
		public static readonly IPropertyData NameProperty = RegisterProperty<string>(nameof(Name));

		#endregion

		#region Events property

		/// <summary>
		/// Gets or sets the Events value.
		/// </summary>
		public ObservableCollection<Event> Events
		{
			get { return GetValue<ObservableCollection<Event>>(EventsProperty); }
			set { SetValue(EventsProperty, value); }
		}

		/// <summary>
		/// Events property data.
		/// </summary>
		public static readonly IPropertyData EventsProperty = RegisterProperty<ObservableCollection<Event>>(nameof(Events));

		#endregion

	}
}
