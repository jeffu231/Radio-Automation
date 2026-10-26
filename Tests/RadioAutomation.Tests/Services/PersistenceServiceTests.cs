using Radio_Automation.Models;
using Radio_Automation.Services;
using RadioAutomation.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RadioAutomation.Tests.Services
{
	public sealed class PersistenceServiceTests
	{
		private static PersistenceService CreateService()
		{
			return new PersistenceService(TestJsonOptions.Create());
		}

		private static Track CreateTrack(string name, string artist, string album, uint year, TimeSpan duration, string path)
		{
			return new Track(name, artist, album, year, duration, new List<Tag>(), new List<Genre>()) { Path = path };
		}

		[Fact]
		public void Ctor_NullOptions_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => new PersistenceService(null));
		}

		[Fact]
		public async Task SaveThenLoadPlaylist_RoundTripsTracks()
		{
			using var dir = new TempDirectory();
			var service = CreateService();
			var playlist = new Playlist(new List<Track>
			{
				CreateTrack("One", "ArtistA", "AlbumA", 2001, TimeSpan.FromMinutes(1), @"C:\one.mp3"),
				CreateTrack("Two", "ArtistB", "AlbumB", 2002, new TimeSpan(0, 2, 30), @"C:\two.mp3")
			}) { Name = "Seed" };

			var saved = await service.SavePlaylistAsync(playlist, dir.File("p.json"));
			var loaded = await service.LoadPlaylistAsync(dir.File("p.json"));

			Assert.True(saved);
			Assert.Equal("Seed", loaded.Name);
			Assert.Equal(2, loaded.Tracks.Count);
			for (var i = 0; i < 2; i++)
			{
				var expected = playlist.Tracks[i];
				var actual = loaded.Tracks[i];
				Assert.Equal(expected.Name, actual.Name);
				Assert.Equal(expected.Artist, actual.Artist);
				Assert.Equal(expected.Album, actual.Album);
				Assert.Equal(expected.Year, actual.Year);
				Assert.Equal(expected.Duration, actual.Duration);
				Assert.Equal(expected.Path, actual.Path);
			}
			Assert.Equal(new TimeSpan(0, 3, 30), loaded.TotalTime);
		}

		[Fact]
		public async Task LoadPlaylist_MissingFile_ReturnsEmptyPlaylist()
		{
			using var dir = new TempDirectory();

			var loaded = await CreateService().LoadPlaylistAsync(dir.File("missing.json"));

			Assert.NotNull(loaded);
			Assert.Empty(loaded.Tracks);
		}

		[Fact]
		public async Task LoadPlaylist_CorruptJson_ReturnsEmptyPlaylist()
		{
			using var dir = new TempDirectory();
			File.WriteAllText(dir.File("p.json"), "{ not json");

			var loaded = await CreateService().LoadPlaylistAsync(dir.File("p.json"));

			Assert.NotNull(loaded);
			Assert.Empty(loaded.Tracks);
		}

		[Fact]
		public async Task SaveThenLoadEventSchedule_RoundTripsEvents()
		{
			using var dir = new TempDirectory();
			var service = CreateService();
			var cron = new Event { Trigger = Trigger.Cron, CronExpression = "0 * * * *", EventType = EventType.Time };
			var mqtt = new Event
			{
				Trigger = Trigger.Mqtt,
				MqttExpression = new MqttExpression { Topic = "radio/cmd", Message = "stop" },
				EventType = EventType.Stop,
				Demand = Demand.Delayed,
				Enabled = true
			};
			var schedule = new EventSchedule { Name = "Weekday", Events = new ObservableCollection<Event> { cron, mqtt } };

			await service.SaveEventScheduleAsync(schedule, dir.File("s.evs"));
			var loaded = await service.LoadEventScheduleAsync(dir.File("s.evs"));

			Assert.Equal("Weekday", loaded.Name);
			Assert.Equal(2, loaded.Events.Count);
			for (var i = 0; i < 2; i++)
			{
				var expected = schedule.Events[i];
				var actual = loaded.Events[i];
				Assert.Equal(expected.Id, actual.Id);
				Assert.Equal(expected.Trigger, actual.Trigger);
				Assert.Equal(expected.EventType, actual.EventType);
				Assert.Equal(expected.Demand, actual.Demand);
				Assert.Equal(expected.Enabled, actual.Enabled);
				Assert.Equal(expected.CronExpression, actual.CronExpression);
				Assert.Equal(expected.MqttExpression.Topic, actual.MqttExpression.Topic);
				Assert.Equal(expected.MqttExpression.Message, actual.MqttExpression.Message);
			}
		}

		[Fact]
		public async Task LoadEventSchedule_MissingFile_ReturnsEmptySchedule()
		{
			using var dir = new TempDirectory();

			var loaded = await CreateService().LoadEventScheduleAsync(dir.File("missing.evs"));

			Assert.NotNull(loaded);
			Assert.Empty(loaded.Events);
		}

		[Fact]
		public async Task ImportZaraPlaylist_ImportsOnlyExistingWellFormedEntries()
		{
			using var dir = new TempDirectory();
			var wav = TestAudio.CreateSineWav(dir.File("a.wav"));
			var lines = new[] { "1", "0\t" + wav, "0\t" + dir.File("missing.wav"), "malformed line without tab" };
			File.WriteAllText(dir.File("z.zpl"), string.Join("\r\n", lines), new UTF8Encoding(false));

			var playlist = await CreateService().ImportZaraPlaylistAsync(dir.File("z.zpl"));

			var track = Assert.Single(playlist.Tracks);
			Assert.Equal(wav, track.Path);
		}

		[Fact]
		public async Task ImportZaraPlaylist_HeaderOnly_ReturnsEmptyPlaylist()
		{
			using var dir = new TempDirectory();
			File.WriteAllText(dir.File("z.zpl"), "header", new UTF8Encoding(false));

			var playlist = await CreateService().ImportZaraPlaylistAsync(dir.File("z.zpl"));

			Assert.Empty(playlist.Tracks);
		}
	}
}
