using System;
using Shot.Enumerations;
using Xamarin.Essentials;

namespace Shot.Services
{
    public class DevicePlatform : IDevicePlatform
    {
        public PlaformEnum Platform
        {
            get
            {
                if (DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
                {
                    return PlaformEnum.Android;
                }

                if (DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                {
                    return PlaformEnum.iOS;
                }

                return PlaformEnum.Unknown;
            }
        }
    }

    public interface IDevicePlatform
    {
        PlaformEnum Platform { get; }
    }
}
