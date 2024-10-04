using Catel.Data;

namespace Radio_Automation.Models
{
	public class MqttExpression:ModelBase
	{
		#region Topic property

		/// <summary>
		/// Gets or sets the Topic value.
		/// </summary>
		public string Topic
		{
			get { return GetValue<string>(TopicProperty); }
			set { SetValue(TopicProperty, value); }
		}

		/// <summary>
		/// Topic property data.
		/// </summary>
		public static readonly IPropertyData TopicProperty = RegisterProperty<string>(nameof(Topic));

		#endregion

		#region Message property

		/// <summary>
		/// Gets or sets the Message value.
		/// </summary>
		public string Message
		{
			get { return GetValue<string>(MessageProperty); }
			set { SetValue(MessageProperty, value); }
		}

		/// <summary>
		/// Message property data.
		/// </summary>
		public static readonly IPropertyData MessageProperty = RegisterProperty<string>(nameof(Message));

		#endregion

		

	}
}
