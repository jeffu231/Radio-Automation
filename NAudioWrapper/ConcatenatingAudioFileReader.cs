using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace NAudioWrapper
{
	public class ConcatenatingAudioFileReader:WaveStream, ISampleProvider
	{
		private readonly AudioFileReader[] providers;
		private int _currentProviderIndex;

		/// <summary>Creates a new ConcatenatingSampleProvider</summary>
		/// <param name="providers">The source providers to play one after the other. Must all share the same sample rate and channel count</param>
		public ConcatenatingAudioFileReader(IEnumerable<AudioFileReader> providers)
		{
			if (providers == null)
				throw new ArgumentNullException(nameof(providers));
			this.providers = providers.ToArray();
			if (this.providers.Length == 0)
				throw new ArgumentException("Must provide at least one input", nameof(providers));
			if (this.providers.Any(p => p.WaveFormat.Channels != this.WaveFormat.Channels))
				throw new ArgumentException("All inputs must have the same channel count", nameof(providers));
			if (this.providers.Any(p => p.WaveFormat.SampleRate != this.WaveFormat.SampleRate))
				throw new ArgumentException("All inputs must have the same sample rate", nameof(providers));

			Length = providers.Sum(p => p.Length);
		}

		#region Overrides of Stream

		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count)
		{
			WaveBuffer waveBuffer = new WaveBuffer(buffer);
			int count1 = count / 4;
			return this.Read(waveBuffer.FloatBuffer, offset / 4, count1) * 4;
		}

		/// <inheritdoc />
		public int Read(float[] buffer, int offset, int count)
		{
			int offset1 = 0;
			while (offset1 < count && this._currentProviderIndex < this.providers.Length)
			{
				int count1 = count - offset1;
				int num = this.providers[this._currentProviderIndex].Read(buffer, offset1, count1);
				offset1 += num;
				if (num == 0)
					++this._currentProviderIndex;
			}
			return offset1;
		}

		/// <summary>The WaveFormat of this Sample Provider</summary>
		public override WaveFormat WaveFormat => this.providers[0].WaveFormat;

		/// <inheritdoc />
		public override long Length { get; }

		/// <inheritdoc />
		public override long Position { get; set; }

		#endregion
	}
}
