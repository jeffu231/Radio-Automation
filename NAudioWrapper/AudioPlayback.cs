using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Catel.Logging;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NAudioWrapper
{
    public class AudioPlayback : IDisposable, IMMNotificationClient
    {
	    private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private WasapiOut? _playbackDevice;
        private WaveStream? _fileStream;

        public event Action? PlaybackResumed;
        public event Action? PlaybackStopped;
        public event Action? PlaybackEnded;
		public event Action? PlaybackPaused;
		public event Action<VolumeEventArgs>? OnPreVolumeMeter;
		public event Action<VolumeEventArgs>? OnSteamVolume;
		/// <summary>
		/// Raised when the output stream died unexpectedly (device invalidated, removed, session change, etc.)
		/// rather than reaching the end of the track. The argument is the underlying error, if any.
		/// </summary>
		public event Action<Exception?>? PlaybackFaulted;

		public MMDevice? CurrentDevice { get; private set; }
		private string? _deviceId;
		private readonly string? _preferredDeviceId;
		private VolumeSampleProvider? _volumeProvider;
		private float _volume = 1f;
		private bool _notificationsRegistered;
		private static readonly MMDeviceEnumerator DeviceEnumerator = new MMDeviceEnumerator();

		public AudioPlayback():this(GetDefaultMMDevice()!)
		{

		}

		internal AudioPlayback(MMDevice device)
		{
			SetDevice(device);
			RegisterNotifications();
		}

		public AudioPlayback(Device device)
		{
			_preferredDeviceId = device.Id;
			SetDevice(ResolveDevice(_preferredDeviceId));
			RegisterNotifications();
		}

		private void SetDevice(MMDevice? device)
		{
			CurrentDevice = device;
			_deviceId = device is null ? null : SafeGetId(device);
		}

		private static string? SafeGetId(MMDevice device)
		{
			try
			{
				return device.ID;
			}
			catch (Exception e)
			{
				Log.Warning(e, "Unable to read the audio device id.");
				return null;
			}
		}

		/// <summary>
		/// Finds the preferred device if it is active, otherwise the current default render endpoint.
		/// </summary>
		private static MMDevice? ResolveDevice(string? preferredId)
		{
			if (!string.IsNullOrEmpty(preferredId))
			{
				try
				{
					var preferred = DeviceEnumerator.GetDevice(preferredId);
					if (preferred.State == DeviceState.Active)
					{
						return preferred;
					}

					Log.Warning($"Preferred audio device {preferred.FriendlyName} is {preferred.State}. Falling back to the default device.");
				}
				catch (Exception e)
				{
					Log.Warning(e, "Preferred audio device is unavailable. Falling back to the default device.");
				}
			}

			try
			{
				return GetDefaultMMDevice();
			}
			catch (Exception e)
			{
				Log.Error(e, "Unable to find a default audio device.");
				return null;
			}
		}

		private bool IsCurrentDeviceActive()
		{
			try
			{
				return CurrentDevice is { State: DeviceState.Active };
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Discards the current output stream and re-resolves the output device (preferred device first,
		/// then the default). Call after <see cref="PlaybackFaulted"/> and before loading again.
		/// </summary>
		/// <returns><c>true</c> if a usable device was found.</returns>
		public bool RefreshDevice()
		{
			ReleaseDevice();
			var device = ResolveDevice(_preferredDeviceId);
			if (device is null)
			{
				Log.Warning("No audio output device is currently available.");
				return false;
			}

			var newId = SafeGetId(device);
			if (newId != _deviceId)
			{
				Log.Info($"Audio output device changed from {_deviceId} to {device.FriendlyName} ({newId}).");
			}

			SetDevice(device);
			return true;
		}

		private void RegisterNotifications()
		{
			try
			{
				DeviceEnumerator.RegisterEndpointNotificationCallback(this);
				_notificationsRegistered = true;
			}
			catch (Exception e)
			{
				Log.Warning(e, "Unable to register for audio device notifications.");
			}
		}

		public static bool HasDefaultDevice()
		{
			var device = GetDefaultMMDevice();
			return device is not null;
		}

		public static Device GetDefaultDevice()
		{
			var mmDevice = GetDefaultMMDevice();
			return new Device(mmDevice!, true);
		}

		private static MMDevice? GetDefaultMMDevice()
		{
			var deviceEnumerator = new MMDeviceEnumerator();
			if (deviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
			{
				return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
			}

			return null;
		}

		public static List<Device> GetActiveDevices()
		{
			var deviceEnumerator = new MMDeviceEnumerator();
			var mmDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
			var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
			var devices = new List<Device>();
			foreach (var device in mmDevices)
			{
				devices.Add(new Device(device,device.ID == defaultDevice.ID));
			}

			return devices;
		}

		public string? DeviceId => _deviceId;

		/// <summary>
		/// Registers a call back for Device Events
		/// </summary>
		/// <param name="client">Object implementing IMMNotificationClient type casted as IMMNotificationClient interface</param>
		/// <returns></returns>
		public int RegisterEndpointNotificationCallback([In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
		{
			//DeviceEnum declared below
			return DeviceEnumerator.RegisterEndpointNotificationCallback(client);
		}

		/// <summary>
		/// UnRegisters a call back for Device Events
		/// </summary>
		/// <param name="client">Object implementing IMMNotificationClient type casted as IMMNotificationClient interface </param>
		/// <returns></returns>
		public int UnRegisterEndpointNotificationCallback([In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client)
		{
			//DeviceEnum declared below
			return DeviceEnumerator.UnregisterEndpointNotificationCallback(client);
		} 

		public bool Load(string fileName)
        {
            Stop();
            CloseFile();
            return EnsureDeviceForLoad() && OpenFile(fileName);
        }

		[SupportedOSPlatform("Windows7.0")]
		public bool Load(string[] files)
		{
			Stop();
			CloseFile();
			return EnsureDeviceForLoad() && LoadFiles(files);
		}

		private void CloseFile()
        {
	        try
	        {
		        _fileStream?.Dispose();
		        _fileStream = null;
	        }
	        catch (Exception e)
	        {
		        Log.Error(e, "Unable to close the file stream.");
	        }
        }

		[SupportedOSPlatform("Windows7.0")]
		private bool LoadFiles(string[] paths)
        {
	        var status = false;
	        try
	        {
		        List<AudioFileReader> readers = new List<AudioFileReader>(paths.Length);
		        foreach (var path in paths)
		        {
			        var reader = new AudioFileReader(path);
			        readers.Add(reader);
		        }
				
		        var merged = new ConcatenatingAudioFileReader(readers.ToArray());
		        _fileStream = merged;
				Initialize(merged);
				status = true;
	        }
	        catch (Exception e)
	        {
				Log.Error(e, $"An error occurred opening the file. {paths}");
				CloseFile();
			}

	        return status;
        }

        private bool OpenFile(string fileName)
        {
	        var status = false;
            try
            {
                var inputStream = new MediaFoundationReader(fileName);
               
				_fileStream = inputStream;
                var sp = _fileStream.WaveFormat.Channels == 1 ? new MonoToStereoSampleProvider(_fileStream.ToSampleProvider()):
                        _fileStream.ToSampleProvider();
                Initialize(sp);
                status = true;
            }
            catch (Exception e)
            {
	            Log.Error(e, $"An error occurred opening the file. {fileName}");
                CloseFile();
            }

            return status;
        }

        private void Initialize(ISampleProvider inputStream)
        {
	        _volumeProvider = new VolumeSampleProvider(inputStream);
			var postVolumeMeter = new MeteringSampleProvider(_volumeProvider);
			postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;
			_playbackDevice!.Init(postVolumeMeter);
        }

		private void PostVolumeMeter_StreamVolume(object? sender, StreamVolumeEventArgs e)
		{
			OnSteamVolume?.Invoke(new VolumeEventArgs(e.MaxSampleValues.Length > 0 ? e.MaxSampleValues[0] : 0, 
                e.MaxSampleValues.Length> 1?e.MaxSampleValues[1]: 0));
		}

		private void SampleChannelOnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
	        OnPreVolumeMeter?.Invoke(new VolumeEventArgs(e.MaxSampleValues[0], e.MaxSampleValues[1]));
        }

        /// <summary>
        /// <c>true</c> when the last <c>Load</c> returned false because no output device could be
        /// opened, as opposed to a problem with the file itself (missing, unreadable, bad format).
        /// </summary>
        public bool LastLoadFailedOnDevice { get; private set; }

        private bool EnsureDeviceForLoad()
        {
            LastLoadFailedOnDevice = !TryEnsureDeviceCreated();
            return !LastLoadFailedOnDevice;
        }

        private void EnsureDeviceCreated()
        {
            TryEnsureDeviceCreated();
        }

        private bool TryEnsureDeviceCreated()
        {
            try
            {
                if (_playbackDevice != null)
                {
                    return true;
                }

                if (!IsCurrentDeviceActive() && !RefreshDevice())
                {
                    return false;
                }

                CreateDevice();
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to create the audio output device.");
                ReleaseDevice();
                return false;
            }
        }

        private void CreateDevice()
        {
			_playbackDevice = new WasapiOut(CurrentDevice,AudioClientShareMode.Exclusive, false, 10);
			_playbackDevice.PlaybackStopped += PlaybackDeviceOnPlaybackStopped;
        }

        private void PlaybackDeviceOnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
	        if (e.Exception != null)
	        {
		        Log.Error(e.Exception, "Audio playback stopped unexpectedly.");
		        // Ignore faults from a device we have already replaced.
		        if (ReferenceEquals(sender, _playbackDevice))
		        {
			        ReleaseDevice();
			        PlaybackFaulted?.Invoke(e.Exception);
		        }

		        return;
	        }

	        DisposePlaybackDevice();
			PlaybackEnded?.Invoke();
        }

        public void Play()
        {
			EnsureDeviceCreated();
            if (_playbackDevice != null && _fileStream != null && _playbackDevice.PlaybackState != PlaybackState.Playing)
            {
	            _volumeProvider!.Volume = Volume;
                _playbackDevice.Play();
				PlaybackResumed?.Invoke();
            }
        }

        public void Pause()
        {
            _playbackDevice?.Pause();
			PlaybackPaused?.Invoke();
        }

        public void Stop()
        {
			_playbackDevice?.Stop();
			DisposePlaybackDevice();
            try
            {
                _fileStream?.Position = 0;
            }
            catch (Exception e)
            {
                // Non-seekable or broken sources (e.g. a dropped internet stream) can't be rewound.
                Log.Warning(e, "Unable to reset the stream position.");
            }
			PlaybackStopped?.Invoke();
        }

        public bool IsPlaying()
        {
	        return _playbackDevice?.PlaybackState == PlaybackState.Playing;
        }

        public bool IsPaused()
        {
	        return _playbackDevice?.PlaybackState == PlaybackState.Paused;
        }

        public bool IsStopped()
        {
	        return _playbackDevice?.PlaybackState == PlaybackState.Stopped;
        }

        public void TogglePlayPause()
        {
	        if (!TryEnsureDeviceCreated())
	        {
		        return;
	        }

	        if (_playbackDevice!.PlaybackState == PlaybackState.Playing)
	        {
		        Pause();
	        }
	        else
	        {
		        Play();
	        }
	        
        }

		public double Position
        {
	        get => _fileStream?.CurrentTime.TotalSeconds ?? 0;
	        set
	        {
		        _fileStream?.CurrentTime = TimeSpan.FromSeconds(value);
			}
		}

		public double Length => _fileStream?.TotalTime.TotalSeconds ?? 0;

		public float Volume
        {
	        get => _volume;

	        set
	        {
				EnsureDeviceCreated();
				_volume = Math.Min(1,Math.Max(0, value));
				_volumeProvider?.Volume = _volume;
	        }
        }
		
        public void Dispose()
        {
            if (_notificationsRegistered)
            {
                try
                {
                    DeviceEnumerator.UnregisterEndpointNotificationCallback(this);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Unable to unregister audio device notifications.");
                }

                _notificationsRegistered = false;
            }

            Stop();
            CloseFile();
            DisposePlaybackDevice();
            GC.SuppressFinalize(this);
        }

        private void DisposePlaybackDevice()
        {
	        _playbackDevice?.Dispose();
	        _playbackDevice = null;
		}

		/// <summary>
		/// Disposes the output device without letting it raise a late PlaybackStopped
		/// that would be mistaken for the end of a track.
		/// </summary>
		private void ReleaseDevice()
		{
			var device = _playbackDevice;
			_playbackDevice = null;
			if (device == null)
			{
				return;
			}

			try
			{
				device.PlaybackStopped -= PlaybackDeviceOnPlaybackStopped;
				device.Dispose();
			}
			catch (Exception e)
			{
				Log.Warning(e, "Error releasing the audio output device.");
			}
		}

        #region Implementation of IMMNotificationClient

        /// <inheritdoc />
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
	        Log.Info($"Audio device {deviceId} state changed to {newState}.");
	        if (deviceId == _deviceId && newState != DeviceState.Active && _playbackDevice != null)
	        {
		        PlaybackFaulted?.Invoke(null);
	        }
        }

        /// <inheritdoc />
        public void OnDeviceAdded(string pwstrDeviceId)
        {
	        Log.Info($"Audio device added: {pwstrDeviceId}.");
        }

        /// <inheritdoc />
        public void OnDeviceRemoved(string deviceId)
        {
	        Log.Info($"Audio device removed: {deviceId}.");
	        if (deviceId == _deviceId && _playbackDevice != null)
	        {
		        PlaybackFaulted?.Invoke(null);
	        }
        }

        /// <inheritdoc />
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
	        if (flow == DataFlow.Render && role == Role.Multimedia)
	        {
		        Log.Info($"Default audio render device changed to {defaultDeviceId}.");
	        }
        }

        /// <inheritdoc />
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
	        
        }

        #endregion
    }
}
