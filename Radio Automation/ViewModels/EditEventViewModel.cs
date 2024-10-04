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
		public static readonly IPropertyData EventProperty = RegisterProperty<Event>(nameof(Event));

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
		public static readonly IPropertyData NameProperty = RegisterProperty<string>(nameof(Name));

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
		public static readonly IPropertyData StartDateTimeProperty = RegisterProperty<DateTime>(nameof(StartDateTime));

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
		public static readonly IPropertyData EndDateTimeProperty = RegisterProperty<DateTime>(nameof(EndDateTime));

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
		public static readonly IPropertyData EventTypeProperty = RegisterProperty<EventType>(nameof(EventType));

		#endregion

		#region Trigger property

		/// <summary>
		/// Gets or sets the Trigger value.
		/// </summary>
		[ViewModelToModel("Event")]
		public Trigger Trigger
		{
			get { return GetValue<Trigger>(TriggerProperty); }
			set { SetValue(TriggerProperty, value); }
		}

		/// <summary>
		/// Trigger property data.
		/// </summary>
		public static readonly IPropertyData TriggerProperty = RegisterProperty<Trigger>(nameof(Trigger));

		#endregion

		#region MqttExpression property

		/// <summary>
		/// Gets or sets the MqttExpression value.
		/// </summary>
		[ViewModelToModel("Event")]
		public MqttExpression MqttExpression
		{
			get { return GetValue<MqttExpression>(MqttExpressionProperty); }
			set { SetValue(MqttExpressionProperty, value); }
		}

		/// <summary>
		/// MqttExpression property data.
		/// </summary>
		public static readonly IPropertyData MqttExpressionProperty = RegisterProperty<MqttExpression>(nameof(MqttExpression));

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
		public static readonly IPropertyData DemandProperty = RegisterProperty<Demand>(nameof(Demand));

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
		public static readonly IPropertyData CronExpressionProperty = RegisterProperty<string>(nameof(CronExpression));

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
		public static readonly IPropertyData ExpiresProperty = RegisterProperty<bool>(nameof(Expires));

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
