using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Radio_Automation.Models;
using TagLib;
using Tag = Radio_Automation.Models.Tag;

namespace Radio_Automation.Services
{
	public class AudioTrackParserService:IAudioTrackParserService
	{
		#region Implementation of IAudioTrackParserService

		/// <inheritdoc />
		public List<Track> RetrieveAudioTracksInPath(string path, bool recurse)
		{
			if (recurse)
			{
				return RecurseFoldersForTracks(path, new List<Track>());
			}

			return RetrieveAudioTracksFromFolder(path);

		}

		/// <inheritdoc />
		public Track RetrieveAudioTrackFromFile(string path)
		{
			return CreateTrackFromFile(path);
		}

		#endregion

		private List<Track> RecurseFoldersForTracks(string path, List<Track> tracks)
		{
			tracks.AddRange(RetrieveAudioTracksFromFolder(path));
			DirectoryInfo i = new DirectoryInfo(path);
			var dirs = i.GetDirectories().Where(d => (d.Attributes & FileAttributes.Hidden) == 0);
			foreach (var dir in dirs)
			{
				tracks.AddRange(RetrieveAudioTracksFromFolder(dir.FullName));
				tracks.AddRange(RecurseFoldersForTracks(dir.FullName, tracks));
			}

			return tracks;
		}

		private List<Track> RetrieveAudioTracksFromFolder(string path)
		{
			var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
				.Where(s => s.EndsWith(".mp3") || s.EndsWith(".wav") || s.EndsWith(".flac") || s.EndsWith(".ogg") || s.EndsWith(".wma"));

			List<Track> tracks = new List<Track>(files.Count());
			foreach (var file in files)
			{
				try
				{
					var t = CreateTrackFromFile(file);
					tracks.Add(t);
				}
				catch (Exception e)
				{
					Console.Out.WriteLine($"Error reading {file}, message {e.Message}");

				}


			}

			return tracks;
		}

		public static Track CreateTrackFromFile(string file)
		{
			try
			{
				var tagFile = TagLib.File.Create(file);
				var t = new Track(tagFile.Tag.Title, tagFile.Tag.FirstPerformer, tagFile.Tag.Album, tagFile.Tag.Year,
					tagFile.Properties.Duration, new List<Tag>(), tagFile.Tag.Genres.Select(x => new Genre(x)).ToList());
				t.Path = file;
				return t;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
			
		}
	}
}
