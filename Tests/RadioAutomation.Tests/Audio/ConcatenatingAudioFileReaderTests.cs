using NAudio.Wave;
using NAudioWrapper;
using RadioAutomation.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using Xunit;

namespace RadioAutomation.Tests.Audio
{
	public sealed class ConcatenatingAudioFileReaderTests : IDisposable
	{
		private readonly TempDirectory _dir = new TempDirectory();
		private readonly List<AudioFileReader> _readers = new List<AudioFileReader>();

		public void Dispose()
		{
			foreach (var reader in _readers)
			{
				reader.Dispose();
			}
			_dir.Dispose();
		}

		private AudioFileReader Open(string name, int sampleRate, int channels, int durationMs)
		{
			var reader = new AudioFileReader(TestAudio.CreateSineWav(_dir.File(name), sampleRate, channels, durationMs));
			_readers.Add(reader);
			return reader;
		}

		[Fact]
		public void Ctor_Null_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new ConcatenatingAudioFileReader(null));
		}

		[Fact]
		public void Ctor_Empty_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => new ConcatenatingAudioFileReader(Array.Empty<AudioFileReader>()));
		}

		[Fact]
		public void Ctor_MismatchedSampleRate_Throws()
		{
			var a = Open("a.wav", 44100, 1, 250);
			var b = Open("b.wav", 22050, 1, 250);

			Assert.Throws<ArgumentException>(() => new ConcatenatingAudioFileReader(new[] { a, b }));
		}

		[Fact]
		public void Ctor_MismatchedChannels_Throws()
		{
			var a = Open("a.wav", 44100, 1, 250);
			var b = Open("b.wav", 44100, 2, 250);

			Assert.Throws<ArgumentException>(() => new ConcatenatingAudioFileReader(new[] { a, b }));
		}

		[Fact]
		public void Ctor_MatchingInputs_LengthIsSumOfInputs()
		{
			var a = Open("a.wav", 44100, 1, 250);
			var b = Open("b.wav", 44100, 1, 500);

			var reader = new ConcatenatingAudioFileReader(new[] { a, b });

			Assert.Equal(a.Length + b.Length, reader.Length);
			Assert.Equal(a.WaveFormat, reader.WaveFormat);
		}

		[Fact]
		public void Read_ReadsAllInputsSequentially()
		{
			var a = Open("a.wav", 44100, 1, 250);
			var b = Open("b.wav", 44100, 1, 500);
			var expected = (a.Length + b.Length) / 4;
			var reader = new ConcatenatingAudioFileReader(new[] { a, b });

			var buffer = new float[4096];
			long total = 0;
			int read;
			while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
			{
				total += read;
			}

			Assert.Equal(expected, total);
		}
	}
}
