using Radio_Automation.Models;

namespace Radio_Automation.Messaging;

public class PlaylistMessageData
{
    public PlaylistMessageData(Track track, PlaylistAction action)
    {
        Track = track;
        PlaylistAction = action;
    }
    public Track Track { get; set; }

    public PlaylistAction PlaylistAction { get; set; } = PlaylistAction.None;
}

public enum PlaylistAction
{
    Add,
    Remove,
    Edit,
    None
}