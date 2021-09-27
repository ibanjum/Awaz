using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using Shot.Enumerations;
using Shot.Services;
using Shot.ViewModels;
using Xamarin.Forms;

[assembly: Dependency(typeof(Shot.Droid.Services.RecordingService))]
namespace Shot.Droid.Services
{
    [Service(Exported = true)]
    public class RecordingService : Service, IRecordingService, IMessageSender
    {
        static private string _filePath;
        static private MediaRecorder _recorder;
        static private Intent _recordingService;
        public static readonly string StopMessage = "STOP";
        public static readonly string StatusPayload = "RECORDING_STATUS";
        private readonly string CHANNEL_ID = "1001";
        public int? MaxAmplitude => _recorder?.MaxAmplitude;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            _recorder = new MediaRecorder();
            _recorder.SetAudioSource(AudioSource.Mic);
            _recorder.SetOutputFormat(OutputFormat.ThreeGpp);
            _recorder.SetAudioEncoder(AudioEncoder.AmrNb);
            _recorder.SetOutputFile(_filePath);
            _recorder.Prepare();
            _recorder.Start();

            MessagingCenter.Send<IMessageSender, string>(this, "RS", MediaStatus.Running.ToString());

            return StartCommandResult.NotSticky;
        }

        public override void OnCreate()
        {
            CreateNotificationChannel();
            Notification("Title", "Notificaiton Message", "Ticker", 101);
            base.OnCreate();
        }

        public override void OnDestroy()
        {
            _recorder.Stop();
            _recorder.Release();
            _recorder = null;
            MessagingCenter.Send<IMessageSender, string>(this, "RS", MediaStatus.Stopped.ToString());
            // var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            //notificationManager.Cancel(NOTIFICATION_ID);
            base.OnDestroy();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public void Pause()
        {
            if (_recorder == null)
                return;

            _recorder.Pause();
            MessagingCenter.Send<IMessageSender, string>(this, "RS", MediaStatus.Paused.ToString());
        }

        public void Resume()
        {
            if (_recorder == null)
                return;

            _recorder.Resume();
            MessagingCenter.Send<IMessageSender, string>(this, "RS", MediaStatus.Running.ToString());
        }

        public void Start(string fileNameForRecording)
        {
            if (string.IsNullOrEmpty(fileNameForRecording))
                return;

            _filePath = fileNameForRecording;

            _recordingService = new Intent(MainActivity.Context, typeof(RecordingService));
            MainActivity.Context.StartService(_recordingService);
        }

        public void Stop()
        {
            if (_recorder == null)
                return;
            MainActivity.Context.StopService(_recordingService);
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = new NotificationChannel(CHANNEL_ID, "asd", NotificationImportance.Default);
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }


        //Notification Builder

        private void Notification(string title, string msg, string ticker, int notifId)
        {
            /*var playPauseIntent = new Intent(MainActivity.Context, typeof(RecordingReceiver));
            //var timerNotificationIntentValue = this.GetTimerNotificationIntentValue(timerAction);
            playPauseIntent.PutExtra("timerNotification", 3);

            const int playPauseIntentId = 0;
            var playPausePendingIntent = PendingIntent.GetBroadcast(MainActivity.Context, playPauseIntentId, playPauseIntent, PendingIntentFlags.UpdateCurrent);

            _widgetLayout = new RemoteViews(PackageName, Resource.Layout.mainx);
            _widgetLayout.SetTextViewText(Resource.Id.textView1, "nothing");
            _widgetLayout.SetOnClickPendingIntent(Resource.Id.button1, playPausePendingIntent);*/

            Intent intent = new Intent(MainActivity.Context, typeof(MainActivity));

            PendingIntent pendingIntent = PendingIntent.GetActivity(MainActivity.Context, 2, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("Awaz")
                .SetDefaults((int)NotificationDefaults.Sound)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetSmallIcon(Resource.Drawable.record);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(notifId, builder.Build());

            MessagingCenter.Subscribe<IMessageSender, string>(this, "TT", (s, e) =>
            {
                builder.SetContentTitle(string.Format("Recording • {0}", e));
                notificationManager.Notify(notifId, builder.Build());
            });
        }
    }
}
