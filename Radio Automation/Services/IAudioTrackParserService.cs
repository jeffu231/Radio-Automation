using System.Collections.Generic;
using Radio_Automation.Models;

namespace Radio_Automation.Services
{
	public interface IAudioTrackParserService
	{
		List<Track> RetrieveAudioTracksInPath(string path, bool recurse);

		Track RetrieveAudioTrackFromFile(string path);
	}
}
