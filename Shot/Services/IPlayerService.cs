using System;
using Shot.Enumerations;

namespace Shot.Services
{
    public interface IPlayerService
    {
        void Play();
        void Pause();
        void SeekTo(int msec, bool isRelative);
        int GetCurrentPosition();
        void SetPlayer(string filePath);
        int GetMetaDataDuration(string filePath);
        void Stop();
    }
}
