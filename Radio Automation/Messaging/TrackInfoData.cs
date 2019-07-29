using Radio_Automation.Models;

namespace Radio_Automation.Messaging
{
	public class TrackInfoData
	{
		public TrackInfoData(Track track, bool isPlaying)
		{
			Track = track;
			IsPlaying = isPlaying;
		}

		public Track Track { get; }

		public bool IsPlaying { get; }
	}
}
