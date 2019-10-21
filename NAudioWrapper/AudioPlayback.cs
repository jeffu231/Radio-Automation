﻿using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace NAudioWrapper
{
    public class AudioPlayback : IDisposable
    {
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

		public AudioPlayback()
		{
			var deviceEnumerator = new MMDeviceEnumerator();
			var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
			foreach (var device in devices)
			{
				Console.Out.WriteLineAsync(device.FriendlyName);
			}

			CurrentDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
			Console.Out.WriteLineAsync($"Default = {CurrentDevice}");
		}

		public void Load(string fileName)
        {
            Stop();
            CloseFile();
            EnsureDeviceCreated();
            OpenFile(fileName);
        }


		public void Load(string[] files)
		{
			Stop();
			CloseFile();
			EnsureDeviceCreated();
			LoadFiles(files);
		}

		private void CloseFile()
        {
            _fileStream?.Dispose();
            _fileStream = null;
        }

        private void LoadFiles(string[] paths)
        {
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
	        }
	        catch (Exception e)
	        {
				MessageBox.Show(e.Message, "Problem opening files");
				CloseFile();
			}
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var inputStream = new MediaFoundationReader(fileName);
				_fileStream = inputStream;
                Initialize(inputStream.ToSampleProvider());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem opening file");
                CloseFile();
            }
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
			_playbackDevice = new WasapiOut(CurrentDevice,AudioClientShareMode.Shared, false, 20);
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
    }
}
