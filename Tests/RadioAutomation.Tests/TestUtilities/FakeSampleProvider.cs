using NAudio.Wave;
using System;

namespace RadioAutomation.Tests.TestUtilities
{
	public sealed class FakeSampleProvider : ISampleProvider
	{
		private readonly float[] _samples;
		private int _position;

		public FakeSampleProvider(float[] samples, int channels = 1, int sampleRate = 44100)
		{
			_samples = samples;
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
		}

		public WaveFormat WaveFormat { get; }

		public int Read(float[] buffer, int offset, int count)
		{
			var toCopy = Math.Min(count, _samples.Length - _position);
			Array.Copy(_samples, _position, buffer, offset, toCopy);
			_position += toCopy;
			return toCopy;
		}
	}
}
