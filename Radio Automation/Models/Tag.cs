using Catel.Data;

namespace Radio_Automation.Models
{
	public class Tag: ModelBase
	{
		public Tag()
		{
			
		}

		public Tag(string name)
		{
			Name = name;
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
	}
}
