using System;
using System.Globalization;
using Android.Media;
using Ninject;
using Shot.Enumerations;
using Shot.Services;
using Shot.ViewModels;
using Xamarin.Forms;

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

        }

        public void Pause()
        {
            if (_player == null)
                return;

            _player.Pause();
            Status = RecordingStatus.Paused;
        }

        public void Play()
        {
            if (_player == null)
                return;

            _player.Start();
            Status = RecordingStatus.Running;
        }

        public void SeekTo(int msec, bool isRelative)
        {
            if (_player == null)
                return;

            if (isRelative)
            {
                var newPosition = _player.CurrentPosition + msec;
                _player.SeekTo(newPosition);
            }
            else
            {
                _player.SeekTo(msec);
            }
        }

        public int GetCurrentPosition()
        {
            if (_player == null)
                return 0;

            return _player.CurrentPosition;
        }

        public void SetPlayer(string filePath)
        {
            _player = new MediaPlayer();
            _player.SetDataSource(filePath);
            _player.Prepare();
            Status = RecordingStatus.Stopped;
        }

        public int GetMetaDataDuration(string filePath)
        {
            _mmr = new MediaMetadataRetriever();
            _mmr.SetDataSource(filePath);
            ushort.TryParse(_mmr.ExtractMetadata(MetadataKey.Duration), out var result);
            return result;
        }
    }
}
