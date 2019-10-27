using Catel.Data;
using NAudioWrapper;

namespace Radio_Automation.Models
{
	public class Settings:ModelBase
	{
		public Settings()
		{
			Volume = 50;
			PrimaryOutputDevice = AudioPlayback.GetDefaultDevice();
			UseWUnderground = true;
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

		#region PrimaryOutputDevice property

		/// <summary>
		/// Gets or sets the PrimaryOutputDevice value.
		/// </summary>
		public Device PrimaryOutputDevice
		{
			get { return GetValue<Device>(PrimaryOutputDeviceProperty); }
			set { SetValue(PrimaryOutputDeviceProperty, value); }
		}

		/// <summary>
		/// PrimaryOutputDevice property data.
		/// </summary>
		public static readonly PropertyData PrimaryOutputDeviceProperty = RegisterProperty("PrimaryOutputDevice", typeof(Device));

		#endregion

		#region WUndergroundKey property

		/// <summary>
		/// Gets or sets the WUndergroundKey value.
		/// </summary>
		public string WUndergroundKey
		{
			get { return GetValue<string>(WUndergroundKeyProperty); }
			set { SetValue(WUndergroundKeyProperty, value); }
		}

		/// <summary>
		/// WUndergroundKey property data.
		/// </summary>
		public static readonly PropertyData WUndergroundKeyProperty = RegisterProperty("WUndergroundKey", typeof(string));

		#endregion

		#region WUndergroundStation property

		/// <summary>
		/// Gets or sets the WUndergroundStation value.
		/// </summary>
		public string WUndergroundStation
		{
			get { return GetValue<string>(WUndergroundStationProperty); }
			set { SetValue(WUndergroundStationProperty, value); }
		}

		/// <summary>
		/// WUndergroundStation property data.
		/// </summary>
		public static readonly PropertyData WUndergroundStationProperty = RegisterProperty("WUndergroundStation", typeof(string));

		#endregion

		#region CurrentSongPath property

		/// <summary>
		/// Gets or sets the CurrentSongPath value.
		/// </summary>
		public string CurrentSongPath
		{
			get { return GetValue<string>(CurrentSongPathProperty); }
			set { SetValue(CurrentSongPathProperty, value); }
		}

		/// <summary>
		/// CurrentSongPath property data.
		/// </summary>
		public static readonly PropertyData CurrentSongPathProperty = RegisterProperty("CurrentSongPath", typeof(string));

		#endregion

		#region UseWUnderground property

		/// <summary>
		/// Gets or sets the UseWUnderground value.
		/// </summary>
		public bool UseWUnderground
		{
			get { return GetValue<bool>(UseWUndergroundProperty); }
			set { SetValue(UseWUndergroundProperty, value); }
		}

		/// <summary>
		/// UseWUnderground property data.
		/// </summary>
		public static readonly PropertyData UseWUndergroundProperty = RegisterProperty("UseWUnderground", typeof(bool));

		#endregion
	}
}
