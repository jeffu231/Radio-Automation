using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;
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

        private IWavePlayer _playbackDevice;
        private WaveStream _fileStream;

        public event Action PlaybackResumed;
        public event Action PlaybackStopped;
        public event Action PlaybackEnded;
		public event Action PlaybackPaused;
		public event Action<VolumeEventArgs> OnPreVolumeMeter;
		public event Action<VolumeEventArgs> OnSteamVolume;
		public static MMDevice CurrentDevice;
		private VolumeSampleProvider _volumeProvider;
		private float _volume = 1f;
		private static readonly MMDeviceEnumerator DeviceEnumerator;

		static AudioPlayback()
		{
			DeviceEnumerator = new MMDeviceEnumerator();
		}

		public AudioPlayback():this(GetDefaultMMDevice())
		{
			
		}

		internal AudioPlayback(MMDevice device)
		{
			CurrentDevice = device;
		}

		public AudioPlayback(Device device)
		{
			var mmDevice = DeviceEnumerator.GetDevice(device.Id);
			if (mmDevice != null && mmDevice.State == DeviceState.Active)
			{
				CurrentDevice = mmDevice;
			}
			else
			{
				CurrentDevice = mmDevice ?? DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
			}
			
		}

		public static Device GetDefaultDevice()
		{
			var mmDevice = GetDefaultMMDevice();
			return new Device(mmDevice, true);
		}

		private static MMDevice GetDefaultMMDevice()
		{
			var deviceEnumerator = new MMDeviceEnumerator();
			return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
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

		public static string DeviceId => CurrentDevice?.ID;

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
            EnsureDeviceCreated();
            return OpenFile(fileName);
        }

		[SupportedOSPlatform("Windows7.0")]
		public bool Load(string[] files)
		{
			Stop();
			CloseFile();
			EnsureDeviceCreated();
			return LoadFiles(files);
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
                Initialize(inputStream.ToSampleProvider());
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
			_playbackDevice.Init(postVolumeMeter);
        }

		private void PostVolumeMeter_StreamVolume(object sender, StreamVolumeEventArgs e)
		{
			OnSteamVolume?.Invoke(new VolumeEventArgs(e.MaxSampleValues.Length > 0 ? e.MaxSampleValues[0] : 0, e.MaxSampleValues.Length> 1?e.MaxSampleValues[1]:0));
		}

		private void SampleChannelOnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
	        OnPreVolumeMeter?.Invoke(new VolumeEventArgs(e.MaxSampleValues[0], e.MaxSampleValues[1]));
        }

        private void EnsureDeviceCreated()
        {
            if (_playbackDevice == null)
            {
                CreateDevice();
            }
        }

        private void CreateDevice()
        {
            //_playbackDevice = new WaveOutEvent {DesiredLatency = 100};
			_playbackDevice = new WasapiOut(CurrentDevice,AudioClientShareMode.Exclusive, false, 10);
			//_playbackDevice.Volume = Volume;
			_playbackDevice.PlaybackStopped += PlaybackDeviceOnPlaybackStopped;
        }

        private void PlaybackDeviceOnPlaybackStopped(object sender, StoppedEventArgs e)
        {
	        DisposePlaybackDevice();
			PlaybackEnded?.Invoke();
        }

        public void Play()
        {
			EnsureDeviceCreated();
            if (_playbackDevice != null && _fileStream != null && _playbackDevice.PlaybackState != PlaybackState.Playing)
            {
	            _volumeProvider.Volume = Volume;
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
            if (_fileStream != null)
            {
                _fileStream.Position = 0;
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
	        EnsureDeviceCreated();
	        
	        if (_playbackDevice.PlaybackState == PlaybackState.Playing)
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
		        if (_fileStream != null)
		        {
			        _fileStream.CurrentTime = TimeSpan.FromSeconds(value);
		        }
			}
		}

		public double Length => _fileStream.TotalTime.TotalSeconds;

		public float Volume
        {
	        get => _volume;

	        set
	        {
				EnsureDeviceCreated();
				_volume = Math.Min(1,Math.Max(0, value));
				if (_volumeProvider != null)
				{
					_volumeProvider.Volume = _volume;
				}
	        }
        }
		
        public void Dispose()
        {
            Stop();
            CloseFile();
            DisposePlaybackDevice();
        }

        private void DisposePlaybackDevice()
        {
	        _playbackDevice?.Dispose();
	        _playbackDevice = null;
		}

        #region Implementation of IMMNotificationClient

        /// <inheritdoc />
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
	        
        }

        /// <inheritdoc />
        public void OnDeviceAdded(string pwstrDeviceId)
        {
	        
        }

        /// <inheritdoc />
        public void OnDeviceRemoved(string deviceId)
        {
	        
        }

        /// <inheritdoc />
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
	        
        }

        /// <inheritdoc />
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
	        
        }

        #endregion
    }
}
