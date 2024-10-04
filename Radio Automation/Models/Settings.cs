using Catel.Data;
using Microsoft.VisualBasic.Logging;
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
			MqttBrokerPort = 1883;
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
		public static readonly IPropertyData LastPlaylistPathProperty = RegisterProperty<string>(nameof(LastPlaylistPath));

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
		public static readonly IPropertyData EventSchedulePathProperty = RegisterProperty<string>(nameof(LastEventSchedulePath));

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
		public static readonly IPropertyData ShufflePlaylistProperty = RegisterProperty<bool>(nameof(ShufflePlaylist));

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
		public static readonly IPropertyData VolumeProperty = RegisterProperty<float>(nameof(Volume));

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
		public static readonly IPropertyData PrimaryOutputDeviceProperty = RegisterProperty<Device>(nameof(PrimaryOutputDevice));

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
		public static readonly IPropertyData WUndergroundKeyProperty = RegisterProperty<string>(nameof(WUndergroundKey));

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
		public static readonly IPropertyData WUndergroundStationProperty = RegisterProperty<string>(nameof(WUndergroundStation));

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
		public static readonly IPropertyData CurrentSongPathProperty = RegisterProperty<string>(nameof(CurrentSongPath));

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
		public static readonly IPropertyData UseWUndergroundProperty = RegisterProperty<bool>(nameof(UseWUnderground));

		#endregion

		#region MqttBroker property

		/// <summary>
		/// Gets or sets the MqttBroker value.
		/// </summary>
		public string MqttBroker
		{
			get { return GetValue<string>(MqttBrokerProperty); }
			set { SetValue(MqttBrokerProperty, value); }
		}

		/// <summary>
		/// MqttBroker property data.
		/// </summary>
		public static readonly IPropertyData MqttBrokerProperty = RegisterProperty<string>(nameof(MqttBroker));

		#endregion

		#region MqttBrokerPort property

		/// <summary>
		/// Gets or sets the MqttBrokerPort value.
		/// </summary>
		public int MqttBrokerPort
		{
			get { return GetValue<int>(MqttBrokerPortProperty); }
			set { SetValue(MqttBrokerPortProperty, value); }
		}

		/// <summary>
		/// MqttBrokerPort property data.
		/// </summary>
		public static readonly IPropertyData MqttBrokerPortProperty = RegisterProperty<int>(nameof(MqttBrokerPort));

		#endregion
	}
}
