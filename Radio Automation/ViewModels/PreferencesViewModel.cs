using System.Collections.Generic;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using NAudioWrapper;
using Radio_Automation.Models;

namespace Radio_Automation.ViewModels
{
	public class PreferencesViewModel: ViewModelBase
	{

		public PreferencesViewModel(Settings settings)
		{
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
