# Radio-Automation

Radio Automation is a purpose built audio player designed to be a off hours music player for my animated light display during the holidays. 

<img src="Assets/ra_main_screen.png" width=600 />

## Features

### Player

Support for playing most media types supported on Windows. There is a simple playlist that utilizes the track ID3 info. Playlists can be populated from selecting specific tracks, M3U playlists, Zara playlist, or an internet stream. There is a basic function to shuffle tracks on each trip through the playlist.

### Scheduler

Support for basic scheduling in a cron like format to schedule things like bumpers. It has support for Time and Temperature audio files in the form that Zara utilized. Time and Temp are source from Wunderground via their API. You will need an API key and the station id for the location you want to use. Items like play, stop, time, temp are supported in an immediate or delayed fashion. If delayed, it will play between tracks. Immediate will interupt. One pane in the main display shows upcoming events.

### MQTT

Support for triggering the same scheduled type events from an MQTT topic. 

### Audio Output

Support for choosing the audio output device on the system.

### UI

The simple UI has sections to display basic info. The now playing section shows the current playing track info, remaining time, end time audio level meter and a volume control. The up next section shows the next track scheduled to play, the current time and weather info. The event section shows upcoming events. The playlist section shows the entire playlist and highlights which track is currently playing.

#### License

Software is released under the GPL v3 and is free to use under those terms.

