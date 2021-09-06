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
    public class RecordingsListViewModel : BaseViewModel
    {
        private readonly IPlayerService _playerService;
        private readonly INavigationService _navigationService;

        public string RecordingsListTitle => AppResources.RecordingsListTitle;
        public string SearchFieldPlaceHolderText => AppResources.SearchFieldPlaceHolderLabel;

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

        public RecordingsListViewModel(
            INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _playerService = DependencyService.Get<IPlayerService>();
            Recordings = new ObservableCollection<RecordingCellModel>();
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

            var pathString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "recordings");
            var filePaths = Directory.GetFiles(pathString);
            foreach (var filePath in filePaths)
            {
                Recordings.Add(new RecordingCellModel()
                {
                    Name = GetEnteredName(filePath),
                    FilePath = filePath,
                    CreationTime = MediaExtension.GetCreationTime(filePath),
                    Duration = _playerService.GetMetaDataDuration(filePath),
                    LongPressCommand = new Command(OnLongPressed),
                    ClickPressCommand = new Command(OnClickPressed)
                });
            }
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
            var selectedButton = await _navigationService.DisplayActionSheet(GetSelectedRecordingActionSheetModel(filePath));

            if (selectedButton == AppResources.DeleteLabel)
            {
                DeleteRecording(filePath);
            }
            else if (selectedButton == AppResources.ShareLabel)
            {
                await ShareSingleFile(filePath);
            }
            cell.IsSelected = false;
        }

        private void DeleteRecording(string filePath)
        {
            var list = Recordings.ToList();
            var cell = list.FirstOrDefault(cm => cm.FilePath == filePath);
            list.Remove(cell);
            Recordings = new ObservableCollection<RecordingCellModel>(list);
            File.Delete(filePath);
        }

        private string GetEnteredName(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var split = fileName.Split('.');
            var val = split[0].Replace("-", " ");
            char[] letters = val.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        private async void OnDeleteSelectedRecordinsPressed()
        {
            if (IsOptionsEnabled)
            {
                var count = Recordings.Where(cm => cm.IsSelected == true).Count();
                var selectedButton = await _navigationService.DisplayActionSheet(GetDeleteConfirmationActionSheet(count));
                if (!string.IsNullOrEmpty(selectedButton) && selectedButton != AppResources.CancelLabel)
                {
                    var list = Recordings.ToList();
                    list.RemoveAll(cm => cm.IsSelected == true);
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

        private async Task ShareSingleFile(string filePath)
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = string.Format(AppResources.SeletedRecordingCountLabel, 1),
                File = new ShareFile(filePath)
            });
        }

        private async void OnClickPressed(object obj)
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
                //await _navigationService.NavigateTo<PlayerViewModel, RecordingCellModel>(cell);
            }
        }

        private ActionSheetModel GetSelectedRecordingActionSheetModel(string filePath)
        {
            return new ActionSheetModel()
            {
                Title = GetEnteredName(filePath),
                Cancel = AppResources.CancelLabel,
                Distruction = AppResources.DeleteLabel,
                Buttons = new string[] { AppResources.ShareLabel }
            };
        }

        private ActionSheetModel GetDeleteConfirmationActionSheet(int count)
        {
            string title = string.Empty;
            string deleteText = string.Empty;
            switch (count)
            {
                case 0:
                    break;
                case 1:
                    title = AppResources.OneDeleteConfirmationLabel;
                    deleteText = AppResources.DeleteLabel;
                    break;
                default:
                    title = AppResources.DeleteConfirmationLabel;
                    deleteText = string.Format(AppResources.DeleteCountLabel, count);
                    break;
            }
            return new ActionSheetModel()
            {
                Title = title,
                Cancel = AppResources.CancelLabel,
                Distruction = deleteText
            };
        }
    }
}
