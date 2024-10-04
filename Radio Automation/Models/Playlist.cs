using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Collections;
using Catel.Data;

namespace Radio_Automation.Models
{
	public class Playlist:SavableModelBase<Playlist>
	{
		public Playlist():this(new List<Track>())
		{
		}

		public Playlist(List<Track> tracks)
		{
			Id = Guid.NewGuid();
			Tracks = new FastObservableCollection<Track>(tracks);
		}

		private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			TotalTime = CalculateTotalTime();
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
		public static readonly IPropertyData IdProperty = RegisterProperty<Guid>(nameof(Id));

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
		public static readonly IPropertyData NameProperty = RegisterProperty<string>(nameof(Name));

		#endregion

		#region TotalTime property

		/// <summary>
		/// Gets or sets the TotalTime value.
		/// </summary>
		public TimeSpan TotalTime
		{
			get { return GetValue<TimeSpan>(TotalTimeProperty); }
			set { SetValue(TotalTimeProperty, value); }
		}

		/// <summary>
		/// TotalTime property data.
		/// </summary>
		public static readonly IPropertyData TotalTimeProperty = RegisterProperty<TimeSpan>(nameof(TotalTime));

		#endregion

		#region Tracks property

		/// <summary>
		/// Gets or sets the Tracks value.
		/// </summary>
		public FastObservableCollection<Track> Tracks
		{
			get { return GetValue<FastObservableCollection<Track>>(TracksProperty); }
			protected set
			{
				var oldT = Tracks;
				if (oldT != null)
				{
					oldT.CollectionChanged -= Tracks_CollectionChanged;
				}
				SetValue(TracksProperty, value);
				CalculateTotalTime();
				Tracks.CollectionChanged += Tracks_CollectionChanged;
			}
		}

		/// <summary>
		/// Tracks property data.
		/// </summary>
		public static readonly IPropertyData TracksProperty = RegisterProperty<FastObservableCollection<Track>>(nameof(Tracks));

		#endregion
		
		internal TimeSpan CalculateTotalTime()
		{
			var ticks = Tracks.Sum(t => t.Duration.Ticks);
			return TimeSpan.FromTicks(ticks);
		}
	}
}
