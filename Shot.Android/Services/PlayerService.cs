using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Shot.Enumerations;
using Shot.Extensions;
using Shot.Services;
using Shot.ViewModels;
using Xamarin.Forms;

[assembly: Dependency(typeof(Shot.Droid.Services.PlayerService))]
namespace Shot.Droid.Services
{
    [Service(Exported = true)]
    public class PlayerService : Service, IPlayerService, IMessageSender
    {
        MediaPlayer Player;
        MediaMetadataRetriever _mmr;
        private ConsistentPlayerViewModel _vm => Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Last().BindingContext as ConsistentPlayerViewModel;
        public RecordingStatus Status { get; set; }
        public string FilePath { get; set; }

        public void Pause()
        {
            if (Player == null)
                return;

            Player.Pause();
            Status = RecordingStatus.Paused;
        }

        public void Play()
        {
            if (Player == null)
                return;

            Player.Start();
        }

        public void SeekTo(int msec, bool isRelative)
        {
            if (Player == null)
                return;

            if (isRelative)
            {
                var newPosition = Player.CurrentPosition + msec;
                Player.SeekTo(newPosition);
            }
            else
            {
                Player.SeekTo(msec);
            }
        }

        public int GetCurrentPosition()
        {
            if (Player == null)
                return 0;

            return Player.CurrentPosition;
        }

        public void SetPlayer(string filePath)
        {
            FilePath = filePath;
            Player = new MediaPlayer();
            Player.SetDataSource(FilePath);
            Player.Prepare();

            Status = RecordingStatus.Running;
            HandleConsistentPlayer();
            SetTimer();
            MessagingCenter.Send<IMessageSender, string>(this, "CPI", filePath);
        }

        public int GetMetaDataDuration(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return 0;

            _mmr = new MediaMetadataRetriever();
            _mmr.SetDataSource(filePath);
            ushort.TryParse(_mmr.ExtractMetadata(MetadataKey.Duration), out var result);
            return result;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        private void HandleConsistentPlayer()
        {
            MessagingCenter.Subscribe<IMessageSender, string>(this, "PlayPause", (e, args) =>
            {
                bool.TryParse(args, out var isPlaying);
                if (isPlaying)
                {
                    Pause();
                }
                else
                {
                    Play();
                    SetTimer();
                }
            });
        }

        private void SetTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                if (typeof(PlayerViewModel) != _vm.GetType() && Status == RecordingStatus.Running)
                {
                    MessagingCenter.Send<IMessageSender, string>(this, "PT", GetCurrentPosition().ToString());
                    if (GetMetaDataDuration(FilePath).Equals(GetCurrentPosition()))
                    {
                        MessagingCenter.Send<IMessageSender>(this, "NextRecording");
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }
    }
}
