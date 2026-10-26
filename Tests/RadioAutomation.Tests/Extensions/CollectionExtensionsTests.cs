using Catel.Collections;
using Radio_Automation.Extensions;
using Radio_Automation.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace RadioAutomation.Tests.Extensions
{
	public sealed class CollectionExtensionsTests
	{
		private static Track CreateTrack(string name, string artist)
		{
			return new Track(name, artist, "Album", 2020, TimeSpan.FromMinutes(3), new List<Tag>(), new List<Genre>());
		}

		[Fact]
		public void Shuffle_ObservableCollection_PreservesElements()
		{
			var original = Enumerable.Range(0, 20).ToList();
			var list = new ObservableCollection<int>(original);

			list.Shuffle();

			Assert.Equal(20, list.Count);
			Assert.Equal(original, list.OrderBy(x => x));
		}

		[Fact]
		public void Shuffle_FastObservableCollection_PreservesElements()
		{
			var original = Enumerable.Range(0, 20).ToList();
			var list = new FastObservableCollection<int>(original);

			list.Shuffle();

			Assert.Equal(20, list.Count);
			Assert.Equal(original, list.OrderBy(x => x));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		public void Shuffle_EmptyAndSingle_DoNotThrow(int count)
		{
			var original = Enumerable.Range(0, count).ToList();
			var observable = new ObservableCollection<int>(original);
			var fast = new FastObservableCollection<int>(original);

			observable.Shuffle();
			fast.Shuffle();

			Assert.Equal(original, observable);
			Assert.Equal(original, fast);
		}

		[Fact]
		public void Shuffle_LargeCollection_EventuallyChangesOrder()
		{
			var original = Enumerable.Range(0, 20).ToList();
			var list = new ObservableCollection<int>(original);

			var changed = false;
			for (var i = 0; i < 10 && !changed; i++)
			{
				list.Shuffle();
				changed = !list.SequenceEqual(original);
			}

			Assert.True(changed);
		}

		[Fact]
		public void SortTrackArtist_OrdersByArtist()
		{
			var charlie = CreateTrack("1", "Charlie");
			var alpha = CreateTrack("2", "Alpha");
			var bravo = CreateTrack("3", "Bravo");
			var list = new FastObservableCollection<Track> { charlie, alpha, bravo };

			list.SortTrackArtist();

			Assert.Same(alpha, list[0]);
			Assert.Same(bravo, list[1]);
			Assert.Same(charlie, list[2]);
		}

		[Fact]
		public void SortTrackArtist_AlreadySorted_IsUnchanged()
		{
			var alpha = CreateTrack("1", "Alpha");
			var bravo = CreateTrack("2", "Bravo");
			var charlie = CreateTrack("3", "Charlie");
			var list = new FastObservableCollection<Track> { alpha, bravo, charlie };

			list.SortTrackArtist();

			Assert.Equal(new[] { alpha, bravo, charlie }, list);
		}

		[Fact]
		public void SortTrackArtist_NullArtist_SortsFirst()
		{
			var alpha = CreateTrack("1", "Alpha");
			var noArtist = CreateTrack("2", null);
			var list = new FastObservableCollection<Track> { alpha, noArtist };

			list.SortTrackArtist();

			Assert.Same(noArtist, list[0]);
		}

		[Fact]
		public void SortTrackName_OrdersByName()
		{
			var charlie = CreateTrack("Charlie", "x");
			var alpha = CreateTrack("Alpha", "x");
			var bravo = CreateTrack("Bravo", "x");
			var list = new FastObservableCollection<Track> { charlie, alpha, bravo };

			list.SortTrackName();

			Assert.Same(alpha, list[0]);
			Assert.Same(bravo, list[1]);
			Assert.Same(charlie, list[2]);
		}
	}
}
