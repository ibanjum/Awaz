using System.Threading.Tasks;
using Shot.Enumerations;

namespace Shot.Services
{
    public interface IRecordingService
    {
        void Start(string fileNameForRecording);
        void Stop();
        void Pause();
        void Resume();
        string FilePath { get; set; }
        RecordingStatus Status { get; set; }
        int? MaxAmplitude { get; }
    }
}
