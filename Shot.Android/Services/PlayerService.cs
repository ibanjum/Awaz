using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Shot.Enumerations;
using Shot.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Shot.Droid.Services.PlayerService))]
namespace Shot.Droid.Services
{
    [Service(Exported = true)]
    public class PlayerService : Service, IPlayerService, IMessageSender
    {
        static MediaPlayer _player;
        MediaMetadataRetriever _mmr;
        static string _filePath;
        static private Intent _playerService;

        public void Pause()
        {
            if (_player == null)
                return;

            _player.Pause();
            MessagingCenter.Send<IMessageSender, string>(this, "PS", MediaStatus.Paused.ToString());
        }

        public void Play()
        {
            if (_player == null)
                return;

            _player.Start();
            MessagingCenter.Send<IMessageSender, string>(this, "PS", MediaStatus.Running.ToString());
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
            if (string.IsNullOrEmpty(filePath))
                return;

            _filePath = filePath;
            _playerService = new Intent(MainActivity.Context, typeof(PlayerService));
            MainActivity.Context.StartService(_playerService);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            _player = new MediaPlayer();
            _player.SetDataSource(_filePath);
            _player.Prepare();
            Play();

            return StartCommandResult.NotSticky;
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

        public void Stop()
        {
            MainActivity.Context.StopService(_playerService);
        }

        public override void OnDestroy()
        {
            _player.Stop();
            base.OnDestroy();

        }
    }
}
