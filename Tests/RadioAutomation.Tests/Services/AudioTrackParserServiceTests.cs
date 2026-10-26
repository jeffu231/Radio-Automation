using Radio_Automation.Services;
using RadioAutomation.Tests.TestUtilities;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RadioAutomation.Tests.Services
{
	public sealed class AudioTrackParserServiceTests
	{
		private readonly AudioTrackParserService _service = new AudioTrackParserService();

		[Fact]
		public void CreateTrackFromFile_MissingFile_ReturnsNull()
		{
			using var dir = new TempDirectory();

			Assert.Null(AudioTrackParserService.CreateTrackFromFile(dir.File("missing.wav")));
		}

		[Fact]
		public void CreateTrackFromFile_ValidWav_ReturnsTrack()
		{
			using var dir = new TempDirectory();
			var path = TestAudio.CreateSineWav(dir.File("a.wav"), durationMs: 500);

			var track = AudioTrackParserService.CreateTrackFromFile(path);

			Assert.NotNull(track);
			Assert.Equal(path, track.Path);
			Assert.InRange(track.Duration.TotalMilliseconds, 450, 550);
			Assert.NotNull(track.Genres);
			Assert.Empty(track.Genres);
		}

		[Fact]
		public void RetrieveAudioTrackFromFile_DelegatesToCreate()
		{
			using var dir = new TempDirectory();
			var path = TestAudio.CreateSineWav(dir.File("a.wav"));

			var track = _service.RetrieveAudioTrackFromFile(path);

			Assert.Equal(path, track.Path);
		}

		[Fact]
		public void RetrieveAudioTracksInPath_NonRecursive_ReturnsOnlyAudioFiles()
		{
			using var dir = new TempDirectory();
			var one = TestAudio.CreateSineWav(dir.File("one.wav"));
			var two = TestAudio.CreateSineWav(dir.File("two.wav"));
			File.WriteAllText(dir.File("notes.txt"), "notes");
			Directory.CreateDirectory(dir.File("sub"));
			TestAudio.CreateSineWav(dir.File(Path.Combine("sub", "three.wav")));

			var tracks = _service.RetrieveAudioTracksInPath(dir.Path, false);

			Assert.Equal(2, tracks.Count);
			Assert.Equal(new[] { one, two }.OrderBy(x => x), tracks.Select(t => t.Path).OrderBy(x => x));
		}

		[Fact]
		public void RetrieveAudioTracksInPath_UnreadableAudioFile_IsSkipped()
		{
			using var dir = new TempDirectory();
			var bytes = new byte[256];
			new Random(1).NextBytes(bytes);
			File.WriteAllBytes(dir.File("bad.mp3"), bytes);

			var tracks = _service.RetrieveAudioTracksInPath(dir.Path, false);

			Assert.DoesNotContain(null, tracks);
		}

		[Fact(Skip = "Known defect D5: extension filter is case-sensitive - see docs/plans/unit-tests-seed-plan.md section 9")]
		public void RetrieveAudioTracksInPath_UppercaseExtension_IsIncluded()
		{
			using var dir = new TempDirectory();
			TestAudio.CreateSineWav(dir.File("LOUD.WAV"));

			var tracks = _service.RetrieveAudioTracksInPath(dir.Path, false);

			Assert.Single(tracks);
		}

		[Fact(Skip = "Known defect D6: recursion returns duplicate tracks - see docs/plans/unit-tests-seed-plan.md section 9")]
		public void RetrieveAudioTracksInPath_Recursive_ReturnsEachFileOnce()
		{
			using var dir = new TempDirectory();
			TestAudio.CreateSineWav(dir.File("root.wav"));
			Directory.CreateDirectory(dir.File("sub"));
			TestAudio.CreateSineWav(dir.File(Path.Combine("sub", "sub.wav")));
			Directory.CreateDirectory(dir.File(Path.Combine("sub", "sub2")));
			TestAudio.CreateSineWav(dir.File(Path.Combine("sub", "sub2", "deep.wav")));

			var tracks = _service.RetrieveAudioTracksInPath(dir.Path, true);

			Assert.Equal(3, tracks.Count);
			Assert.Equal(3, tracks.Select(t => t.Path).Distinct().Count());
		}
	}
}
