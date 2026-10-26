using NAudioWrapper;
using RadioAutomation.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using Xunit;

namespace RadioAutomation.Tests.Audio
{
	public sealed class SampleAggregatorTests
	{
		private static readonly float[] ThreeSamples = { 0.1f, 0.2f, 0.3f };
		private static readonly float[] TwoPositiveSamples = { 0.3f, 0.6f };

		private static List<MaxSampleEventArgs> Capture(SampleAggregator aggregator)
		{
			var raised = new List<MaxSampleEventArgs>();
			aggregator.MaximumCalculated += (s, e) => raised.Add(e);
			return raised;
		}

		[Fact]
		public void Ctor_FftLengthNotPowerOfTwo_Throws()
		{
			Assert.Throws<ArgumentException>(() => new SampleAggregator(new FakeSampleProvider(new float[1]), 1000));
		}

		[Fact]
		public void Ctor_DefaultFftLength_Succeeds()
		{
			var source = new FakeSampleProvider(new float[1]);

			var aggregator = new SampleAggregator(source);

			Assert.Same(source.WaveFormat, aggregator.WaveFormat);
		}

		[Fact]
		public void Read_PassesSamplesThroughUnchanged()
		{
			var aggregator = new SampleAggregator(new FakeSampleProvider(ThreeSamples));
			var buffer = new float[3];

			var read = aggregator.Read(buffer, 0, 3);

			Assert.Equal(3, read);
			Assert.Equal(ThreeSamples, buffer);
		}

		[Fact]
		public void Read_NotificationCountReached_RaisesMaximumCalculated()
		{
			var aggregator = new SampleAggregator(new FakeSampleProvider(new[] { 0.1f, -0.5f, 0.9f, 0.2f })) { NotificationCount = 4 };
			var raised = Capture(aggregator);

			aggregator.Read(new float[4], 0, 4);

			var args = Assert.Single(raised);
			Assert.Equal(-0.5f, args.MinSample);
			Assert.Equal(0.9f, args.MaxSample);
		}

		[Fact]
		public void Read_NotificationCountZero_NeverRaises()
		{
			var aggregator = new SampleAggregator(new FakeSampleProvider(new[] { 0.1f, -0.5f, 0.9f, 0.2f })) { NotificationCount = 0 };
			var raised = Capture(aggregator);

			aggregator.Read(new float[4], 0, 4);

			Assert.Empty(raised);
		}

		[Fact]
		public void Read_Stereo_OnlySamplesFirstChannel()
		{
			var source = new FakeSampleProvider(new[] { 0.1f, 0.99f, -0.2f, -0.99f }, channels: 2);
			var aggregator = new SampleAggregator(source) { NotificationCount = 2 };
			var raised = Capture(aggregator);

			aggregator.Read(new float[4], 0, 4);

			var args = Assert.Single(raised);
			Assert.Equal(0.1f, args.MaxSample);
			Assert.Equal(-0.2f, args.MinSample);
		}

		[Fact]
		public void Read_AllPositiveSamples_MinIsZero()
		{
			var aggregator = new SampleAggregator(new FakeSampleProvider(TwoPositiveSamples)) { NotificationCount = 2 };
			var raised = Capture(aggregator);

			aggregator.Read(new float[2], 0, 2);

			var args = Assert.Single(raised);
			Assert.Equal(0f, args.MinSample);
			Assert.Equal(0.6f, args.MaxSample);
		}

		[Fact]
		public void Reset_ClearsAccumulatedState()
		{
			var source = new FakeSampleProvider(new[] { 0.9f, -0.9f, 0.1f, -0.1f, 0.2f, -0.2f });
			var aggregator = new SampleAggregator(source) { NotificationCount = 4 };
			var raised = Capture(aggregator);

			aggregator.Read(new float[2], 0, 2);
			aggregator.Reset();
			aggregator.Read(new float[4], 0, 4);

			var args = Assert.Single(raised);
			Assert.Equal(0.2f, args.MaxSample);
			Assert.Equal(-0.2f, args.MinSample);
		}
	}
}
