using Catel.Data;

namespace Radio_Automation.Models
{
	public class Settings:ModelBase
	{
		public Settings()
		{
			Volume = 100;
		}

		#region LastPlaylistPath property

		/// <summary>
		/// Gets or sets the LastPlaylistPath value.
		/// </summary>
		public string LastPlaylistPath
		{
			get { return GetValue<string>(LastPlaylistPathProperty); }
			set { SetValue(LastPlaylistPathProperty, value); }
		}

		/// <summary>
		/// LastPlaylistPath property data.
		/// </summary>
		public static readonly PropertyData LastPlaylistPathProperty = RegisterProperty("LastPlaylistPath", typeof(string));

		#endregion

		#region EventSchedulePath property

		/// <summary>
		/// Gets or sets the EventSchedulePath value.
		/// </summary>
		public string LastEventSchedulePath
		{
			get { return GetValue<string>(EventSchedulePathProperty); }
			set { SetValue(EventSchedulePathProperty, value); }
		}

		/// <summary>
		/// EventSchedulePath property data.
		/// </summary>
		public static readonly PropertyData EventSchedulePathProperty = RegisterProperty("LastEventSchedulePath", typeof(string));

		#endregion

		#region ShufflePlaylist property

		/// <summary>
		/// Gets or sets the ShufflePlaylist value.
		/// </summary>
		public bool ShufflePlaylist
		{
			get { return GetValue<bool>(ShufflePlaylistProperty); }
			set { SetValue(ShufflePlaylistProperty, value); }
		}

		/// <summary>
		/// ShufflePlaylist property data.
		/// </summary>
		public static readonly PropertyData ShufflePlaylistProperty = RegisterProperty("ShufflePlaylist", typeof(bool));

		#endregion

		#region Volume property

		/// <summary>
		/// Gets or sets the Volume value.
		/// </summary>
		public float Volume
		{
			get { return GetValue<float>(VolumeProperty); }
			set { SetValue(VolumeProperty, value); }
		}

		/// <summary>
		/// Volume property data.
		/// </summary>
		public static readonly PropertyData VolumeProperty = RegisterProperty("Volume", typeof(float));

		#endregion
	}
}
