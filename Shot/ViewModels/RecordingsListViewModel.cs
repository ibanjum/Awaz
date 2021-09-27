using System;
using System.Collections.ObjectModel;
using Shot.Models;
using Shot.Navigation;
using Shot.Services;
using System.IO;
using Xamarin.Forms;
using Shot.Extensions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Essentials;
using Shot.Enumerations;

namespace Shot.ViewModels
{
    public class RecordingsListViewModel : BaseViewModel, IMessageSender
    {
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;

        public string RecordingsListTitle => AppResources.RecordingsListTitle;
        public string SearchFieldPlaceHolderText => AppResources.SearchFieldPlaceHolderLabel;
        public string RecordImageSource => ImageNames.RecordImage;
        public string DeleteImageSource => ImageNames.DeleteImage;
        public string ShareImageSource => ImageNames.ShareImage;

        public string SelectButtonText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public bool IsMediaPlayerOn
        {
            get { return GetPropertyValue<bool>(); }
            set { SetPropertyValue(value); }
        }

        public RecordingCellModel CurrentRecording
        {
            get { return GetPropertyValue<RecordingCellModel>(); }
            set { SetPropertyValue(value); }
        }

        public string PlayPauseImageSource
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public MediaStatus PlayingStatus
        {
            get { return GetPropertyValue<MediaStatus>(); }
            set
            {
                SetPropertyValue(value);
                PlayPauseImageSource = PlayingStatus == MediaStatus.Running ? ImageNames.PlayerPauseImage : ImageNames.PlayerPlayImage;
                IsMediaPlayerOn = PlayingStatus != MediaStatus.Stopped ? true : false;
            }
        }

        public int SliderMaximum
        {
            get { return GetPropertyValue<int>(); }
            set { SetPropertyValue(value); }
        }

        public int CurrentSliderPosition
        {
            get { return GetPropertyValue<int>(); }
            set { SetPropertyValue(value); }
        }

        public string SeletedRecordingCountText
        {
            get { return GetPropertyValue<string>(); }
            set { SetPropertyValue(value); }
        }

        public ObservableCollection<RecordingCellModel> Recordings
        {
            get { return GetPropertyValue<ObservableCollection<RecordingCellModel>>(); }
            set { SetPropertyValue(value); }
        }

        public bool IsSectionModeOn
        {
            get { return GetPropertyValue<bool>(); }
            set
            {
                SetPropertyValue(value);
                SelectButtonText = IsSectionModeOn ? AppResources.CancelLabel : AppResources.SelectLabel;
            }
        }

        public bool IsOptionsEnabled
        {
            get { return GetPropertyValue<bool>(); }
            set
            {
                SetPropertyValue(value);
            }
        }

        public ICommand SelectButtonCommand => new Command(OnSelectPressed);
        public ICommand ShareSelectedRecordingsCommand => new Command(OnDeleteSelectedRecordinsPressed);
        public ICommand DeleteSelectedRecordingsCommand => new Command(OnShareSelectedRecordinsPressed);
        public ICommand RecordCommand => new Command(OnRecordPressed);
        public ICommand PlayCommand => new Command(OnPlayPressed);
        public ICommand PlayerBarCommmand => new Command(OnPlayerBarPressed);

        public RecordingsListViewModel(
            INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _playerService = DependencyService.Get<IPlayerService>();
            Recordings = new ObservableCollection<RecordingCellModel>();
            SubscribeRecordingStatus();
            SliderMaximum = 1;
            UpdateTimerData();
        }

        public override Task Init()
        {
            PopulateRecordings();
            IsSectionModeOn = false;
            SeletedRecordingCountText = AppResources.SelectItemsLabel;
            return Task.CompletedTask;
        }

        private void PopulateRecordings()
        {
            Recordings.Clear();

            var pathString = FileExtension.CreateRecordingDirectory();
            var filePaths = Directory.GetFiles(pathString);
            foreach (var filePath in filePaths)
            {
                Recordings.Add(new RecordingCellModel()
                {
                    Name = FileExtension.GetEnteredName(filePath),
                    FilePath = filePath,
                    CreationTime = FileExtension.GetCreationTime(filePath),
                    Duration = _playerService.GetMetaDataDuration(filePath),
                    FileSize = FileExtension.GetFileSize(filePath),
                    FileFormat = FileExtension.GetFileExtension(filePath),
                    LongPressCommand = new Command(OnLongPressed),
                    ClickPressCommand = new Command(OnClickPressed)
                });
            }
            var list = Recordings.ToList();
            list.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));
            Recordings = new ObservableCollection<RecordingCellModel>(list);
        }

        private void OnSelectPressed()
        {
            if (IsSectionModeOn)
            {
                foreach (var recording in Recordings)
                {
                    recording.IsSelected = false;
                }
            }
            IsSectionModeOn = !IsSectionModeOn;
        }

        private async void OnLongPressed(object obj)
        {
            var filePath = obj as string;
            var cell = Recordings.FirstOrDefault(cm => cm.FilePath == filePath);
            cell.IsSelected = true;
            var selectedButton = await _navigationService.DisplayActionSheet(ActionSheetExtension.GetSelectedRecordingActionSheetModel(filePath));

            if (selectedButton == AppResources.DeleteLabel)
            {
                DeleteRecording(filePath);
            }
            else if (selectedButton == AppResources.ShareLabel)
            {
                await ActionSheetExtension.ShareSingleFile(filePath);
            }
            cell.IsSelected = false;
        }

        private void DeleteRecording(string filePath)
        {
            var list = Recordings.ToList();
            var cell = list.FirstOrDefault(cm => cm.FilePath == filePath);
            list.Remove(cell);
            Recordings = new ObservableCollection<RecordingCellModel>(list);
            FileExtension.DeleteFile(filePath);
        }

        private async void OnDeleteSelectedRecordinsPressed()
        {
            if (IsOptionsEnabled)
            {
                var count = Recordings.Where(cm => cm.IsSelected == true).Count();
                var selectedButton = await _navigationService.DisplayActionSheet(ActionSheetExtension.GetDeleteConfirmationActionSheet(count));
                if (!string.IsNullOrEmpty(selectedButton) && selectedButton != AppResources.CancelLabel)
                {
                    var list = Recordings.ToList();
                    foreach (var recording in Recordings)
                    {
                        if (recording.IsSelected)
                        {
                            list.Remove(recording);
                            FileExtension.DeleteFile(recording.FilePath);
                        }
                    }
                    Recordings = new ObservableCollection<RecordingCellModel>(list);
                    IsSectionModeOn = false;
                    IsOptionsEnabled = false;
                }
            }
        }

        private async void OnRecordPressed()
        {
            await _navigationService.NavigateTo<RecordingViewModel>();
        }

        private async void OnShareSelectedRecordinsPressed()
        {
            if (IsOptionsEnabled)
            {
                var recordings = Recordings.Where(cm => cm.IsSelected == true);
                var files = new List<ShareFile>();
                foreach (var recording in recordings)
                {
                    files.Add(new ShareFile(recording.FilePath));
                }
                await Share.RequestAsync(new ShareMultipleFilesRequest
                {
                    Title = SeletedRecordingCountText,
                    Files = files
                });
            }
        }

        private void OnClickPressed(object obj)
        {
            if (obj == null)
                return;

            var filePath = obj as string;
            var cell = Recordings.FirstOrDefault(cm => cm.FilePath == filePath);
            CurrentRecording = cell;

            if (IsSectionModeOn)
            {
                CurrentRecording.IsSelected = !CurrentRecording.IsSelected;
                var selectedCount = Recordings.Where(cm => cm.IsSelected == true).Count();
                IsOptionsEnabled = selectedCount > 0;
                string selectionText;
                switch (selectedCount)
                {
                    case 0:
                        selectionText = AppResources.SelectItemsLabel;
                        break;
                    case 1:
                        selectionText = AppResources.OneRecordingCountLabel;
                        break;
                    default:
                        selectionText = string.Format(AppResources.SeletedRecordingCountLabel, selectedCount);
                        break;
                }
                SeletedRecordingCountText = selectionText;
            }
            else
            {
                _playerService.SetPlayer(CurrentRecording.FilePath);
                SliderMaximum = CurrentRecording.Duration;
            }
        }

        private void OnPlayPressed()
        {
            switch (PlayingStatus)
            {
                case MediaStatus.Paused:
                    _playerService.Play();
                    break;
                case MediaStatus.Running:
                    _playerService.Pause();
                    break;
            }

        }

        private void UpdateTimerData()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () =>
            {
                if (PlayingStatus == MediaStatus.Running)
                {
                    CurrentSliderPosition = _playerService.GetCurrentPosition();
                    if (_playerService.GetMetaDataDuration(CurrentRecording?.FilePath).Equals(_playerService.GetCurrentPosition()))
                        OnForwardPressed();
                }

                return true;
            });
        }

        private void SubscribeRecordingStatus()
        {
            MessagingCenter.Subscribe<IMessageSender, string>(this, "PS", (s, e) =>
            {
                PlayingStatus = (MediaStatus)Enum.Parse(typeof(MediaStatus), e);
            });
        }

        private void OnForwardPressed()
        {
            _playerService.Pause();
            _playerService.Stop();
            var currentIndex = Recordings.IndexOf(CurrentRecording);
            RecordingCellModel nxtRecording;

            if (currentIndex + 1 < Recordings.Count)
                nxtRecording = Recordings[currentIndex + 1];
            else
                nxtRecording = Recordings[0];

            _playerService.SetPlayer(nxtRecording.FilePath);
            CurrentRecording = nxtRecording;
            SliderMaximum = CurrentRecording.Duration;
        }

        private async void OnPlayerBarPressed()
        {
            await _navigationService.NavigateTo<PlayerViewModel, PlayerModel>(
                new PlayerModel()
                {
                    Recordings = Recordings,
                    CurrentRecording = CurrentRecording,
                    PlayingStatus = PlayingStatus
                },
                isModel: true);
        }
    }
}
