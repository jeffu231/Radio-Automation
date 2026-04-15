using Catel;
using Catel.Logging;
using Catel.Reflection;
using OneWay.M3U;
using Radio_Automation.Extensions;
using Radio_Automation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;

namespace Radio_Automation.Services
{
	public class PersistenceService:IPersistenceService
	{
		private readonly JsonSerializerOptions _serializerOptions;
		private readonly string _settingsPath;
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

		public PersistenceService(JsonSerializerOptions serializerOptions)
		{
			Argument.IsNotNull(() => serializerOptions);
			_serializerOptions = serializerOptions;
			var assembly = AssemblyHelper.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

			_settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), assembly.Company()??@"jtdev", assembly.Product() ?? @"Radio Automation", @"settings.json");
			EnsureTempPath();
		}

		#region Implementation of IPersistenceService

		/// <inheritdoc />
		public async Task<Playlist> LoadPlaylistAsync(string path)
		{
			if (File.Exists(path))
			{
				await using var fileStream = File.Open(path, FileMode.Open);
				try
				{
					var pl = await JsonSerializer.DeserializeAsync<Playlist>(fileStream, _serializerOptions);
					pl?.CalculateTotalTime();
					return pl;
				}
				catch
				{
					Log.Error($"Error loading playlist from path {path}");
				}
			}

			return new Playlist();
		}

		/// <inheritdoc />
		public async Task<bool> SavePlaylistAsync(Playlist p, string path)
		{
			string jsonString = JsonSerializer.Serialize(p, _serializerOptions);
			await using var fileStream2 = File.Open(path, FileMode.Create);
			await using var writer = new StreamWriter(fileStream2); 
			await writer.WriteAsync(jsonString);
			return true;
		}

		/// <inheritdoc />
		public async Task<Playlist> ImportZaraPlaylistAsync(string path)
		{
			var lines = await FileExtensions.ReadAllLinesAsync(path);

			return await Task.Factory.StartNew(() =>
			{
				var totalTracks = lines.Length - 1;
				List<Track> tracks = new List<Track>(totalTracks);
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
							Log.Error($"Error importing Zara playlist: {items[1]} not found!");
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
						else
						{
							Log.Error($"Error creating track from path {m3UMediaInfo.Uri.LocalPath}");
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
			if (File.Exists(_settingsPath))
			{
				await using var fileStream = File.Open(_settingsPath, FileMode.Open);
				try
				{
					return await JsonSerializer.DeserializeAsync<Settings>(fileStream, _serializerOptions);
				}
				catch
				{
					Log.Error($"Error loading settings from path {_settingsPath}");
				}
			}

			return new Settings();
		}

		/// <inheritdoc />
		public async Task<bool> SaveSettingsAsync(Settings settings)
		{
			string jsonString = JsonSerializer.Serialize(settings, _serializerOptions);
			await using var fileStream2 = File.Open(_settingsPath, FileMode.Create);
			await using var writer = new StreamWriter(fileStream2); 
			await writer.WriteAsync(jsonString);
			return true;
		}

		/// <inheritdoc />
		public async Task<EventSchedule> LoadEventScheduleAsync(string path)
		{
			if (File.Exists(path))
			{
				await using var fileStream = File.Open(path, FileMode.Open);
				try
				{
					return await JsonSerializer.DeserializeAsync<EventSchedule>(fileStream, _serializerOptions);
				}
				catch
				{
					Log.Error($"Error loading event schedule from path {path}");
				}
			}

			return new EventSchedule();
		}

		/// <inheritdoc />
		public async Task<bool> SaveEventScheduleAsync(EventSchedule eventSchedule, string path)
		{
			string jsonString = JsonSerializer.Serialize(eventSchedule, _serializerOptions);
			await using var fileStream2 = File.Open(path, FileMode.Create);
			await using var writer = new StreamWriter(fileStream2);
			await writer.WriteAsync(jsonString);
			return true;
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
