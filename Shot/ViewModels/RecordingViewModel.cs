using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Shot.Enumerations;
using Shot.Extensions;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Shot.ViewModels
{
    public class RecordingViewModel : BaseViewModel, IMessageSender
    {
        private readonly IRecordingService _recordingService;
        private readonly INavigationService _navigationService;
        private readonly IPermissionsService _permissionsService;
        private readonly IDevicePlatform _devicePlatform;

        public string Title => AppResources.RecordingStartLabel;
        public string RecordingsListText => AppResources.RecordingsListText;
        public string CompleteRecordingCommandText => AppResources.DoneLabel;

        public string RecordPauseImageSource
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public string RecordingText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public bool IsDoneCommandEnabled
        {
            get { return GetPropertyValue<bool>(); }
            set { SetPropertyValue(value); }
        }

        public string TimeText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public ObservableCollection<ISeries> Series
        {
            get { return GetPropertyValue<ObservableCollection<ISeries>>(); }
            set { SetPropertyValue(value); }
        }

        public IEnumerable<ICartesianAxis> YAxes
        {
            get { return GetPropertyValue<IEnumerable<ICartesianAxis>>(); }
            set { SetPropertyValue(value); }
        }

        public IEnumerable<ICartesianAxis> XAxes
        {
            get { return GetPropertyValue<IEnumerable<ICartesianAxis>>(); }
            set { SetPropertyValue(value); }
        }

        public MediaStatus RecordingStatus
        {
            get { return GetPropertyValue<MediaStatus>(); }
            set
            {
                SetPropertyValue(value);
                IsDoneCommandEnabled = RecordingStatus != MediaStatus.Stopped;
                RecordPauseImageSource = RecordingStatus == MediaStatus.Running ? ImageNames.PlayerPauseImage : ImageNames.RecordImage;
            }
        }

        public ICommand RecordPauseCommand => new Command(OnPlayPausePressed);
        public ICommand CompleteRecordingCommand => new Command(OnRecordingDonePressed);
        public ICommand SettingsCommand => new Command(OnSettingsPressed);

        private readonly ObservableCollection<double> _observableValues;
        private readonly ObservableCollection<double> _negativeObservableValues;
        private bool isMediaRecorderOn;
        Stopwatch _stopwatch;

        public RecordingViewModel(
            INavigationService navigationService,
            IPermissionsService permissionsService,
            IDevicePlatform devicePlatform) : base(navigationService)
        {
            _navigationService = navigationService;
            _permissionsService = permissionsService;
            _devicePlatform = devicePlatform;
            _recordingService = DependencyService.Get<IRecordingService>();
            _observableValues = new ObservableCollection<double>();
            _negativeObservableValues = new ObservableCollection<double>();

            if (_devicePlatform.Platform == PlaformEnum.Android)
            {
                SubscribeRecordingStatus();
            }

            Series = GraphExtension.CreateGraph(_observableValues, _negativeObservableValues);
            YAxes = new Axis[] { new Axis() { ShowSeparatorLines = false, Labeler = (value) => string.Empty } };
            XAxes = new Axis[] { new Axis() { Labeler = (value) => string.Empty, MaxLimit = 100 } };

            _stopwatch = new Stopwatch();
            isMediaRecorderOn = true;
            RecordingStatus = MediaStatus.Stopped;
            SetRecordingTimer();
        }

        private async void OnPlayPausePressed()
        {
            switch (RecordingStatus)
            {
                case MediaStatus.Running:
                    _recordingService.Pause();
                    _stopwatch.Stop();
                    break;
                case MediaStatus.Paused:
                    _recordingService.Resume();
                    _stopwatch.Start();
                    break;
                case MediaStatus.Stopped:
                    var popupModel = new PopupModel(AppResources.SaveRecordingLabel, AppResources.EnterNameLabel, AppResources.SaveLabel);
                    var fileName = await _navigationService.DisplayPrompt(popupModel);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var ifPermissionGranted = await _permissionsService.CheckOrRequestMicrophoneAndSpeechPermission();
                        if (ifPermissionGranted)
                        {
                            var filePath = FileExtension.GetFilePath(fileName);
                            _recordingService.Start(filePath);
                            _stopwatch.Start();
                            await SecureStorage.SetAsync(ConsStrings.RecordingStartedKey, filePath);
                        }
                    }
                    break;
            }
        }

        private void OnRecordingDonePressed()
        {
            _recordingService.Pause();
            _recordingService.Stop();
            _stopwatch.Reset();
            Series.Clear();
            SecureStorage.Remove(ConsStrings.RecordingStartedKey);
        }

        private void SetRecordingTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(400), () =>
            {
                if (isMediaRecorderOn)
                {
                    TimeText = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                    if (RecordingStatus != MediaStatus.Paused)
                    {
                        var amplitude = _recordingService.MaxAmplitude;
                        if (amplitude != null)
                        {
                            if (_observableValues.Count > 100)
                            {
                                _observableValues.RemoveAt(0);
                                _negativeObservableValues.RemoveAt(0);
                            }
                            var negativeAmplitude = -amplitude;
                            _observableValues.Add((double)amplitude);
                            _negativeObservableValues.Add((double)negativeAmplitude);
                        }
                        if (_devicePlatform.Platform == PlaformEnum.Android)
                        {
                            MessagingCenter.Send<IMessageSender, string>(this, "TT", TimeText);
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        private async void OnSettingsPressed()
        {
            await _navigationService.NavigateTo<SettingsViewModel>();
        }

        private void SubscribeRecordingStatus()
        {
            MessagingCenter.Subscribe<IMessageSender, string>(this, "RS", (s, e) =>
            {
                RecordingStatus = (MediaStatus)Enum.Parse(typeof(MediaStatus), e);
            });
        }
    }
}
