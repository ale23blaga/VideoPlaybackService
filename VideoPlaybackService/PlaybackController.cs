using System.Diagnostics;

namespace VideoPlaybackService
{
    public enum PlaybackStatus 
    { 
        Idle, Playing, Error    
    }

    public class PlaybackState
    {
        public PlaybackStatus Status { get; set; } = PlaybackStatus.Idle;
        public string? CurrentVideo { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public class PlaybackController
    {

        
        private Process? playerProcess;
        private readonly PlaybackState state = new();
        private readonly object locker = new();

        public PlaybackState GetState() => state;
            
        public PlaybackState Play(string videoPath)
        {
            lock(locker)
            {
                if (!File.Exists(videoPath))
                {
                    state.Status = PlaybackStatus.Error;
                    state.CurrentVideo = null;
                    state.ErrorMessage = $"Video file not found: {videoPath}";
                    return state;
                }

                StopProcess();

                try
                {
                    playerProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "wmplayer.exe",
                        Arguments = $"\"{videoPath}\" /fullscreen",
                        UseShellExecute = true
                    });

                    state.Status = PlaybackStatus.Playing;
                    state.CurrentVideo = videoPath;
                    state.ErrorMessage = null;
                }
                catch (Exception ex)
                {
                    state.Status = PlaybackStatus.Error;
                    state.ErrorMessage = $"Failed to start player: {ex.Message}";
                }

                return state;
            }
        }

        public PlaybackState Stop()
        {
            lock( locker)
            {
                StopProcess();
                return state;
            }
        }

        private void StopProcess()
        {
            try
            {
                if (playerProcess != null && !playerProcess.HasExited)
                {
                    playerProcess.Kill();
                    playerProcess.Dispose();
                }
            }
            catch { }
            finally
            {
                playerProcess = null;
                state.Status = PlaybackStatus.Idle;
                state.CurrentVideo = null;
                state.ErrorMessage = null;
            }
        }

    }
}
