using System;
using Shot.Enumerations;
using Shot.Services;

[assembly: Xamarin.Forms.Dependency(typeof(Shot.iOS.Services.RecordingService))]
namespace Shot.iOS.Services
{
    public class RecordingService : IRecordingService
    {
        public MediaStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Start(string fileNameForRecording)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
