using System;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using Radio_Automation.Models;

namespace Radio_Automation.ViewModels
{
	public class EditEventViewModel:ViewModelBase
	{
		public EditEventViewModel(Event evt)
		{
			Event = evt;
			Title = $"Edit {evt.Name}";
		}

		#region Overrides of ViewModelBase

		/// <inheritdoc />
		public override string Title { get; protected set; }

		#endregion

		#region Event model property

		/// <summary>
		/// Gets or sets the Event value.
		/// </summary>
		[Model]
		public Event Event
		{
			get { return GetValue<Event>(EventProperty); }
			private set { SetValue(EventProperty, value); }
		}

		/// <summary>
		/// Event property data.
		/// </summary>
		public static readonly PropertyData EventProperty = RegisterProperty("Event", typeof(Event));

		#endregion

		#region Name property

		/// <summary>
		/// Gets or sets the Name value.
		/// </summary>
		[ViewModelToModel("Event")]
		public string Name
		{
			get { return GetValue<string>(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		/// <summary>
		/// Name property data.
		/// </summary>
		public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), null);

		#endregion

		#region StartDateTime property

		/// <summary>
		/// Gets or sets the StartDateTime value.
		/// </summary>
		[ViewModelToModel("Event")]
		public DateTime StartDateTime
		{
			get { return GetValue<DateTime>(StartDateTimeProperty); }
			set { SetValue(StartDateTimeProperty, value); }
		}

		/// <summary>
		/// StartDateTime property data.
		/// </summary>
		public static readonly PropertyData StartDateTimeProperty = RegisterProperty("StartDateTime", typeof(DateTime), null);

		#endregion

		#region EndDateTime property

		/// <summary>
		/// Gets or sets the EndDateTime value.
		/// </summary>
		[ViewModelToModel("Event")]
		public DateTime EndDateTime
		{
			get { return GetValue<DateTime>(EndDateTimeProperty); }
			set { SetValue(EndDateTimeProperty, value); }
		}

		/// <summary>
		/// EndDateTime property data.
		/// </summary>
		public static readonly PropertyData EndDateTimeProperty = RegisterProperty("EndDateTime", typeof(DateTime), null);

		#endregion

		#region EventType property

		/// <summary>
		/// Gets or sets the EventType value.
		/// </summary>
		[ViewModelToModel("Event")]
		public EventType EventType
		{
			get { return GetValue<EventType>(EventTypeProperty); }
			set { SetValue(EventTypeProperty, value); }
		}

		/// <summary>
		/// EventType property data.
		/// </summary>
		public static readonly PropertyData EventTypeProperty = RegisterProperty("EventType", typeof(EventType), null);

		#endregion

		#region Demand property

		/// <summary>
		/// Gets or sets the Demand value.
		/// </summary>
		[ViewModelToModel("Event")]
		public Demand Demand
		{
			get { return GetValue<Demand>(DemandProperty); }
			set { SetValue(DemandProperty, value); }
		}

		/// <summary>
		/// Demand property data.
		/// </summary>
		public static readonly PropertyData DemandProperty = RegisterProperty("Demand", typeof(Demand), null);

		#endregion

		#region CronExpression property

		/// <summary>
		/// Gets or sets the CronExpression value.
		/// </summary>
		[ViewModelToModel("Event")]
		public string CronExpression
		{
			get { return GetValue<string>(CronExpressionProperty); }
			set { SetValue(CronExpressionProperty, value); }
		}

		/// <summary>
		/// CronExpression property data.
		/// </summary>
		public static readonly PropertyData CronExpressionProperty = RegisterProperty("CronExpression", typeof(string), null);

		#endregion

		#region Expires property

		/// <summary>
		/// Gets or sets the Expires value.
		/// </summary>
		[ViewModelToModel("Event")]
		public bool Expires
		{
			get { return GetValue<bool>(ExpiresProperty); }
			set { SetValue(ExpiresProperty, value); }
		}

		/// <summary>
		/// Expires property data.
		/// </summary>
		public static readonly PropertyData ExpiresProperty = RegisterProperty("Expires", typeof(bool), null);

		#endregion

		public DateTime DefaultDateTime => DateTime.Now;

		#region Ok command

		private TaskCommand _okCommand;

		/// <summary>
		/// Gets the Ok command.
		/// </summary>
		public TaskCommand OkCommand
		{
			get { return _okCommand ?? (_okCommand = new TaskCommand(OkAsync, CanOk)); }
		}

		/// <summary>
		/// Method to invoke when the Ok command is executed.
		/// </summary>
		private async Task OkAsync()
		{
			//await SaveViewModelAsync();
			await CloseViewModelAsync(true);
		}

		/// <summary>
		/// Method to check whether the Ok command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanOk()
		{
			return true;
		}

		#endregion

	}
}
