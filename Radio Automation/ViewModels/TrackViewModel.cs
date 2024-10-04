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
		}

		#region Overrides of ViewModelBase

		/// <inheritdoc />
		protected override Task InitializeAsync()
		{
			TrackPlayingMessage.Register(this, TrackPlaying);
			return Task.CompletedTask;
		}

		#endregion

		private void TrackPlaying(TrackPlayingMessage message)
		{
			if (Track == message.Data.Track)
			{
				IsPlaying = message.Data.IsPlaying;
			}
		}

		#region Overrides of ViewModelBase

		/// <inheritdoc />
		protected override Task OnClosingAsync()
		{
			TrackPlayingMessage.Unregister(this, TrackPlaying);
			return Task.CompletedTask;
		}

		#endregion

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
		public static readonly IPropertyData TrackProperty = RegisterProperty<Track>(nameof(Track));

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
		public static readonly IPropertyData IsPlayingProperty = RegisterProperty<bool>(nameof(IsPlaying));

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
		public static readonly IPropertyData IsSelectedProperty = RegisterProperty<bool>(nameof(IsSelected));

		#endregion


	}
}
