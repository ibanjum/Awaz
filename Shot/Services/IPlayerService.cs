using System;
using Shot.Enumerations;

namespace Shot.Services
{
    public interface IPlayerService
    {
        void Play();
        void Pause();
        RecordingStatus Status { get; set; }
        string FilePath { get; set; }
        void SeekTo(int msec, bool isRelative);
        int GetCurrentPosition();
        void SetPlayer(string filePath);
        int GetMetaDataDuration(string filePath);
    }
}
