using Catel.IoC;
using Catel.Logging;
using Radio_Automation.Services;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Radio_Automation
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		//Logging
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

		#region Overrides of Application

		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			RegisterGlobalExceptionHandling();
			var serviceLocator = ServiceLocator.Default;//this.GetServiceLocator();
			serviceLocator.RegisterType<IAudioTrackParserService, AudioTrackParserService>();
			serviceLocator.RegisterType<IPersistenceService, PersistenceService>();
			serviceLocator.RegisterType<IMqttPublisherService, MqttPublisherService>();

			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
				TypeInfoResolver = new DefaultJsonTypeInfoResolver
				{
					//This ignores properties in the Catel ModelBase that are marked with XmlIgnore so 
					//they will not be serialized by the Json serializer. They are ignored in Catels serializer, but we 
					//want to use the System.TextJson serializer.
					Modifiers = { IgnoreXmlIgnoredProperties }
				},
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};
			serviceLocator.RegisterInstance(options);
		}

		#endregion

		private void RegisterGlobalExceptionHandling()
	    {
	        // this is the line you really want 
	        AppDomain.CurrentDomain.UnhandledException += 
	            (sender, args) => CurrentDomainOnUnhandledException(args);

	        // optional: hooking up some more handlers
	        // remember that you need to hook up additional handlers when 
	        // logging from other dispatchers, schedulers, or applications

	        Dispatcher.UnhandledException += 
	            (sender, args) => DispatcherOnUnhandledException(args);

	        Current.DispatcherUnhandledException +=
	            (sender, args) => CurrentOnDispatcherUnhandledException(args);

	        TaskScheduler.UnobservedTaskException += 
	            (sender, args) => TaskSchedulerOnUnobservedTaskException(args);
	    }

	    private static void TaskSchedulerOnUnobservedTaskException(UnobservedTaskExceptionEventArgs args)
	    {
	        Log.Error(args.Exception, args.Exception.Message);
	        args.SetObserved();
	    }

	    private static void CurrentOnDispatcherUnhandledException(DispatcherUnhandledExceptionEventArgs args)
	    {
	        Log.Error(args.Exception, args.Exception.Message);
	        // args.Handled = true;
	    }

	    private static void DispatcherOnUnhandledException(DispatcherUnhandledExceptionEventArgs args)
	    {
	        Log.Error(args.Exception, args.Exception.Message);
	        // args.Handled = true;
	    }

	    private static void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs args)
	    {
	        var exception = args.ExceptionObject as Exception;
	        var terminatingMessage = args.IsTerminating ? " The application is terminating." : string.Empty;
	        var exceptionMessage = exception?.Message ?? "An unmanaged exception occured.";
	        var message = string.Concat(exceptionMessage, terminatingMessage);
	        Log.Error(exception, message);
	    }

		public static Action<JsonTypeInfo> IgnoreXmlIgnoredProperties { get; } = typeInfo =>
		{
			if (typeInfo.Kind != JsonTypeInfoKind.Object)
				return;

			foreach (var property in typeInfo.Properties)
			{
				// Check if the property has the XmlIgnoreAttribute
				if (property.AttributeProvider?.IsDefined(typeof(XmlIgnoreAttribute), inherit: false) == true)
				{
					// Set the ShouldSerialize predicate to always return false (ignore)
					property.ShouldSerialize = (obj, val) => false;
				}
			}
		};
	}
}
