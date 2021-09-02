using System;
using Shot.Enumerations;

namespace Shot.Services
{
    public interface IPlayerService
    {
        void Play(string filePath);
        void Pause();
        void Resume();
        string GetDuration(string filePath);
        RecordingStatus Status { get; set; }
        void SeekTo(int sec, bool isForwad);
        string GetCurrentPosition();
    }
}
