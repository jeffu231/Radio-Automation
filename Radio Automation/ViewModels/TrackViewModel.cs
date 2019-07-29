using System;
using System.Threading.Tasks;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Radio_Automation.Messaging;
using Radio_Automation.Models;

namespace Radio_Automation.ViewModels
{
	public class TrackViewModel:ViewModelBase
	{
		public TrackViewModel(Track track)
		{
			Argument.IsNotNull("track", track);
			Track = track;
			TrackPlayingMessage.Register(this, TrackPlaying);
		}

		private void TrackPlaying(TrackPlayingMessage message)
		{
			if (Track == message.Data.Track)
			{
				IsPlaying = message.Data.IsPlaying;
			}
		}

		#region Track model property

		/// <summary>
		/// Gets or sets the Track value.
		/// </summary>
		[Model]
		public Track Track
		{
			get { return GetValue<Track>(TrackProperty); }
			private set { SetValue(TrackProperty, value); }
		}

		/// <summary>
		/// Track property data.
		/// </summary>
		public static readonly PropertyData TrackProperty = RegisterProperty("Track", typeof(Track));

		#endregion

		#region IsPlaying property

		/// <summary>
		/// Gets or sets the IsPlaying value.
		/// </summary>
		public bool IsPlaying
		{
			get { return GetValue<bool>(IsPlayingProperty); }
			set { SetValue(IsPlayingProperty, value); }
		}

		/// <summary>
		/// IsPlaying property data.
		/// </summary>
		public static readonly PropertyData IsPlayingProperty = RegisterProperty("IsPlaying", typeof(bool));

		#endregion

		#region IsSelected property

		/// <summary>
		/// Gets or sets the IsSelected value.
		/// </summary>
		public bool IsSelected
		{
			get { return GetValue<bool>(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}

		/// <summary>
		/// IsSelected property data.
		/// </summary>
		public static readonly PropertyData IsSelectedProperty = RegisterProperty("IsSelected", typeof(bool));

		#endregion


	}
}
