using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace RadioAutomation.Tests.TestUtilities
{
	public static class TestAudio
	{
		public static string CreateSineWav(string path, int sampleRate = 44100, int channels = 1, int durationMs = 250)
		{
			var provider = new SignalGenerator(sampleRate, channels)
			{
				Type = SignalGeneratorType.Sin,
				Frequency = 440,
				Gain = 0.2
			}.Take(TimeSpan.FromMilliseconds(durationMs));

			WaveFileWriter.CreateWaveFile16(path, provider);
			return path;
		}
	}
}
