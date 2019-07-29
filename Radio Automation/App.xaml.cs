using System.Windows;
using Catel.IoC;
using Catel.Runtime.Serialization.Json;
using Radio_Automation.Services;

namespace Radio_Automation
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			var serviceLocator = ServiceLocator.Default;//this.GetServiceLocator();
			serviceLocator.RegisterType<IAudioTrackParserService, AudioTrackParserService>();
			serviceLocator.RegisterType<IPersistenceService, PersistenceService>();
			serviceLocator.RegisterType<IJsonSerializer, JsonSerializer>();
		}

		
	}
}
