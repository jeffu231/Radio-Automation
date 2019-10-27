using System;
using System.Collections.Generic;
using System.Text;
using Catel.Data;

namespace Radio_Automation.Models
{
	public class Track: ModelBase
	{
		public Track()
		{
			Id = Guid.NewGuid();
		}

		public Track(string name, string path)
		{
			Name = name;
			Path = path;
			Artist = @"Stream";
			Album = string.Empty;
			Year = (uint)DateTime.Now.Year;
			Duration = TimeSpan.Zero;
			Tags = new List<Tag>();
			Genres = new List<Genre>();
		}

		public Track(string name, string artist, string album, uint year, TimeSpan duration, List<Tag> tags, List<Genre> genres):this()
		{
			Name = name;
			Artist = artist;
			Album = album;
			Year = year;
			Duration = duration;
			Tags = tags;
			Genres = genres;
		}

		#region Id property

		/// <summary>
		/// Gets or sets the Id value.
		/// </summary>
		public Guid Id
		{
			get { return GetValue<Guid>(IdProperty); }
			set { SetValue(IdProperty, value); }
		}

		/// <summary>
		/// Id property data.
		/// </summary>
		public static readonly PropertyData IdProperty = RegisterProperty("Id", typeof(Guid));

		#endregion

		#region Name property

		/// <summary>
		/// Gets or sets the Name value.
		/// </summary>
		public string Name
		{
			get { return GetValue<string>(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		/// <summary>
		/// Name property data.
		/// </summary>
		public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string));

		#endregion

		#region Artist property

		/// <summary>
		/// Gets or sets the Artist value.
		/// </summary>
		public string Artist
		{
			get { return GetValue<string>(ArtistProperty); }
			set { SetValue(ArtistProperty, value); }
		}

		/// <summary>
		/// Artist property data.
		/// </summary>
		public static readonly PropertyData ArtistProperty = RegisterProperty("Artist", typeof(string));

		#endregion

		#region Album property

		/// <summary>
		/// Gets or sets the Album value.
		/// </summary>
		public string Album
		{
			get { return GetValue<string>(AlbumProperty); }
			set { SetValue(AlbumProperty, value); }
		}

		/// <summary>
		/// Album property data.
		/// </summary>
		public static readonly PropertyData AlbumProperty = RegisterProperty("Album", typeof(string));

		#endregion

		/// <summary>
		/// Formatted track name
		/// </summary>
		public string FormattedName => $"{Artist} - {Name}";

		#region Year property

		/// <summary>
		/// Gets or sets the Year value.
		/// </summary>
		public uint Year
		{
			get { return GetValue<uint>(YearProperty); }
			set { SetValue(YearProperty, value); }
		}

		/// <summary>
		/// Year property data.
		/// </summary>
		public static readonly PropertyData YearProperty = RegisterProperty("Year", typeof(uint));

		#endregion

		#region Duration property

		/// <summary>
		/// Gets or sets the Duration value.
		/// </summary>
		public TimeSpan Duration
		{
			get { return GetValue<TimeSpan>(DurationProperty); }
			set { SetValue(DurationProperty, value); }
		}

		/// <summary>
		/// Duration property data.
		/// </summary>
		public static readonly PropertyData DurationProperty = RegisterProperty("Duration", typeof(TimeSpan));

		#endregion

		#region Tags property

		/// <summary>
		/// Gets or sets the Tags value.
		/// </summary>
		public List<Tag> Tags
		{
			get { return GetValue<List<Tag>>(TagsProperty); }
			set { SetValue(TagsProperty, value); }
		}

		/// <summary>
		/// Tags property data.
		/// </summary>
		public static readonly PropertyData TagsProperty = RegisterProperty("Tags", typeof(List<Tag>));

		#endregion

		#region Genres property

		/// <summary>
		/// Gets or sets the Genres value.
		/// </summary>
		public List<Genre> Genres
		{
			get { return GetValue<List<Genre>>(GenresProperty); }
			set { SetValue(GenresProperty, value); }
		}

		/// <summary>
		/// Genres property data.
		/// </summary>
		public static readonly PropertyData GenresProperty = RegisterProperty("Genres", typeof(List<Genre>));

		#endregion

		#region Path property

		/// <summary>
		/// Gets or sets the Path value.
		/// </summary>
		public string Path
		{
			get { return GetValue<string>(PathProperty); }
			set { SetValue(PathProperty, value); }
		}

		/// <summary>
		/// Path property data.
		/// </summary>
		public static readonly PropertyData PathProperty = RegisterProperty("Path", typeof(string));

		#endregion

		public string GetStringTags(List<Tag> tags)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Tag tag in tags)
			{
				sb.Append(tag + ", ");
			}
			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}

		public string GetStringGenres(List<Genre> genres)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Genre genre in genres)
			{
				sb.Append(genre + ", ");
			}
			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
	}
}
