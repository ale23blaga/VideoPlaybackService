# VideoPlaybackService

A small HTTP service that controls video playback on a local machine, built as a prototype to support automated vision system testing.

## Overview
Instead of placing physical food replicas under a camera every time you want to run a test, you put a screen in front of the camera and play a video of the food waste event. This service lets automated tests trigger the playback over HTTP, so that the tests can be scripted and repeated without any manual work.

The process is the following:
Auttomated Test -> HTTP Request -> VideoPlaybackService -> Wmplayer.exe -> Screen -> Camera

## Project structure
~~~
VideoPlaybackService/
    - Program.cs
    - PlaybackController.cs
~~~
`PlaybackController.cs` holds all the logic, it launches the player, it stops it and it keeps track of wheter the system is idle, playing or in an error state. `Program.cs` sets up the API and defines the endpoints.

## API

| Method | Endpoint  | Body | Description|
|---|---|---|---|
|`POST`|`/play`| {"videoFile": "C:\videos\scenario.mp4"} | Starts playing the video |
| `STOP`| `/stop`| - | Stops current playback|
| `GET` | `/status` | - | Gets current state|

### Example for status response

~~~
{ 
    "status": "Playing"
    "currentVideo": "C:\videos\scenario.mp4"
    "errorMesage: null
}
~~~

Status can be either `Idle`, `Playing` or `Error`.

## Design decisions

**Repeated requests**: if `/play` is called while something is already playing, the current video is stopped first and the new one starts. No request gets rejected.

**Concurrency**: the shared state is protected with a lock (`locker`) so that two requests that happen at the same time donnot cause issues.

**Error handling**: if the video file does not exist, the servie returns a 400 with an error message.

**State tracking**: `/status` lets a test check if the system is idle before triggering the next scenario, which makes sequencing tests better.

**Video seelection**: the test passes the full file path in the request body. 

## How to run

### Requirements
- .NET 8 SDK
- Windows (the service uses `wmplayer.exe`)
- A `.mp4` file to test with.

### Start the service

Open in Video Studio and press F5. The port that the service starts on will be shown in the console when the service starts.

## Limitations
- There is **no way to detect** when a video finishes, the service only knows if it is stopped when `/stop` is called
- State is in the memory, if the service restars it is also reset.
- The player controll is basic, we just launch and kill `wmplayer.exe`, there is no pause or seek.
- File paths have to be absolute, which means that the test caller needs to know where the videos are.

## Future improvements
- It would be better to switch from wmplayer to a more capable video player that allows play, pause seek and detection when a video ends on its own, and also one that works across different operating systems, not just Windows.
- A config file that acts as a lookup table, mapping readable names like `"fruits"` to their respective video file paths, so tests don't need to hardcode full paths, and also adding new tests scenarios is just adding a new entry to that file.
- Automatic state reset to `Idle` when a video finishes playing.
