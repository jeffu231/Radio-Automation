using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Windows;
using NAudioWrapper;
using Radio_Automation.Events;
using Radio_Automation.Extensions;
using Radio_Automation.Messaging;
using Radio_Automation.Models;
using Radio_Automation.Services;
using Weather;

namespace Radio_Automation.ViewModels
{
	public class MainWindowViewModel: ViewModelBase
	{
		//Services injected
		private readonly ISelectDirectoryService _selectDirectoryService;
		private readonly IAudioTrackParserService _audioTrackParserService;
		private readonly IOpenFileService _openFileService;
		private readonly ISaveFileService _saveFileService;
		private readonly IPersistenceService _persistenceService;
		private readonly IBusyIndicatorService _busyIndicatorService;

		//Logging
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();
		
		private AudioPlayback _audioPlayer;
		private PlaybackState _playbackState = PlaybackState.Stopped;
		private readonly DispatcherTimer _playPositionTimer;
		private readonly DispatcherTimer _pendingEventTimer;
		private readonly DispatcherTimer _weatherUpdateTimer;
		private readonly EventScheduler _eventScheduler;
		private readonly MqttEventListener _mqttEventListener;
		private EventSchedule _eventSchedule;
		private readonly Queue<Event> _eventQueue = new Queue<Event>();
		private Settings _settings = new Settings();
		private readonly WunderGround _wg;
        private readonly IDispatcherService _dispatcherService;

		public MainWindowViewModel(ISelectDirectoryService selectDirectoryService, IOpenFileService openFileService, 
			IAudioTrackParserService audioTrackParserService, IPersistenceService persistenceService, IBusyIndicatorService busyIndicatorService, 
			ISaveFileService saveFileService, IDispatcherService dispatcherService)
		{
			_wg = new WunderGround();
			CurrentTrackIndex = -1;
			_selectDirectoryService = selectDirectoryService;
			_audioTrackParserService = audioTrackParserService;
			_openFileService = openFileService;
			_saveFileService = saveFileService;
			_persistenceService = persistenceService;
			_busyIndicatorService = busyIndicatorService;
			_dispatcherService = dispatcherService;

			PendingEvents = new ObservableCollection<PendingEvent>();
			SelectedTracks = new ObservableCollection<Track>();

			UpdatePlayPauseStates();

			Clock = new Clock();
			TrackEndTime = DateTime.MinValue;
			_playPositionTimer = new DispatcherTimer{Interval = TimeSpan.FromSeconds(1)};
			_playPositionTimer.Tick += PlayPositionTimer_Tick;
			_playPositionTimer.IsEnabled = true;

			_pendingEventTimer = new DispatcherTimer{Interval = TimeSpan.FromSeconds(1)};
			_pendingEventTimer.Tick += PendingEventTimer_Tick;
			_pendingEventTimer.IsEnabled = true;
			_pendingEventTimer.Start();

			_weatherUpdateTimer = new DispatcherTimer{Interval = TimeSpan.FromMinutes(10)};
			_weatherUpdateTimer.Tick += WeatherUpdateTimer_Tick;
			_weatherUpdateTimer.IsEnabled = true;
			_weatherUpdateTimer.Start();

			Playlist = new Playlist();

			EventBus.EventTriggered += EventTriggered;
			_eventScheduler = new EventScheduler();
			_mqttEventListener = new MqttEventListener();
			_eventSchedule = new EventSchedule();
			
		}

		#region Overrides of ViewModelBase

		/// <inheritdoc />
		protected override async Task InitializeAsync()
		{
			await RestoreSettingsAsync();

			ConfigurePrimaryAudioPlayer();

			if (!string.IsNullOrEmpty(_settings.LastPlaylistPath))
			{
				Playlist = await _persistenceService.LoadPlaylistAsync(_settings.LastPlaylistPath);
			}

			await UpdateWeatherData();

			_eventSchedule = await _persistenceService.LoadEventScheduleAsync(_settings.LastEventSchedulePath);

			_eventScheduler.LoadSchedule(_eventSchedule);

			await _mqttEventListener.Connect(_settings);

			await _mqttEventListener.LoadSchedule(_eventSchedule);
			
			PlaylistMessage.Register(this, PlaylistAction);

		}

		private void ConfigurePrimaryAudioPlayer()
		{
			_audioPlayer?.Dispose();

			_audioPlayer = new AudioPlayback(_settings.PrimaryOutputDevice);
			_audioPlayer.PlaybackPaused += PlaybackPaused;
			_audioPlayer.PlaybackResumed += PlaybackResumed;
			_audioPlayer.PlaybackEnded += PlaybackEnded;
			_audioPlayer.OnSteamVolume += AudioPlayer_OnStreamVolume;

			VolumeChangedCommand.Execute();
			Volume = _settings.Volume;
		}

		/// <inheritdoc />
		public override string Title => @"Radio Automation";

        #endregion

        private void PlaylistAction(PlaylistMessage message)
        {
            if (message.Data.PlaylistAction == Messaging.PlaylistAction.Remove)
            {
	            foreach (var selectedTrack in SelectedTracks.ToList())
	            {
					Playlist.Tracks.Remove(selectedTrack);
					Log.Info($"Track {selectedTrack.FormattedName} removed from playlist.");
				}
            }
        }

        private void EventTriggered(Event e)
		{
			if (_playbackState != PlaybackState.Playing && e.Demand == Demand.Delayed)
			{
				PendingEvents.Remove(PendingEvents.FirstOrDefault(x => x.Event == e));
				// Don't play delayed events if the player is stopped.
				return;
			}

			if (_playbackState != PlaybackState.Stopped && e.Demand == Demand.Delayed)
			{
				_eventQueue.Enqueue(e);
			}
			else
            {
                _dispatcherService.BeginInvoke(() => ExecuteEvent(e));
			}
		}

		private enum PlaybackState
		{
			Playing, Stopped, Paused
		}

		#region Playlist property

		/// <summary>
		/// Gets or sets the Playlist value.
		/// </summary>
		public Playlist Playlist
		{
			get { return GetValue<Playlist>(PlaylistProperty); }
			set
			{
				SetValue(PlaylistProperty, value);
				if (Playlist.Tracks.Count > 0)
				{
					CurrentTrackIndex = 0;
				}
			}
		}

		/// <summary>
		/// Playlist property data.
		/// </summary>
		public static readonly IPropertyData PlaylistProperty = RegisterProperty<Playlist>(nameof(Playlist));

		#endregion

		#region PendingEvents property

		/// <summary>
		/// Gets or sets the PendingEvents value.
		/// </summary>
		public ObservableCollection<PendingEvent> PendingEvents
		{
			get { return GetValue<ObservableCollection<PendingEvent>>(PendingEventsProperty); }
			set { SetValue(PendingEventsProperty, value); }
		}

		/// <summary>
		/// PendingEvents property data.
		/// </summary>
		public static readonly IPropertyData PendingEventsProperty = RegisterProperty<ObservableCollection<PendingEvent>>(nameof(PendingEvents));

		#endregion

		#region PlayPauseImageSource property

		/// <summary>
		/// Gets or sets the PlayPauseImageSource value.
		/// </summary>
		public string PlayPauseImageSource
		{
			get { return GetValue<string>(PlayPauseImageSourceProperty); }
			set { SetValue(PlayPauseImageSourceProperty, value); }
		}

		/// <summary>
		/// PlayPauseImageSource property data.
		/// </summary>
		public static readonly IPropertyData PlayPauseImageSourceProperty = RegisterProperty<string>(nameof(PlayPauseImageSource));

		#endregion

		#region Clock property

		/// <summary>
		/// Gets or sets the Clock value.
		/// </summary>
		public Clock Clock
		{
			get { return GetValue<Clock>(ClockProperty); }
			set { SetValue(ClockProperty, value); }
		}

		/// <summary>
		/// Clock property data.
		/// </summary>
		public static readonly IPropertyData ClockProperty = RegisterProperty<Clock>(nameof(Clock));

		#endregion

		#region NextTrack property

		/// <summary>
		/// Gets or sets the NextTrack value.
		/// </summary>
		public Track NextTrack
		{
			get { return GetValue<Track>(NextTrackProperty); }
			set { SetValue(NextTrackProperty, value); }
		}

		/// <summary>
		/// NextTrack property data.
		/// </summary>
		public static readonly IPropertyData NextTrackProperty = RegisterProperty<Track>(nameof(NextTrack));

		#endregion

		#region CurrentTrack property

		/// <summary>
		/// Gets or sets the CurrentTrack value.
		/// </summary>
		public Track CurrentTrack
		{
			get { return GetValue<Track>(CurrentTrackProperty); }
			set { SetValue(CurrentTrackProperty, value); }
		}

		/// <summary>
		/// CurrentTrack property data.
		/// </summary>
		public static readonly IPropertyData CurrentTrackProperty = RegisterProperty<Track>(nameof(CurrentTrack));

		#endregion

		#region CurrentTrackIndex property

		/// <summary>
		/// Gets or sets the CurrentTrackIndex value.
		/// </summary>
		public int CurrentTrackIndex
		{
			get { return GetValue<int>(CurrentTrackIndexProperty); }
			set { SetValue(CurrentTrackIndexProperty, value); }
		}

		/// <summary>
		/// CurrentTrackIndex property data.
		/// </summary>
		public static readonly IPropertyData CurrentTrackIndexProperty = RegisterProperty<int>(nameof(CurrentTrackIndex));

		#endregion

		#region Position property

		/// <summary>
		/// Gets or sets the Position value.
		/// </summary>
		public TimeSpan Position
		{
			get { return GetValue<TimeSpan>(PositionProperty); }
			set { SetValue(PositionProperty, value); }
		}

		/// <summary>
		/// Position property data.
		/// </summary>
		public static readonly IPropertyData PositionProperty = RegisterProperty<TimeSpan>(nameof(Position));

		#endregion

		#region RemainingTime property

		/// <summary>
		/// Gets or sets the RemainingTime value.
		/// </summary>
		public TimeSpan RemainingTime
		{
			get { return GetValue<TimeSpan>(RemainingTimeProperty); }
			set { SetValue(RemainingTimeProperty, value); }
		}

		/// <summary>
		/// RemainingTime property data.
		/// </summary>
		public static readonly IPropertyData RemainingTimeProperty = RegisterProperty<TimeSpan>(nameof(RemainingTime));

		#endregion

		#region TrackEndTime property

		/// <summary>
		/// Gets or sets the TrackEndTime value.
		/// </summary>
		public DateTime TrackEndTime
		{
			get { return GetValue<DateTime>(TrackEndTimeProperty); }
			set { SetValue(TrackEndTimeProperty, value); }
		}

		/// <summary>
		/// TrackEndTime property data.
		/// </summary>
		public static readonly IPropertyData TrackEndTimeProperty = RegisterProperty<DateTime>(nameof(TrackEndTime));

		#endregion

		#region Temperature property

		/// <summary>
		/// Gets or sets the Temperature value.
		/// </summary>
		public int Temperature
		{
			get { return GetValue<int>(TemperatureProperty); }
			set { SetValue(TemperatureProperty, value); }
		}

		/// <summary>
		/// Temperature property data.
		/// </summary>
		public static readonly IPropertyData TemperatureProperty = RegisterProperty<int>(nameof(Temperature));

		#endregion

		#region Humidity property

		/// <summary>
		/// Gets or sets the Humidity value.
		/// </summary>
		public int Humidity
		{
			get { return GetValue<int>(HumidityProperty); }
			set { SetValue(HumidityProperty, value); }
		}

		/// <summary>
		/// Humidity property data.
		/// </summary>
		public static readonly IPropertyData HumidityProperty = RegisterProperty<int>(nameof(Humidity));

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

		#region LeftLevel property

		/// <summary>
		/// Gets or sets the LeftLevel value.
		/// </summary>
		public int LeftLevel
		{
			get { return GetValue<int>(LeftLevelProperty); }
			set { SetValue(LeftLevelProperty, value); }
		}

		/// <summary>
		/// LeftLevel property data.
		/// </summary>
		public static readonly IPropertyData LeftLevelProperty = RegisterProperty<int>(nameof(LeftLevel));

		#endregion

		#region RightLevel property

		/// <summary>
		/// Gets or sets the RightLevel value.
		/// </summary>
		public int RightLevel
		{
			get { return GetValue<int>(RightLevelProperty); }
			set { SetValue(RightLevelProperty, value); }
		}

		/// <summary>
		/// RightLevel property data.
		/// </summary>
		public static readonly IPropertyData RightLevelProperty = RegisterProperty<int>(nameof(RightLevel));

		#endregion

		#region SelectedTracks property

		/// <summary>
		/// Gets or sets the SelectedTracks value.
		/// </summary>
		public ObservableCollection<Track> SelectedTracks
		{
			get { return GetValue<ObservableCollection<Track>>(SelectedTracksProperty); }
			set { SetValue(SelectedTracksProperty, value); }
		}

		/// <summary>
		/// SelectedTracks property data.
		/// </summary>
		public static readonly IPropertyData SelectedTracksProperty = RegisterProperty<ObservableCollection<Track>>(nameof(SelectedTracks));

		#endregion

		#region Overrides of ViewModelBase

		/// <inheritdoc />
		protected override async Task CloseAsync()
		{
			var success = SaveSettingsAsync();
			success.Wait(TimeSpan.FromSeconds(10));
			StopPlaying();
			_audioPlayer?.Dispose();
			await base.CloseAsync();
		}

		#endregion

		#region NewPlaylist command

		private Command _newPlaylistCommand;

		/// <summary>
		/// Gets the NewPlaylist command.
		/// </summary>
		public Command NewPlaylistCommand
		{
			get { return _newPlaylistCommand ?? (_newPlaylistCommand = new Command(NewPlaylist)); }
		}

		/// <summary>
		/// Method to invoke when the NewPlaylist command is executed.
		/// </summary>
		private void NewPlaylist()
		{
			Playlist = new Playlist();
		}

		#endregion

		#region OpenPlaylist command

		private TaskCommand _openPlaylistCommand;

		/// <summary>
		/// Gets the OpenPlaylist command.
		/// </summary>
		public TaskCommand OpenPlaylistCommand
		{
			get { return _openPlaylistCommand ?? (_openPlaylistCommand = new TaskCommand(OpenPlaylist)); }
		}

		/// <summary>
		/// Method to invoke when the OpenPlaylist command is executed.
		/// </summary>
		private async Task OpenPlaylist()
		{
			var determineOpenFileContext = new DetermineOpenFileContext
			{
				IsMultiSelect = false,
				CheckFileExists = true,
				Title = @"Import Zara Playlist",
				Filter = "Playlist (*.rpl) | *.rpl"
			};

			var result = await _openFileService.DetermineFileAsync(determineOpenFileContext);


			if (result.Result)
			{
				_busyIndicatorService.Show();
				Playlist = await _persistenceService.LoadPlaylistAsync(result.FileName);
				_settings.LastPlaylistPath = result.FileName;
				_busyIndicatorService.Hide();
			}
		}

		#endregion

		#region ImportZaraPlaylist command

		private TaskCommand _importZaraPlaylistCommand;

		/// <summary>
		/// Gets the ImportZaraPlaylist command.
		/// </summary>
		public TaskCommand ImportZaraPlaylistCommand
		{
			get { return _importZaraPlaylistCommand ?? (_importZaraPlaylistCommand = new TaskCommand(ImportZaraPlaylist)); }
		}

		/// <summary>
		/// Method to invoke when the ImportZaraPlaylist command is executed.
		/// </summary>
		private async Task ImportZaraPlaylist()
		{
			var dofc = new DetermineOpenFileContext
			{
				IsMultiSelect = false,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				CheckFileExists = true,
				Title = @"Import Zara Playlist",
				Filter = "ZaraRadio List (*.lst) | *.lst"
			};

			var result = await _openFileService.DetermineFileAsync(dofc);
			if (result.Result)
			{
				_busyIndicatorService.Show();
				Playlist = await _persistenceService.ImportZaraPlaylistAsync(result.FileName);
				_busyIndicatorService.Hide();
			}
		}

		#endregion

		#region ImportM3UPlaylist command

		private TaskCommand _importM3UPlaylistCommand;

		/// <summary>
		/// Gets the ImportM3UPlaylist command.
		/// </summary>
		public TaskCommand ImportM3UPlaylistCommand
		{
			get { return _importM3UPlaylistCommand ?? (_importM3UPlaylistCommand = new TaskCommand(ImportM3UPlaylistAsync)); }
		}

		/// <summary>
		/// Method to invoke when the ImportM3UPlaylist command is executed.
		/// </summary>
		private async Task ImportM3UPlaylistAsync()
		{
			var dofc = new DetermineOpenFileContext
			{
				IsMultiSelect = false,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				CheckFileExists = true,
				Title = @"Import M3U Playlist",
				Filter = "M3U Play List (*.m3u) | *.m3u"
			};

			var result = await _openFileService.DetermineFileAsync(dofc);
			if (result.Result)
			{
				_busyIndicatorService.Show();
				Playlist = await _persistenceService.ImportM3UPlaylistAsync(result.FileName);
				_busyIndicatorService.Hide();
			}
		}

		#endregion

		#region SavePlaylist command

		private TaskCommand _savePlaylistCommand;

		/// <summary>
		/// Gets the SavePlaylist command.
		/// </summary>
		public TaskCommand SavePlaylistCommand
		{
			get { return _savePlaylistCommand ?? (_savePlaylistCommand = new TaskCommand(SavePlaylist)); }
		}

		/// <summary>
		/// Method to invoke when the SavePlaylist command is executed.
		/// </summary>
		private async Task SavePlaylist()
		{
			if (File.Exists(_settings.LastPlaylistPath))
			{
				_busyIndicatorService.Show();
				await _persistenceService.SavePlaylistAsync(Playlist, _settings.LastPlaylistPath);
				_busyIndicatorService.Hide();
			}
			else
			{
				await SavePlaylistAs();
			}
		}

		#endregion

		#region SavePlaylistAs command

		private TaskCommand _savePlaylistAsCommand;

		/// <summary>
		/// Gets the SavePlaylistAs command.
		/// </summary>
		public TaskCommand SavePlaylistAsCommand
		{
			get { return _savePlaylistAsCommand ?? (_savePlaylistAsCommand = new TaskCommand(SavePlaylistAs)); }
		}

		/// <summary>
		/// Method to invoke when the SavePlaylistAs command is executed.
		/// </summary>
		private async Task SavePlaylistAs()
		{
			var dependencyResolver = this.GetDependencyResolver();
			var saveFileService = dependencyResolver.Resolve<ISaveFileService>();

			var dsfc = new DetermineSaveFileContext
			{
				Filter = "Playlist|*.rpl",
				Title = @"Save Playlist"
			};

			var result = await saveFileService.DetermineFileAsync(dsfc);
			if (result.Result)
			{
				_busyIndicatorService.Show();
				await _persistenceService.SavePlaylistAsync(Playlist, result.FileName);
				_settings.LastPlaylistPath = result.FileName;
				_busyIndicatorService.Hide();
			}
		}

		#endregion

		#region Exit command

		private Command<Window> _exitCommand;

		/// <summary>
		/// Gets the Exit command.
		/// </summary>
		public Command<Window> ExitCommand => _exitCommand ?? (_exitCommand = new Command<Window>(Exit));

		/// <summary>
		/// Method to invoke when the Exit command is executed.
		/// </summary>
		private void Exit(Window window)
		{
			window?.Close();
		}

		#endregion

		#region EditEventSchedule command

		private TaskCommand _editEventScheduleCommand;

		/// <summary>
		/// Gets the EditEventSchedule command.
		/// </summary>
		public TaskCommand EditEventScheduleCommand
		{
			get { return _editEventScheduleCommand ?? (_editEventScheduleCommand = new TaskCommand(EditEventScheduleAsync)); }
		}

		/// <summary>
		/// Method to invoke when the EditEventSchedule command is executed.
		/// </summary>
		private async Task EditEventScheduleAsync()
		{
			var viewModel = new EventScheduleViewModel(_eventSchedule);
			viewModel.Path = _settings.LastEventSchedulePath;
			var dependencyResolver = this.GetDependencyResolver();
			var uiVisualizerService = dependencyResolver.Resolve<IUIVisualizerService>();
			await uiVisualizerService.ShowDialogAsync(viewModel);
			_settings.LastEventSchedulePath = viewModel.Path;
			_eventSchedule = viewModel.EventSchedule;
			PendingEvents.Clear();
			await _mqttEventListener.Connect(_settings);
			_eventScheduler.LoadSchedule(_eventSchedule);
			await _mqttEventListener.LoadSchedule(_eventSchedule);
		}

		#endregion

		#region AddInternetStream command

		private Command _addInternetStreamCommand;

		/// <summary>
		/// Gets the AddInternetStream command.
		/// </summary>
		public Command AddInternetStreamCommand
		{
			get { return _addInternetStreamCommand ?? (_addInternetStreamCommand = new Command(AddInternetStream)); }
		}

		/// <summary>
		/// Method to invoke when the AddInternetStream command is executed.
		/// </summary>
		private void AddInternetStream()
		{
			var stream = new Track("HalloweenRadio.net", "http://stream.zenolive.com/3yaezmm1h3quv");
			Playlist.Tracks.Add(stream);
		}

		#endregion

		#region AddFile command

		private Command _addFileCommand;

		/// <summary>
		/// Gets the AddFile command.
		/// </summary>
		public Command AddFileCommand
		{
			get { return _addFileCommand ?? (_addFileCommand = new Command(AddFile, CanAddFile)); }
		}

		/// <summary>
		/// Method to invoke when the AddFile command is executed.
		/// </summary>
		private async void AddFile()
		{
			var dofc = new DetermineOpenFileContext
			{
				IsMultiSelect = true,
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				Filter = "Audio files (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac"
			};

			var result = await _openFileService.DetermineFileAsync(dofc);
			if (result.Result)
			{
				foreach (var fileName in result.FileNames)
				{
					AddFileToPlaylist(fileName);
				}
			}
		}

		/// <summary>
		/// Method to check whether the AddFile command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanAddFile()
		{
			return _playbackState == PlaybackState.Stopped;
		}

		#endregion

		#region AddFolder command

		private Command _addFolderCommand;

		/// <summary>
		/// Gets the AddFolder command.
		/// </summary>
		public Command AddFolderCommand
		{
			get { return _addFolderCommand ?? (_addFolderCommand = new Command(AddFolder, CanAddFolder)); }
		}

		/// <summary>
		/// Method to invoke when the AddFolder command is executed.
		/// </summary>
		private async void AddFolder()
		{
			var ddc = new DetermineDirectoryContext
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				ShowNewFolderButton = false
			};

			var result = await _selectDirectoryService.DetermineDirectoryAsync(ddc);
			if (result.Result)
			{
				AddFolderToPlaylist(result.DirectoryName);
			}

		}

		/// <summary>
		/// Method to check whether the AddFolder command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanAddFolder()
		{
			return _playbackState == PlaybackState.Stopped;
		}

		#endregion

		#region Help command

		private Command _helpCommand;

		/// <summary>
		/// Gets the Help command.
		/// </summary>
		public Command HelpCommand
		{
			get { return _helpCommand ?? (_helpCommand = new Command(Help)); }
		}

		/// <summary>
		/// Method to invoke when the Help command is executed.
		/// </summary>
		private void Help()
		{
			// TODO: Handle command logic here
		}

		#endregion

		#region Preferences command

		private TaskCommand _preferencesCommand;

		/// <summary>
		/// Gets the Preferences command.
		/// </summary>
		public TaskCommand PreferencesCommand
		{
			get { return _preferencesCommand ?? (_preferencesCommand = new TaskCommand(PreferencesAsync)); }
		}

		/// <summary>
		/// Method to invoke when the Preferences command is executed.
		/// </summary>
		private async Task PreferencesAsync()
		{
		
			var dependencyResolver = this.GetDependencyResolver();
			var uiVisualizerService = dependencyResolver.Resolve<IUIVisualizerService>();
			var openFileService = dependencyResolver.Resolve<IOpenFileService>();
			var selectDirectoryService = dependencyResolver.Resolve<ISelectDirectoryService>();
			PreferencesViewModel viewModel = new PreferencesViewModel(_settings, openFileService, selectDirectoryService);
			var ok = await uiVisualizerService.ShowDialogAsync(viewModel);
			if (ok.DialogResult.HasValue && ok.DialogResult.Value)
			{
				await SaveSettingsAsync();
				if (_settings.PrimaryOutputDevice.Id != AudioPlayback.DeviceId)
				{
					ConfigurePrimaryAudioPlayer();
				}
			}
		}

		#endregion

		#region StartPlayback command

		private Command _playPauseCommand;

		/// <summary>
		/// Gets the StartPlayback command.
		/// </summary>
		public Command PlayPauseCommand
		{
			get { return _playPauseCommand ?? (_playPauseCommand = new Command(PlayPause, CanPlayPause)); }
		}

		/// <summary>
		/// Method to invoke when the StartPlayback command is executed.
		/// </summary>
		private void PlayPause()
		{

			switch (_playbackState)
			{
				case PlaybackState.Paused:
					_playbackState = PlaybackState.Playing;
					_audioPlayer.TogglePlayPause();
					TrackEndTime = DateTime.Now.Add(CurrentTrack.Duration - Position);
					break;
				case PlaybackState.Playing:
					_playbackState = PlaybackState.Paused;
					_audioPlayer.TogglePlayPause();
					break;
				default:
					StartPlay();
					break;

			}
			
		}

		private void StartPlay()
		{
			UpdateCurrentNextTracks();
			if (CurrentTrack != null)
			{
				TrackPlayingMessage.SendWith(new TrackInfoData(CurrentTrack, true));
				Log.Info($"Playing Track {CurrentTrack.FormattedName}");
				_playbackState = PlaybackState.Playing;
				var success = _audioPlayer.Load(CurrentTrack.Path);
				if(success)
				{
					RemainingTime = CurrentTrack.Duration;
					TrackEndTime = DateTime.Now.Add(CurrentTrack.Duration);
					_audioPlayer.TogglePlayPause();
				}
				else
				{
					PlaybackEnded();
				}
			}
		}

		/// <summary>
		/// Method to check whether the StartPlayback command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanPlayPause()
		{
			return true;
		}

		#endregion

		#region StopPlayback command

		private Command _stopPlaybackCommand;

		/// <summary>
		/// Gets the StopPlayback command.
		/// </summary>
		public Command StopPlaybackCommand
		{
			get { return _stopPlaybackCommand ?? (_stopPlaybackCommand = new Command(StopPlayback, CanStopPlayback)); }
		}

		/// <summary>
		/// Method to invoke when the StopPlayback command is executed.
		/// </summary>
		private void StopPlayback()
		{
			_playbackState = PlaybackState.Stopped;
			_audioPlayer?.Stop();
		}

		/// <summary>
		/// Method to check whether the StopPlayback command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanStopPlayback()
		{
			return _playbackState == PlaybackState.Playing || _playbackState == PlaybackState.Paused;
		}

		#endregion

		#region ForwardToEnd command

		private Command _forwardToEndCommand;

		/// <summary>
		/// Gets the ForwardToEnd command.
		/// </summary>
		public Command ForwardToEndCommand
		{
			get { return _forwardToEndCommand ?? (_forwardToEndCommand = new Command(ForwardToEnd, CanForwardToEnd)); }
		}

		/// <summary>
		/// Method to invoke when the ForwardToEnd command is executed.
		/// </summary>
		private void ForwardToEnd()
		{
			_audioPlayer?.Stop();
		}

		/// <summary>
		/// Method to check whether the ForwardToEnd command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanForwardToEnd()
		{
			return _playbackState == PlaybackState.Playing;
		}

		#endregion

		#region RewindToStart command

		private Command _rewindToStartCommand;

		/// <summary>
		/// Gets the RewindToStart command.
		/// </summary>
		public Command RewindToStartCommand
		{
			get { return _rewindToStartCommand ?? (_rewindToStartCommand = new Command(RewindToStart, CanRewindToStart)); }
		}

		/// <summary>
		/// Method to invoke when the RewindToStart command is executed.
		/// </summary>
		private void RewindToStart()
		{
			_audioPlayer.Position = 0; // set position to zero
		}

		/// <summary>
		/// Method to check whether the RewindToStart command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanRewindToStart()
		{
			return _playbackState == PlaybackState.Playing;
		}

		#endregion

		#region PlayTemperature command

		private TaskCommand _playTemperatureCommand;

		/// <summary>
		/// Gets the PlayTemperature command.
		/// </summary>
		public TaskCommand PlayTemperatureCommand
		{
			get { return _playTemperatureCommand ?? (_playTemperatureCommand = new TaskCommand(PlayTemperatureAsync)); }
		}

		/// <summary>
		/// Method to invoke when the PlayTemperature command is executed.
		/// </summary>
		private async Task PlayTemperatureAsync()
		{
			await PlayTemperature();
		}

		#endregion

		#region Shuffle command

		private Command _shuffleCommand;

		/// <summary>
		/// Gets the Shuffle command.
		/// </summary>
		public Command ShuffleCommand
		{
			get { return _shuffleCommand ?? (_shuffleCommand = new Command(Shuffle, CanShuffle)); }
		}

		/// <summary>
		/// Method to invoke when the Shuffle command is executed.
		/// </summary>
		private void Shuffle()
		{
			Playlist.Tracks.Shuffle();
			//_eventSchedule.UpNext();
			//PlayTime(new AudioPlayback());
		}

		/// <summary>
		/// Method to check whether the Shuffle command can be executed.
		/// </summary>
		/// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
		private bool CanShuffle()
		{
			return _playbackState == PlaybackState.Stopped;
		}

		#endregion

        #region SortTrackArtist command

        private Command _sortTrackArtistCommand;

        /// <summary>
        /// Gets the SortTrackArtist command.
        /// </summary>
        public Command SortTrackArtistCommand
        {
            get
            {
                return _sortTrackArtistCommand ??= new Command(OnSortTrackArtistExecute, OnSortTrackArtistCanExecute);
            }
        }

        /// <summary>
        /// Method to invoke when the SortTrackArtist command is executed.
        /// </summary>
        private void OnSortTrackArtistExecute()
        {
            Playlist.Tracks.SortTrackArtist();
        }

        /// <summary>
        /// Method to check whether the SortTrackArtist command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSortTrackArtistCanExecute()
        {
            return _playbackState == PlaybackState.Stopped;
        }

        #endregion

        #region SortTrackName command

        private Command _sortTrackNameCommand;

        /// <summary>
        /// Gets the SortTrackName command.
        /// </summary>
        public Command SortTrackNameCommand
        {
            get
            {
                return _sortTrackNameCommand ??= new Command(OnSortTrackNameExecute, OnSortTrackNameCanExecute);
            }
        }

        /// <summary>
        /// Method to invoke when the SortTrackName command is executed.
        /// </summary>
        private void OnSortTrackNameExecute()
        {
            Playlist.Tracks.SortTrackName();
        }

        /// <summary>
        /// Method to check whether the SortTrackName command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnSortTrackNameCanExecute()
        {
            return _playbackState == PlaybackState.Stopped;
        }

        #endregion

		#region VolumeChanged command

		private Command _volumeChangedCommand;

		/// <summary>
		/// Gets the VolumeChanged command.
		/// </summary>
		public Command VolumeChangedCommand
		{
			get { return _volumeChangedCommand ?? (_volumeChangedCommand = new Command(VolumeChanged)); }
		}

		/// <summary>
		/// Method to invoke when the VolumeChanged command is executed.
		/// </summary>
		private void VolumeChanged()
		{
			_audioPlayer.Volume = Volume/100;
		}

		#endregion

		#region ShowAbout command

		private TaskCommand _showAboutCommand;

		/// <summary>
		/// Gets the ShowAbout command.
		/// </summary>
		public TaskCommand ShowAboutCommand
		{
			get { return _showAboutCommand ?? (_showAboutCommand = new TaskCommand(ShowAboutAsync)); }
		}

		/// <summary>
		/// Method to invoke when the ShowAbout command is executed.
		/// </summary>
		private async Task ShowAboutAsync()
		{
			await Task.CompletedTask;
			//if (ApplicationDeployment.IsNetworkDeployed)
			//{
			//	var version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(4);
			//	await _messageService.ShowInformationAsync(version, "Version");
			//}
		}

		#endregion


		#region Player Events

		private void PlaybackEnded()
		{
			TrackEndTime = DateTime.MinValue;
			TrackPlayingMessage.SendWith(new TrackInfoData(CurrentTrack, false));
			if (_playbackState == PlaybackState.Stopped)
			{
				StopPlaying();
				return;
			}

			if (_eventQueue.Any())
			{
				CurrentTrack = null;
				ExecuteEvent(_eventQueue.Dequeue());
			}
			else
			{
				if (++CurrentTrackIndex >= Playlist.Tracks.Count)
				{
					CurrentTrackIndex = 0;
					Shuffle();
				}

				StartPlay();
			}
		}

		private void StopPlaying()
		{
			_playbackState = PlaybackState.Stopped;
			_playPositionTimer.Stop();
			UpdatePlayPauseStates();

			UpdateCommandStates();
			Position = TimeSpan.Zero;
			RemainingTime = TimeSpan.Zero;
			CurrentTrack = null;
			NextTrack = null;
		}

		private void PlaybackResumed()
		{
			_playPositionTimer.Start();
			UpdatePlayPauseStates();
		}

		private void PlaybackPaused()
		{
			_playPositionTimer.Stop();
			UpdatePlayPauseStates();
		}

		private void PlayPositionTimer_Tick(object sender, EventArgs e)
		{
			if(CurrentTrack != null)
			{

				try
				{
					var p = TimeSpan.FromSeconds(_audioPlayer.Position);
					if (CurrentTrack.Duration == TimeSpan.Zero)
					{
						TrackEndTime = Clock.CurrentDateTime;
					}
					else
					{
						RemainingTime = CurrentTrack.Duration - p;
					}

					Position = p;
				}
				catch (Exception exception)
				{
					Log.Error(exception, "Unable to update play position.");
				}
			}
		}

		private void PendingEventTimer_Tick(object sender, EventArgs e)
		{
			var pending = _eventScheduler.UpNext(5);
			PendingEvents.AddRange(pending.Except(PendingEvents));
		}

		private async void WeatherUpdateTimer_Tick(object sender, EventArgs e)
		{
			await UpdateWeatherData();
		}


		private void AudioPlayer_OnStreamVolume(VolumeEventArgs e)
		{
			LeftLevel = (int)(e.Left * 100);
			RightLevel = (int)(e.Right * 100);
		}

		#endregion

		private void UpdatePlayPauseStates()
		{
			switch (_playbackState)
			{
				case PlaybackState.Playing:
					PlayPauseImageSource = "/Resources;component/Icons/control_pause_blue.png";
					break;
				case PlaybackState.Paused:
				case PlaybackState.Stopped:
					LeftLevel = 0;
					RightLevel = 0;
					PlayPauseImageSource = "/Resources;component/Icons/control_play_blue.png";
					break;
			}
		}

		private void UpdateCurrentNextTracks()
		{
			if (CurrentTrackIndex >= 0)
			{
				CurrentTrack = Playlist.Tracks[CurrentTrackIndex];
				NextTrack = CurrentTrackIndex + 1 < Playlist.Tracks.Count ? Playlist.Tracks[CurrentTrackIndex + 1] : null;
			}
			else
			{
				CurrentTrack = null;
				NextTrack = null;
			}
			
			UpdateCurrentSong();
		}

		private async Task<bool> SaveSettingsAsync()
		{
			_settings.Volume = Volume;
			return await _persistenceService.SaveSettingsAsync(_settings);
		}

		private async Task RestoreSettingsAsync()
		{
			_settings = await _persistenceService.LoadSettingsAsync();
		}

		private void LoadPlaylistFromFolder(string path, bool recurse=false)
		{
			var tracks = _audioTrackParserService.RetrieveAudioTracksInPath(path, recurse);
			Playlist = new Playlist(tracks);
		}

		private void AddFolderToPlaylist(string path, bool recurse = false)
		{
			var tracks = _audioTrackParserService.RetrieveAudioTracksInPath(path, recurse);
			Playlist.Tracks.AddItems(tracks);
		}

		private void AddFileToPlaylist(string filePath)
		{
			var track = _audioTrackParserService.RetrieveAudioTrackFromFile(filePath);
			Playlist.Tracks.Add(track);
		}

		protected void UpdateCommandStates()
		{
			var viewModelBase = this as ViewModelBase;
			var commandManager = viewModelBase.GetViewModelCommandManager();
			commandManager.InvalidateCommands();
		}

		private async void ExecuteEvent(Event e)
		{
			PendingEvents.Remove(PendingEvents.FirstOrDefault(x => x.Event == e));

			Log.Info($"Executing pending event {e.Trigger} - {e.EventType}: State {_playbackState}");

			switch (e.EventType)
			{
				case EventType.Time:
					await PlayTime();
					break;
				case EventType.Temperature:
					var status = await PlayTemperature();
					if (status == false)
					{
						PlaybackEnded();
					}
					break;
				case EventType.Stop:
					StopPlayback();
					break;
				case EventType.Play:
					if (_playbackState == PlaybackState.Stopped || _playbackState == PlaybackState.Paused)
					{
						PlayPause();
					}
					break;
			}
		}

		private async Task UpdateWeatherData()
		{
			if (_settings.UseWUnderground)
			{
				try
				{
					var obs = await _wg.GetObservationAsync(_settings.WUndergroundStation, _settings.WUndergroundKey);
					Temperature = (int)Math.Round(obs.Temp);
					Humidity = (int)Math.Round(obs.Humidity);
				}
				catch (Exception e)
				{
					Log.Error(e, "An error occurred requesting a weather update. Reading may be stale.");
				}
				
			}
		}

		private void UpdateCurrentSong()
		{
			if (Directory.Exists(Path.GetDirectoryName(_settings.CurrentSongPath)))
			{
				var currentSong = CurrentTrack != null ? CurrentTrack.FormattedName : "Nothing Playing";
				try
				{
					var tryCount = 0;
					using (FileStream fs = new FileStream(_settings.CurrentSongPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
					{
						
						while (!fs.CanWrite || tryCount > 10)
						{
							tryCount++;
							Thread.Sleep(500);
						}
						// If we get here, the file is not in use by another process and we have permissions.
					}
					if (tryCount < 10)
					{
						File.WriteAllLines(_settings.CurrentSongPath, new[] { currentSong });
					}
					else
					{
						Log.Error("Unable to write current song file after multiple attempts. It may be in use by another process.");
					}
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Unable to write current song file.");
				}
			}
			else if (!string.IsNullOrEmpty(_settings.CurrentSongPath))
			{
				Log.Error("Specified current song directory does not exist.");
			}
		}

		private async Task<bool> PlayTemperature()
		{
			var player = _audioPlayer;
			var success = false;
			if (player.IsPaused() || player.IsPlaying())
			{
				player = new AudioPlayback(_settings.PrimaryOutputDevice);
				player.PlaybackEnded += () =>
				{
					FadeVolumeUp();
					player.Dispose();
				};
				player.Volume = _audioPlayer.Volume;
				FadeVolumeDown();
			}
			var format = "000";
			var tempFile = Temperature < 0 ? $"TMPN{Temperature.ToString(format)}.mp3" : $"TMP{Temperature.ToString(format)}.mp3";
			var temperaturePath = Path.Combine($"{_settings.TimeTempFilePath}\\Temperature", tempFile);
			if (File.Exists(temperaturePath))
			{
				player.Load(temperaturePath);
				player.TogglePlayPause();
				success = true;
			}
			else
			{
				Log.Error($"Temperature file does not exist: {temperaturePath}");
			}

			return await Task.FromResult(success);
		}

		[SupportedOSPlatform("Windows7.0")]
		private async Task<bool> PlayTime()
		{
			var player = _audioPlayer;
			if (player.IsPaused() || player.IsPlaying())
			{
				player = new AudioPlayback(_settings.PrimaryOutputDevice);
				player.PlaybackEnded += () =>
				{
					FadeVolumeUp();
					player.Dispose();
				};
				player.Volume = _audioPlayer.Volume;
				FadeVolumeDown();
			}
			var time = DateTime.Now;
			var hourSuffix = time.Minute == 0 ? @"_O" : string.Empty;
			var hourPath = Path.Combine($"{_settings.TimeTempFilePath}\\Time",
					$"HRS{time:HH}{hourSuffix}.mp3");

			var status = false;

			if (time.Minute > 0)
			{
				var minutePath = Path.Combine($"{_settings.TimeTempFilePath}\\Time", $"MIN{time:mm}.mp3");
				status = player.Load(new[] { hourPath, minutePath });
			}
			else
			{
				status = player.Load(hourPath);
			}

			if (status)
			{
				player.TogglePlayPause();
			}
			else
			{
				return await Task.FromResult(false);
			}
			
			return await Task.FromResult(true);
		}

		private void FadeVolumeDown()
		{
			for (float i = _audioPlayer.Volume - .1f; i > 0; i = i - .1f)
			{
				_audioPlayer.Volume = i;
				Thread.Sleep(25);
			}

			_audioPlayer.Volume = 0;
		}

		private void FadeVolumeUp()
		{
			for (float i = 0; i < Volume; i = i + .1f)
			{
				_audioPlayer.Volume = i;
				Thread.Sleep(25);
			}

			_audioPlayer.Volume = Volume;
		}
	}
}
