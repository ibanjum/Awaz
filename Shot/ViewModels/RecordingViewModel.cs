using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Shot.Enumerations;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using Xamarin.Forms;

namespace Shot.ViewModels
{
    public class RecordingViewModel : BaseViewModel
    {
        private readonly IRecordingService _recordingService;
        private readonly INavigationService _navigationService;
        private readonly IPermissionsService _permissionsService;

        Stopwatch _stopwatch;

        public string Title => AppResources.AppTitle;
        public string RecordingsListText => AppResources.RecordingsListText;
        public string StopText => AppResources.StopText;

        public string RecordingText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public bool IsStopCommandEnabled
        {
            get { return GetPropertyValue<bool>(); }
            set { SetPropertyValue(value); }
        }

        public string TimeText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public RecordingStatus RecordingStatus
        {
            get { return GetPropertyValue(_recordingService.Status); }
            set
            {
                SetPropertyValue(value);
                SetRecordButtonText(RecordingStatus);
                IsStopCommandEnabled = RecordingStatus != RecordingStatus.Stopped;
                RecordCommand = new Command(OnRecordPressed);
            }
        }

        public ICommand RecordCommand { get; set; }
        public ICommand RecordingsListCommand => new Command(RecordingsListPressed);
        public ICommand StopCommand => new Command(OnStopRecordingPressed);

        public RecordingViewModel(
            INavigationService navigationService,
            IPermissionsService permissionsService) : base(navigationService)
        {
            _navigationService = navigationService;
            _permissionsService = permissionsService;
            _recordingService = DependencyService.Get<IRecordingService>();
            _stopwatch = new Stopwatch();
            RecordingStatus = RecordingStatus.Stopped;
            SetRecordingTimer();
        }

        private async void OnRecordPressed()
        {
            switch (_recordingService.Status)
            {
                case RecordingStatus.Stopped:
                    var popupModel = new PopupModel(AppResources.SaveRecordingLabel, AppResources.EnterNameLabel, AppResources.SaveLabel);
                    var fileName = await _navigationService.DisplayPrompt(popupModel);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var ifPermissionGranted = await _permissionsService.CheckOrRequestMicrophonePermission();
                        if (ifPermissionGranted)
                        {
                            _recordingService.Start(GetFilePath(fileName));
                            _stopwatch.Start();
                        }
                    }
                    break;
                case RecordingStatus.Running:
                    _recordingService.Pause();
                    _stopwatch.Stop();
                    break;
                case RecordingStatus.Paused:
                    _recordingService.Resume();
                    _stopwatch.Start();
                    break;
            }

            RecordingStatus = _recordingService.Status;
        }

        private void OnStopRecordingPressed()
        {
            if (_recordingService.Status != RecordingStatus.Stopped)
            {
                _recordingService.Stop();
                _stopwatch.Reset();
            }
            RecordingStatus = _recordingService.Status;
        }

        private async void RecordingsListPressed(object obj)
        {
            await _navigationService.NavigateTo<RecordingsListViewModel>();
        }

        private void SetRecordingTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                TimeText = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                return true;
            });
        }

        private string GetFilePath(string fileName)
        {
            var trimmedName = fileName.Replace(" ", "-");
            var filePathDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "recordings");
            if (!File.Exists(filePathDir))
            {
                Directory.CreateDirectory(filePathDir);
            }

            return Path.Combine(filePathDir, trimmedName + ".3gpp");
        }

        private void SetRecordButtonText(RecordingStatus status)
        {
            switch (status)
            {
                case RecordingStatus.Stopped:
                    RecordingText = AppResources.RecordingStartLabel;
                    break;
                case RecordingStatus.Running:
                    RecordingText = AppResources.RecordingPauseLabel;
                    break;
                case RecordingStatus.Paused:
                    RecordingText = AppResources.RecordingResumeLabel;
                    break;
            }
        }
    }
}
