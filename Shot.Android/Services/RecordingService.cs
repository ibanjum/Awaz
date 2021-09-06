using System;
using System.Threading.Tasks;
using Android.Media;
using Android.Util;
using Java.IO;
using Shot.Enumerations;
using Shot.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Shot.Droid.Services.RecordingService))]
namespace Shot.Droid.Services
{
    public class RecordingService : IRecordingService
    {
        MediaRecorder _recorder;

        public RecordingService()
        {

        }

        public RecordingStatus Status { get; set; }

        public string FilePath { get; set; }

        public int? MaxAmplitude => _recorder?.MaxAmplitude;

        public void Pause()
        {
            if (_recorder == null)
                return;

            _recorder.Pause();
            Status = RecordingStatus.Paused;
        }

        public void Resume()
        {
            if (_recorder == null)
                return;

            _recorder.Resume();
            Status = RecordingStatus.Running;
        }

        public void Start(string fileNameForRecording)
        {
            if (string.IsNullOrEmpty(fileNameForRecording))
                return;
            FilePath = fileNameForRecording;
            InitRecorder();
            _recorder.SetOutputFile(FilePath);
            _recorder.Prepare();
            _recorder.Start();
            Status = RecordingStatus.Running;
        }

        public void Stop()
        {
            if (_recorder == null)
            {
                return;
            }
            _recorder.Stop();
            _recorder.Release();
            _recorder = null;
            Status = RecordingStatus.Stopped;
        }

        private void InitRecorder()
        {
            _recorder = new MediaRecorder();
            _recorder.SetAudioSource(AudioSource.Mic);
            _recorder.SetOutputFormat(OutputFormat.ThreeGpp);
            _recorder.SetAudioEncoder((AudioEncoder.AmrNb));
        }
    }
}
