using Catel.Messaging;

namespace Radio_Automation.Messaging
{
	public class TrackPlayingMessage: MessageBase<TrackPlayingMessage, TrackInfoData>
	{
		public TrackPlayingMessage()
		{
			
		}

		public TrackPlayingMessage(TrackInfoData data):base(data)
		{
			
		}
	}
}
