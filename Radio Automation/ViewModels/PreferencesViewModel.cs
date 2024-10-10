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
		private readonly IOpenFileService _openFileService;
		private readonly ISelectDirectoryService _selectDirectoryService;

		public PreferencesViewModel(Settings settings, IOpenFileService saveFileService, ISelectDirectoryService selectDirectoryService)
		{
			_openFileService = saveFileService;
			_selectDirectoryService = selectDirectoryService;
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
		public static readonly IPropertyData SettingsProperty = RegisterProperty<Settings>(nameof(Settings));

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
		public static readonly IPropertyData DevicesProperty = RegisterProperty<List<Device>>(nameof(Devices));

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
		public static readonly IPropertyData PrimaryOutputDeviceProperty = RegisterProperty<Device>(nameof(PrimaryOutputDevice));

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
		public static readonly IPropertyData UseWUndergroundProperty = RegisterProperty<bool>(nameof(UseWUnderground));

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
		public static readonly IPropertyData WUndergroundKeyProperty = RegisterProperty<string>(nameof(WUndergroundKey));

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
		public static readonly IPropertyData WUndergroundStationProperty = RegisterProperty<string>(nameof(WUndergroundStation));

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
		public static readonly IPropertyData CurrentSongPathProperty = RegisterProperty<string>(nameof(CurrentSongPath));

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
		public static readonly IPropertyData MqttBrokerProperty = RegisterProperty<string>(nameof(MqttBroker));

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
		public static readonly IPropertyData MqttBrokerPortProperty = RegisterProperty<int>(nameof(MqttBrokerPort));

		#endregion

		#region TimeTempPath property

		/// <summary>
		/// Gets or sets the TimeTempFilePath value.
		/// </summary>
		[ViewModelToModel("Settings")]
		public string TimeTempFilePath
		{
			get { return GetValue<string>(TimeTempFilePathProperty); }
			set { SetValue(TimeTempFilePathProperty, value); }
		}

		/// <summary>
		/// TimeTempFilePath property data.
		/// </summary>
		public static readonly IPropertyData TimeTempFilePathProperty = RegisterProperty<string>(nameof(TimeTempFilePath));

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
			var dofc = new DetermineOpenFileContext
			{
				CheckFileExists = true
			};
			var result = await _openFileService.DetermineFileAsync(dofc);
			if (result.Result)
			{
				CurrentSongPath = result.FileName;
			}
		}

		#endregion

		#region SelectTimeTempPath command

		private TaskCommand _selectTimeTempPathCommand;

		/// <summary>
		/// Gets the SelectTimeTempPath command.
		/// </summary>
		public TaskCommand SelectTimeTempFilePathCommand
		{
			get { return _selectTimeTempPathCommand ?? (_selectTimeTempPathCommand = new TaskCommand(SelectTimeTempPathAsync)); }
		}

		/// <summary>
		/// Method to invoke when the SelectTimeTempPath command is executed.
		/// </summary>
		private async Task SelectTimeTempPathAsync()
		{
			var dsfc = new DetermineDirectoryContext();
			var result = await _selectDirectoryService.DetermineDirectoryAsync(dsfc);
			if (result.Result)
			{
				TimeTempFilePath = result.DirectoryName;
			}
		}

		#endregion
	}
}
