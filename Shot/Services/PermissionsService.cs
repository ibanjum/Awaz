using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Shot.Services
{
    public class PermissionsService : IPermissionsService
    {
        private readonly IPermissions _permissions;

        public PermissionsService(IPermissions permissions)
        {
            _permissions = permissions;
        }

        public async Task<bool> CheckOrRequestMicrophoneAndSpeechPermission()
        {
            var microphoneStatus = await _permissions.CheckStatusAsync<Permissions.Microphone>();
            if (microphoneStatus != PermissionStatus.Granted)
            {
                microphoneStatus = await _permissions.RequestAsync<Permissions.Microphone>();
            }

            var speechStatus = await _permissions.CheckStatusAsync<Permissions.Speech>();

            if (speechStatus != PermissionStatus.Granted)
            {
                speechStatus = await _permissions.RequestAsync<Permissions.Speech>();
            }
            return microphoneStatus == PermissionStatus.Granted && speechStatus == PermissionStatus.Granted;
        }
    }

    public interface IPermissionsService
    {
        Task<bool> CheckOrRequestMicrophoneAndSpeechPermission();
    }
}
