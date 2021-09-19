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

namespace Shot.ViewModels
{
    public class RecordingsListViewModel : ConsistentPlayerViewModel
    {
        private readonly INavigationService _navigationService;

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

        public string SeletedRecordingCountText
        {
            get { return GetPropertyValue<string>(); }
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

        public RecordingsListViewModel(
            INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            Recordings = new ObservableCollection<RecordingCellModel>();
        }

        public override Task Init()
        {
            PlayerService = DependencyService.Get<IPlayerService>();
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
                    Duration = PlayerService.GetMetaDataDuration(filePath),
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

            if (IsSectionModeOn)
            {
                cell.IsSelected = !cell.IsSelected;
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
                CurrentRecording = cell;
                PlayCommand.Execute(null);
                //await _navigationService.NavigateTo<PlayerViewModel, RecordingCellModel>(cell);

            }
        }
    }
}
