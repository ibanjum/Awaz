using System;
using System.Globalization;
using Android.Media;
using Shot.Enumerations;
using Shot.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Shot.Droid.Services.PlayerService))]
namespace Shot.Droid.Services
{
    public class PlayerService : IPlayerService
    {
        MediaPlayer _player;
        MediaMetadataRetriever _mmr;

        public RecordingStatus Status { get; set; }

        public PlayerService()
        {
            _player = new MediaPlayer();
            _player.Reset();
            _mmr = new MediaMetadataRetriever();
            Status = RecordingStatus.Stopped;
        }

        public void Pause()
        {
            _player.Pause();
            Status = RecordingStatus.Paused;
        }

        public void Play(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                _player.SetDataSource(filePath);
                _player.Prepare();
                _player.Start();
                Status = RecordingStatus.Running;
            }
        }

        public void Resume()
        {
            _player.Start();
            Status = RecordingStatus.Running;
        }

        public string GetDuration(string filePath)
        {
            _mmr.SetDataSource(filePath);
            var ticks = ushort.Parse(_mmr.ExtractMetadata(MetadataKey.Duration));
            var timeSpan = TimeSpan.FromMilliseconds(ticks);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        public void SeekTo(int sec, bool isForward)
        {
            var seekTo = isForward ? (_player.CurrentPosition + (1000 * sec)) : (_player.CurrentPosition - (1000 * sec));
            _player.SeekTo(seekTo);
        }

        public string GetCurrentPosition()
        {
            return TimeSpan.FromMilliseconds(_player.CurrentPosition).ToString(@"hh\:mm\:ss");
        }
    }
}
