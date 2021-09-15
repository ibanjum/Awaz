using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using Ninject;
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

            MessagingCenter.Send<IMessageSender, string>(this, "RS", RecordingStatus.Running.ToString());

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
            MessagingCenter.Send<IMessageSender, string>(this, "RS", RecordingStatus.Stopped.ToString());
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
            MessagingCenter.Send<IMessageSender, string>(this, "RS", RecordingStatus.Paused.ToString());
        }

        public void Resume()
        {
            if (_recorder == null)
                return;

            _recorder.Resume();
            MessagingCenter.Send<IMessageSender, string>(this, "RS", RecordingStatus.Running.ToString());
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
            channel.Description = "bro";

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }


        //Notification Builder

        private void Notification(string title, string msg, string ticker, int notifId)
        {

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle(title)
                .SetContentText(msg)
                .SetTicker(ticker)
                .SetDefaults((int)NotificationDefaults.Sound)
                .SetVisibility((int)NotificationVisibility.Public)
                .SetSmallIcon(Resource.Drawable.record);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(notifId, builder.Build());
        }
    }
}
