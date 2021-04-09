using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.ViewModels.Main
{
    public class FilesViewModel : BaseViewModel
    {
        public MvxObservableCollection<SkyFileDVM> SkyFiles { get; } = new MvxObservableCollection<SkyFileDVM>();

        /// <summary>
        /// Updated with view binding.
        /// </summary>
        public int CurrentlySelectedFileIndex { get; set; }

        public SkyFileDVM CurrentlySelectedFileDvm { get; set; }

        public SkyFileDVM PreviousSelectedSkyFileDvm { get; set; }

        public bool IsLoading { get; set; }

        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly ILog _log;

        public IMvxAsyncCommand SelectFileCommand { get; set; }
        public IMvxCommand SelectImageCommand { get; set; }
        public IMvxCommand<SkyFile> OpenFileInBrowserCommand { get; set; }
        public IMvxCommand UploadCommand { get; set; }
        public IMvxCommand ClearDataCommand { get; set; }

        private Func<Task> _selectFileAsyncFunc;
        public Func<Task> SelectFileAsyncFunc
        {
            get => _selectFileAsyncFunc;
            set => _selectFileAsyncFunc = value;
        }

        private Func<Task> _selectImageAsyncFunc;
        public Func<Task> SelectImageAsyncFunc
        {
            get => _selectImageAsyncFunc;
            set => _selectImageAsyncFunc = value;
        }

        public IMvxCommand AfterFileSelected { get; set; }
        public IMvxCommand ScrollToFileCommand { get; set; }

        public FilesViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "File Storage";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            _log = log;
            SelectFileCommand = new MvxAsyncCommand(async () => await SelectFileAsyncFunc());
            SelectImageCommand = new MvxAsyncCommand(async () => await SelectImageAsyncFunc());
            UploadCommand = new MvxAsyncCommand(UploadStagedFiles);
            ClearDataCommand = new MvxCommand(ClearData);
        }

        public override Task Initialize()
        {
            LoadSkyFiles();

            return base.Initialize();
        }

        private void LoadSkyFiles()
        {
            var newSkyFiles = GetSkyFileDVMs(storageService.LoadSkyFiles());
            SkyFiles.SwitchTo(newSkyFiles);

            // TODO remove if possible
            RaiseAllPropertiesChanged();
        }

        private List<SkyFileDVM> GetSkyFileDVMs(List<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDVM>();
            foreach (var skyFile in skyFiles)
            {
                dvms.Add(GetSkyFileDVM(skyFile));
            }

            return dvms;
        }

        private SkyFileDVM GetSkyFileDVM(SkyFile skyFile)
        {
            return new SkyFileDVM
            {
                SkyFile = skyFile,
                TapCommand = new MvxCommand(() => ToggleSelectState(skyFile)),
                OpenCommand = new MvxCommand(() => OpenFileInBrowser(skyFile)),
                CopySkyLinkCommand = new MvxAsyncCommand(() => CopyFileLinkToClipboard(skyFile)),
                DeleteCommand = new MvxCommand(() => DeleteSkyFileFromList(skyFile)),
            };
        }

        public void StageFile(SkyFile stagedFile)
        {
            SkyFiles.Add(GetSkyFileDVM(stagedFile));
        }

        private void OpenFileInBrowser(SkyFile skyFile)
        {
            if (skyFile.Status == FileStatus.Staged)
            {
                PromptToUploadFile();
                return;
            }

            OpenFileInBrowserCommand.Execute(skyFile);
        }

        private void DeleteSkyFileFromList(SkyFile file)
        {
            if (file.Status == FileStatus.Staged)
            {
                file.Data = null;

                var newFiles = new List<SkyFileDVM>(SkyFiles.Where(f => f.SkyFile.Filename != file.Filename));
                SkyFiles.SwitchTo(newFiles);
                return;
            }

            var newSkyFiles = new List<SkyFileDVM>(SkyFiles.Where(f => f.SkyFile.Skylink != file.Skylink));
            SkyFiles.SwitchTo(newSkyFiles);

            storageService.DeleteSkyFile(file);
        }

        private async Task CopyFileLinkToClipboard(SkyFile skyFile)
        {
            if (skyFile.Status == FileStatus.Staged)
            {
                PromptToUploadFile();
                return;
            }

            string skyLink = Util.GetSkylinkUrl(skyFile.Skylink);
            await Xamarin.Essentials.Clipboard.SetTextAsync(skyLink);

            Log.Trace("Set clipboard text to " + skyLink);
            userDialogs.Toast("Copied SkyLink to clipboard");
        }

        private void PromptToUploadFile()
        {
            userDialogs.Toast("Please upload the file first");
        }

        private async Task UploadStagedFiles()
        {
            IsLoading = true;

            //get staged files
            var currentlyStagedFiles = SkyFiles.Where(s => s.SkyFile.Status == FileStatus.Staged).ToList();

            foreach (var stagedFile in currentlyStagedFiles)
            {
                stagedFile.IsLoading = true;
                var newSkyFiles = SkyFiles.ToList();
                SkyFiles.SwitchTo(newSkyFiles);

                await UploadFile(stagedFile.SkyFile);

                stagedFile.IsLoading = false;
            }

            IsLoading = false;
        }

        int uploadCount = 0;
        private async Task UploadFile(SkyFile stagedFile)
        {
            try
            {
                uploadCount++;

                Log.Trace($"Uploading file #{uploadCount}: {stagedFile?.Filename ?? "null"}");

                var skyFile = await apiService.UploadFile(stagedFile.Filename, stagedFile.Data, stagedFile.FileSizeBytes, null);
                Log.Trace("UPLOAD COMPLETE: " + skyFile.Skylink);

                var existingFile = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == skyFile.Skylink);
                if (existingFile != null)
                {
                    var message = "File was already uploaded";
                    Log.Trace(message);
                    userDialogs.Toast(message);

                    int indexOfExistingFile = SkyFiles.IndexOf(existingFile);
                    SkyFiles.Move(indexOfExistingFile, 0);

                    ScrollToFileCommand.Execute();

                    return;
                }

                //var newSkyFiles = SkyFiles.ToList();
                var fileDvm = SkyFiles.FirstOrDefault(f => f.SkyFile.Skylink == stagedFile.Skylink);
                fileDvm.SetUploaded(skyFile);
                //SkyFiles.SwitchTo(newSkyFiles);

                _ = fileDvm.RaisePropertyChanged(() => fileDvm.FillColor);
                _ = RaisePropertyChanged(() => SkyFiles);

                ScrollToFileCommand.Execute();

                storageService.SaveSkyFiles(skyFile);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                userDialogs.Toast("File upload failed");
            }
        }

        private void ToggleSelectState(SkyFile selectedFile)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);

            bool currentSelectionState = selectedFileDVM.IsSelected;
            bool newSelectionState = !currentSelectionState;

            if (newSelectionState == true)
            {
                int newSelectedIndex = SkyFiles.IndexOf(selectedFileDVM);

                CurrentlySelectedFileDvm = selectedFileDVM;
                CurrentlySelectedFileIndex = newSelectedIndex;

                foreach (var skyFile in SkyFiles)
                {
                    skyFile.IsSelected = false;
                }

                selectedFileDVM.IsSelected = true;

                AfterFileSelected.Execute();

                PreviousSelectedSkyFileDvm = selectedFileDVM;
            }
            else // newSelectionState == false
            {
                foreach (var skyFile in SkyFiles)
                {
                    skyFile.IsSelected = false;
                }

                AfterFileSelected.Execute();

                PreviousSelectedSkyFileDvm = null;
            }
        }

        public int? GetIndexForPreviouslySelectedFile()
        {
            try
            {
                return SkyFiles.IndexOf(PreviousSelectedSkyFileDvm);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }

        private void ClearData()
        {
            storageService.ClearAllData();

            SkyFiles.Clear();
        }
    }
}
