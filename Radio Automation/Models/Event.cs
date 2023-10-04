using System;
using Catel.Data;

namespace Radio_Automation.Models
{
	public class Event:ModelBase
	{
		public Event()
		{
			Id = Guid.NewGuid();
			StartDateTime = DateTime.Now;
			EndDateTime = DateTime.Now.AddYears(1);
			Trigger = Trigger.Cron;
			MqttExpression = new();
		}

		public Event(EventType eventType):this()
		{
			EventType = eventType;
		}

		#region Id property

		/// <summary>
		/// Gets or sets the Id value.
		/// </summary>
		public Guid Id
		{
			get { return GetValue<Guid>(IdProperty); }
			set { SetValue(IdProperty, value); }
		}

		/// <summary>
		/// Id property data.
		/// </summary>
		public static readonly PropertyData IdProperty = RegisterProperty("Id", typeof(Guid));

		#endregion

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
		public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string));

		#endregion

		#region Trigger property

		/// <summary>
		/// Gets or sets the Trigger value.
		/// </summary>
		public Trigger Trigger
		{
			get { return GetValue<Trigger>(TriggerProperty); }
			set { SetValue(TriggerProperty, value); } 
		}

		/// <summary>
		/// Trigger property data.
		/// </summary>
		public static readonly PropertyData TriggerProperty = RegisterProperty("Trigger", typeof(Trigger));

		#endregion


		#region EventType property

		/// <summary>
		/// Gets or sets the EventType value.
		/// </summary>
		public EventType EventType
		{
			get { return GetValue<EventType>(EventTypeProperty); }
			set { SetValue(EventTypeProperty, value); }
		}

		/// <summary>
		/// EventType property data.
		/// </summary>
		public static readonly PropertyData EventTypeProperty = RegisterProperty("EventType", typeof(EventType));

		#endregion
		
		#region Demand property

		/// <summary>
		/// Gets or sets the Demand value.
		/// </summary>
		public Demand Demand
		{
			get { return GetValue<Demand>(DemandProperty); }
			set { SetValue(DemandProperty, value); }
		}

		/// <summary>
		/// Demand property data.
		/// </summary>
		public static readonly PropertyData DemandProperty = RegisterProperty("Demand", typeof(Demand));

		#endregion

		#region CronExpression property

		/// <summary>
		/// Gets or sets the CronExpression value.
		/// </summary>
		public string CronExpression
		{
			get { return GetValue<string>(CronExpressionProperty); }
			set { SetValue(CronExpressionProperty, value); }
		}

		/// <summary>
		/// CronExpression property data.
		/// </summary>
		public static readonly PropertyData CronExpressionProperty = RegisterProperty("CronExpression", typeof(string));

		#endregion

		#region MqttExpression property

		/// <summary>
		/// Gets or sets the MqttExpression value.
		/// </summary>
		public MqttExpression MqttExpression
		{
			get { return GetValue<MqttExpression>(MqttExpressionProperty); }
			set { SetValue(MqttExpressionProperty, value); }
		}

		/// <summary>
		/// MqttExpression property data.
		/// </summary>
		public static readonly PropertyData MqttExpressionProperty = RegisterProperty("MqttExpression", typeof(MqttExpression));

		#endregion


		#region StartDateTime property

		/// <summary>
		/// Gets or sets the StartDateTime value.
		/// </summary>
		public DateTime StartDateTime
		{
			get { return GetValue<DateTime>(StartDateTimeProperty); }
			set { SetValue(StartDateTimeProperty, value); }
		}

		/// <summary>
		/// StartDateTime property data.
		/// </summary>
		public static readonly PropertyData StartDateTimeProperty = RegisterProperty("StartDateTime", typeof(DateTime));

		#endregion

		#region Expires property

		/// <summary>
		/// Gets or sets the Expires value.
		/// </summary>
		public bool Expires
		{
			get { return GetValue<bool>(ExpiresProperty); }
			set { SetValue(ExpiresProperty, value); }
		}

		/// <summary>
		/// Expires property data.
		/// </summary>
		public static readonly PropertyData ExpiresProperty = RegisterProperty("Expires", typeof(bool));

		#endregion

		#region EndDateTime property

		/// <summary>
		/// Gets or sets the EndDateTime value.
		/// </summary>
		public DateTime EndDateTime
		{
			get { return GetValue<DateTime>(EndDateTimeProperty); }
			set { SetValue(EndDateTimeProperty, value); }
		}

		/// <summary>
		/// EndDateTime property data.
		/// </summary>
		public static readonly PropertyData EndDateTimeProperty = RegisterProperty("EndDateTime", typeof(DateTime));

		#endregion

		#region Enabled property

		/// <summary>
		/// Gets or sets the Enabled value.
		/// </summary>
		public bool Enabled
		{
			get { return GetValue<bool>(EnabledProperty); }
			set { SetValue(EnabledProperty, value); }
		}

		/// <summary>
		/// Enabled property data.
		/// </summary>
		public static readonly PropertyData EnabledProperty = RegisterProperty("Enabled", typeof(bool));

		#endregion

		#region File property

		/// <summary>
		/// Gets or sets the File value.
		/// </summary>
		public string File
		{
			get { return GetValue<string>(FileProperty); }
			set { SetValue(FileProperty, value); }
		}

		/// <summary>
		/// File property data.
		/// </summary>
		public static readonly PropertyData FileProperty = RegisterProperty("File", typeof(string));

		#endregion

	}

	public enum EventType
	{
		Play,
		Stop,
		Pause,
		Temperature,
		Time,
		Humidity,
		Playlist,
		SingleTrack
	}

	public enum Trigger
	{
		Cron,
		Mqtt
	}

	public enum Demand
	{
		Immediate,
		Delayed
	}
}
