using Radio_Automation.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace RadioAutomation.Tests.Models
{
	public sealed class TrackTests
	{
		[Fact]
		public void Ctor_Full_AssignsAllProperties()
		{
			var tags = new List<Tag>();
			var genres = new List<Genre>();

			var track = new Track("Name", "Artist", "Album", 2020, TimeSpan.FromMinutes(3), tags, genres);

			Assert.Equal("Name", track.Name);
			Assert.Equal("Artist", track.Artist);
			Assert.Equal("Album", track.Album);
			Assert.Equal(2020u, track.Year);
			Assert.Equal(TimeSpan.FromMinutes(3), track.Duration);
			Assert.Same(tags, track.Tags);
			Assert.Same(genres, track.Genres);
			Assert.NotEqual(Guid.Empty, track.Id);
		}

		[Fact]
		public void Ctor_NamePath_SetsStreamDefaults()
		{
			var track = new Track("My Stream", "http://x");

			Assert.Equal("My Stream", track.Name);
			Assert.Equal("Stream", track.Artist);
			Assert.Equal(string.Empty, track.Album);
			Assert.Equal(TimeSpan.Zero, track.Duration);
			Assert.Equal((uint)DateTime.Now.Year, track.Year);
			Assert.NotNull(track.Tags);
			Assert.Empty(track.Tags);
			Assert.NotNull(track.Genres);
			Assert.Empty(track.Genres);
			Assert.Equal("http://x", track.Path);
		}

		[Fact]
		public void FormattedName_CombinesArtistAndName()
		{
			var track = new Track { Artist = "Artist", Name = "Name" };

			Assert.Equal("Artist - Name", track.FormattedName);
		}

		[Fact]
		public void IsValid_DefaultsToTrue()
		{
			Assert.True(new Track().IsValid);
		}

		[Fact]
		public void Ctor_NamePath_AssignsId()
		{
			Assert.NotEqual(Guid.Empty, new Track("s", "p").Id);
		}

		[Fact]
		public void GetStringTags_EmptyList_ReturnsEmptyString()
		{
			Assert.Equal(string.Empty, new Track().GetStringTags(new List<Tag>()));
		}

		[Fact]
		public void GetStringTags_MultipleTags_ReturnsCommaSeparatedNames()
		{
			var tags = new List<Tag> { new Tag("rock"), new Tag("live") };

			Assert.Equal("rock, live", new Track().GetStringTags(tags));
		}

		[Fact]
		public void GetStringGenres_EmptyList_ReturnsEmptyString()
		{
			Assert.Equal(string.Empty, new Track().GetStringGenres(new List<Genre>()));
		}

		[Fact]
		public void GetStringGenres_MultipleGenres_ReturnsCommaSeparatedNames()
		{
			var genres = new List<Genre> { new Genre("Rock"), new Genre("Pop") };

			Assert.Equal("Rock, Pop", new Track().GetStringGenres(genres));
		}
	}
}
