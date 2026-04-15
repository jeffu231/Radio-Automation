# MQTT Song Publish Feature

## Overview

The current application is able to publish the current playing song information to a specified file on the file system.
The file name is specified in the Prefences panel and each time a new song is played, the song title and artist are written to the file.
It is desireable to be able to publish this same information to the configured MQTT broker. An MQTT broker is already a configurable option 
in the Preferences panel to accept event commands from outside sources.

## Implementation

* Add the ability to publish the current song information to the configured MQTT broker using the existing MQTT libraries.
* Create a new section in the Prefenences panel to hold the settings for saving the song info for each song played.
* Inside the new section in the preference panel, add a new option to enable/disable the file system saving of the song information.
* Move the current song file path textbox into the new section in the Preferences panel below an option to enable it.
* Inside the new section in the preference panel, add a new option to enable/disable the MQTT publishing of the song information.
* Inside the new section in the preference panel below the enable/disable of the MQTT song publishing, 
  add a new textbox to specify the MQTT topic to publish the song information to.
* Add logic to validate the MQTT topic name is in proper format.
* Add logic to publish the same song title and artist information to the MQTT broker under the specific topic if it is enabled. 
* The song info published to MQTT should be in json format.
* Update the logic to publish the song information to the file system if it is enabled.
* Update the settings to persist the new preference settings.