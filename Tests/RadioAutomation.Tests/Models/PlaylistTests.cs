using Radio_Automation.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace RadioAutomation.Tests.Models
{
	public sealed class PlaylistTests
	{
		private static Track CreateTrack(TimeSpan duration)
		{
			return new Track("Name", "Artist", "Album", 2020, duration, new List<Tag>(), new List<Genre>());
		}

		[Fact]
		public void Ctor_Default_IsEmptyWithZeroTotalTime()
		{
			var playlist = new Playlist();

			Assert.Empty(playlist.Tracks);
			Assert.Equal(TimeSpan.Zero, playlist.TotalTime);
			Assert.NotEqual(Guid.Empty, playlist.Id);
		}

		[Fact]
		public void Ctor_WithTracks_CalculatesTotalTime()
		{
			var playlist = new Playlist(new List<Track> { CreateTrack(TimeSpan.FromMinutes(1)), CreateTrack(new TimeSpan(0, 2, 30)) });

			Assert.Equal(new TimeSpan(0, 3, 30), playlist.TotalTime);
		}

		[Fact]
		public void AddingTrack_UpdatesTotalTime()
		{
			var playlist = new Playlist(new List<Track> { CreateTrack(TimeSpan.FromMinutes(1)) });

			playlist.Tracks.Add(CreateTrack(TimeSpan.FromSeconds(45)));

			Assert.Equal(new TimeSpan(0, 1, 45), playlist.TotalTime);
		}

		[Fact]
		public void RemovingTrack_UpdatesTotalTime()
		{
			var removed = CreateTrack(TimeSpan.FromMinutes(2));
			var playlist = new Playlist(new List<Track> { CreateTrack(TimeSpan.FromMinutes(1)), removed });

			playlist.Tracks.Remove(removed);

			Assert.Equal(TimeSpan.FromMinutes(1), playlist.TotalTime);
		}
	}
}
