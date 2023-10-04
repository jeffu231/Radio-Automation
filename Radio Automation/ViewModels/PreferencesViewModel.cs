using System.Collections.Generic;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using NAudioWrapper;
using Radio_Automation.Models;

namespace Radio_Automation.ViewModels
{
	public class PreferencesViewModel: ViewModelBase
	{
		private readonly ISaveFileService _saveFileService;

		public PreferencesViewModel(Settings settings, ISaveFileService saveFileService)
		{
			_saveFileService = saveFileService;
			Devices = AudioPlayback.GetActiveDevices();
			Settings = settings; 
			Title = "Preferences";
		}

		#region Settings model property

		/// <summary>
		/// Gets or sets the Settings value.
		/// </summary>
		[Model]
		public Settings Settings
		{
			get { return GetValue<Settings>(SettingsProperty); }
			private set { SetValue(SettingsProperty, value); }
		}

		/// <summary>
		/// Settings property data.
		/// </summary>
		public static readonly PropertyData SettingsProperty = RegisterProperty("Settings", typeof(Settings));

		#endregion
		
		#region Devices property

		/// <summary>
		/// Gets or sets the Devices value.
		/// </summary>
		public List<Device> Devices
		{
			get { return GetValue<List<Device>>(DevicesProperty); }
			set { SetValue(DevicesProperty, value); }
		}

		/// <summary>
		/// Devices property data.
		/// </summary>
		public static readonly PropertyData DevicesProperty = RegisterProperty("Devices", typeof(List<Device>));

		#endregion

		#region PrimaryOutputDevice property

		/// <summary>
		/// Gets or sets the PrimaryOutputDevice value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public Device PrimaryOutputDevice
		{
			get { return GetValue<Device>(PrimaryOutputDeviceProperty); }
			set { SetValue(PrimaryOutputDeviceProperty, value); }
		}

		/// <summary>
		/// PrimaryOutputDevice property data.
		/// </summary>
		public static readonly PropertyData PrimaryOutputDeviceProperty = RegisterProperty("PrimaryOutputDevice", typeof(Device), null);

		#endregion

		#region UseWUnderground property

		/// <summary>
		/// Gets or sets the UseWUnderground value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public bool UseWUnderground
		{
			get { return GetValue<bool>(UseWUndergroundProperty); }
			set { SetValue(UseWUndergroundProperty, value); }
		}

		/// <summary>
		/// UseWUnderground property data.
		/// </summary>
		public static readonly PropertyData UseWUndergroundProperty = RegisterProperty("UseWUnderground", typeof(bool), null);

		#endregion

		#region WUndergroundKey property

		/// <summary>
		/// Gets or sets the WUndergroundKey value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public string WUndergroundKey
		{
			get { return GetValue<string>(WUndergroundKeyProperty); }
			set { SetValue(WUndergroundKeyProperty, value); }
		}

		/// <summary>
		/// WUndergroundKey property data.
		/// </summary>
		public static readonly PropertyData WUndergroundKeyProperty = RegisterProperty("WUndergroundKey", typeof(string), null);

		#endregion

		#region WUndergroundStation property

		/// <summary>
		/// Gets or sets the WUndergroundStation value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public string WUndergroundStation
		{
			get { return GetValue<string>(WUndergroundStationProperty); }
			set { SetValue(WUndergroundStationProperty, value); }
		}

		/// <summary>
		/// WUndergroundStation property data.
		/// </summary>
		public static readonly PropertyData WUndergroundStationProperty = RegisterProperty("WUndergroundStation", typeof(string), null);

		#endregion

		#region CurrentSongPath property

		/// <summary>
		/// Gets or sets the CurrentSongPath value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public string CurrentSongPath
		{
			get { return GetValue<string>(CurrentSongPathProperty); }
			set { SetValue(CurrentSongPathProperty, value); }
		}

		/// <summary>
		/// CurrentSongPath property data.
		/// </summary>
		public static readonly PropertyData CurrentSongPathProperty = RegisterProperty("CurrentSongPath", typeof(string), null);

		#endregion

		#region MqttBroker property

		/// <summary>
		/// Gets or sets the MqttBroker value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public string MqttBroker
		{
			get { return GetValue<string>(MqttBrokerProperty); }
			set { SetValue(MqttBrokerProperty, value); }
		}

		/// <summary>
		/// MqttBroker property data.
		/// </summary>
		public static readonly PropertyData MqttBrokerProperty = RegisterProperty("MqttBroker", typeof(string), null);

		#endregion

		#region MqttBrokerPort property

		/// <summary>
		/// Gets or sets the MqttBrokerPort value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public int MqttBrokerPort
		{
			get { return GetValue<int>(MqttBrokerPortProperty); }
			set { SetValue(MqttBrokerPortProperty, value); }
		}

		/// <summary>
		/// MqttBrokerPort property data.
		/// </summary>
		public static readonly PropertyData MqttBrokerPortProperty = RegisterProperty("MqttBrokerPort", typeof(object), null);

		#endregion
		
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

		#region SelectCurrentSongPath command

		private TaskCommand _selectCurrentSongPathCommand;

		/// <summary>
		/// Gets the SelectCurrentSongPath command.
		/// </summary>
		public TaskCommand SelectCurrentSongPathCommand
		{
			get { return _selectCurrentSongPathCommand ?? (_selectCurrentSongPathCommand = new TaskCommand(SelectCurrentSongPathAsync)); }
		}

		/// <summary>
		/// Method to invoke when the SelectCurrentSongPath command is executed.
		/// </summary>
		private async Task SelectCurrentSongPathAsync()
		{
			var dsfc = new DetermineSaveFileContext
			{
				CheckFileExists = true
			};
			var result = await _saveFileService.DetermineFileAsync(dsfc);
			if (result.Result)
			{
				CurrentSongPath = result.FileName;
			}
		}

		#endregion
	}
}
