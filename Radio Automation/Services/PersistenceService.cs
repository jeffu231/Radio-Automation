using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Json;
using Newtonsoft.Json;
using OneWay.M3U;
using Radio_Automation.Extensions;
using Radio_Automation.Models;
using FileMode = System.IO.FileMode;

namespace Radio_Automation.Services
{
	public class PersistenceService:IPersistenceService
	{
		private readonly IJsonSerializer _serializer;
		private readonly ISerializationConfiguration _serializationConfiguration;
		private string _settingsPath;

		public PersistenceService(IJsonSerializer serializer)
		{
			Argument.IsNotNull(() => serializer);
			_serializer = serializer;
			_serializer.WriteTypeInfo = false;
			_serializer.PreserveReferences = false;
			_serializationConfiguration = new JsonSerializationConfiguration{Formatting = Formatting.Indented};
			_settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Radio Automation", @"settings.json");
			EnsureTempPath();
		}

		#region Implementation of IPlaylistService

		/// <inheritdoc />
		public async Task<Playlist> LoadPlaylistAsync(string path)
		{
			return await Task.Factory.StartNew(() =>
			{
				using (var fileStream = File.Open(path, FileMode.Open))
				{
					var pl = _serializer.Deserialize<Playlist>(fileStream, _serializationConfiguration);
					pl?.CalculateTotalTime();
					return pl;
				}
			});
		}

		/// <inheritdoc />
		public async Task<bool> SavePlaylistAsync(Playlist p, string path)
		{
			return await Task.Factory.StartNew(() =>
			{
				using (var fileStream = File.Open(path, FileMode.Create))
				{
					_serializer.Serialize(p, fileStream, _serializationConfiguration);
				}

				return true;
				
			});

		}

		/// <inheritdoc />
		public async Task<Playlist> ImportZaraPlaylistAsync(string path)
		{
			var lines = await FileExtensions.ReadAllLinesAsync(path);

			return await Task.Factory.StartNew(() =>
			{
				var totalTracks = lines.Length - 1;
				List<Track> tracks = new List<Track>(totalTracks);
				var index = 1;
				foreach (var line in lines.Skip(1))
				{
					var items = line.Split(new[] { "\t" }, StringSplitOptions.None);
					if (items.Length == 2)
					{
						if (File.Exists(items[1]))
						{
							var track = AudioTrackParserService.CreateTrackFromFile(items[1]);
							if (track != null)
							{
								tracks.Add(track);
							}
						}
						else
						{
							Console.Out.WriteLineAsync($"{items[1]} not found!");
						}
					}
				}

				return new Playlist(tracks);

			});
			
		}

		/// <inheritdoc />
		public async Task<Playlist> ImportM3UPlaylistAsync(string path)
		{
			return await Task.Factory.StartNew(() =>
			{
				M3UFileInfo m3uFile;
				var file = new FileInfo(path);
				using (var reader = new M3UFileReader(file))
				{
					m3uFile = reader.Read();
				}

				if (m3uFile != null)
				{
					List<Track> tracks = new List<Track>(m3uFile.MediaFiles.Count);
					foreach (var m3UMediaInfo in m3uFile.MediaFiles)
					{
						var track = AudioTrackParserService.CreateTrackFromFile(m3UMediaInfo.Uri.LocalPath);
						if (track != null)
						{
							tracks.Add(track);
						}

					}
					return new Playlist(tracks);
				}
				
				return new Playlist();
			});
		}

		/// <inheritdoc />
		public async Task<Settings> LoadSettingsAsync()
		{
			return await Task.Factory.StartNew(() =>
			{
				if (File.Exists(_settingsPath))
				{
					using (var fileStream = File.Open(_settingsPath, FileMode.Open))
					{
						try
						{
							return _serializer.Deserialize<Settings>(fileStream, _serializationConfiguration);
						}
						catch
						{
							//TODO Log this
						}
						
					}
				}

				return new Settings();

			});
		}

		/// <inheritdoc />
		public async Task<bool> SaveSettingsAsync(Settings settings)
		{
			return await Task.Factory.StartNew(() =>
			{
				using (var fileStream = File.Open(_settingsPath, FileMode.Create))
				{
					_serializer.Serialize(settings, fileStream, _serializationConfiguration);
				}

				return true;

			});
		}

		/// <inheritdoc />
		public async Task<EventSchedule> LoadEventScheduleAsync(string path)
		{
			return await Task.Factory.StartNew(() =>
			{
				if (File.Exists(path))
				{
					using (var fileStream = File.Open(path, FileMode.Open))
					{
						try
						{
							return _serializer.Deserialize<EventSchedule>(fileStream, _serializationConfiguration);
						}
						catch
						{
							//TODO log this
						}

					}
				}

				return new EventSchedule();

			});
		}

		/// <inheritdoc />
		public async Task<bool> SaveEventScheduleAsync(EventSchedule eventSchedule, string path)
		{
			return await Task.Factory.StartNew(() =>
			{
				using (var fileStream = File.Open(path, FileMode.Create))
				{
					_serializer.Serialize(eventSchedule, fileStream, _serializationConfiguration);
				}

				return true;

			});
		}

		#endregion

		private void EnsureTempPath()
		{
			var dir = Path.GetDirectoryName(_settingsPath);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}
	}
}
