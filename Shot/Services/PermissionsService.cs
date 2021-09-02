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

        public async Task<bool> CheckOrRequestMicrophonePermission()
        {
            var permissionStatus = await _permissions.CheckStatusAsync<Permissions.Microphone>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await _permissions.RequestAsync<Permissions.Microphone>();
            }
            return permissionStatus == PermissionStatus.Granted;
        }
    }

    public interface IPermissionsService
    {
        Task<bool> CheckOrRequestMicrophonePermission();
    }
}
