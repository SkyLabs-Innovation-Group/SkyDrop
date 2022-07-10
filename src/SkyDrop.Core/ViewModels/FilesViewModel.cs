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
using static SkyDrop.Core.ViewModels.Main.FilesViewModel;

namespace SkyDrop.Core.ViewModels.Main
{
    public enum FileLayoutType
    {
        List = 0,
        Grid = 1
    }

    public class FilesViewModel : BaseViewModel<NavParam, SkyFile>
    {
        public class NavParam
        {
            public bool IsUnzippedFilesMode { get; set; }
            public string ArchiveUrl { get; set; }
        }

        public MvxObservableCollection<SkyFileDVM> SkyFiles { get; } = new MvxObservableCollection<SkyFileDVM>();
        public FileLayoutType LayoutType { get; set; } = FileLayoutType.Grid;
        public IMvxCommand ToggleLayoutCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public bool IsUnzippedFilesMode { get; set; }
        public string ArchiveUrl { get; set; }
        public bool IsError { get; set; }
        public bool IsLoading { get; set; }
        public bool IsLoadingLabelVisible => IsLoading || IsError;
        public string LoadingLabelText { get; set; }

        private readonly IApiService apiService;
        private readonly IFileSystemService fileSystemService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly ILog log;

        public FilesViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IFileSystemService fileSystemService,
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "File Storage";

            this.apiService = apiService;
            this.storageService = storageService;
            this.fileSystemService = fileSystemService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.log = log;

            ToggleLayoutCommand = new MvxCommand(() => LayoutType = LayoutType == FileLayoutType.List ? FileLayoutType.Grid : FileLayoutType.List);
            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await LoadSkyFiles();
        }

        private async Task LoadSkyFiles()
        {
            List<SkyFileDVM> loadedSkyFiles;
            if (IsUnzippedFilesMode)
                loadedSkyFiles = await DownloadAndUnzipArchive();
            else
                loadedSkyFiles = GetSkyFileDVMs(storageService.LoadSkyFiles());

            SkyFiles.SwitchTo(loadedSkyFiles);
        }

        private async Task<List<SkyFileDVM>> DownloadAndUnzipArchive()
        {
            var didDownload = false;
            try
            {
                IsLoading = true;
                LoadingLabelText = "Downloading...";

                List<SkyFile> files;
                using (var stream = await apiService.DownloadFile(ArchiveUrl))
                {
                    didDownload = true;
                    LoadingLabelText = "Unzipping...";
                    await Task.Delay(500);
                    files = fileSystemService.UnzipArchive(stream);
                }
                
                return GetUnzippedFileDVMs(files);
            }
            catch(Exception e)
            {
                IsError = true;
                var actionName = didDownload ? "unzip" : "download";
                LoadingLabelText = $"Failed to {actionName} file";
                log.Exception(e);
                return null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveUnzippedFile(SkyFile file)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(f => f.SkyFile.FullFilePath == file.FullFilePath);
            selectedFileDVM.IsLoading = true;

            try
            {
                await Task.Delay(1000); //wait for animation
                await fileSystemService.SaveFile(file.GetStream(), file.Filename, isPersistent: true);
                userDialogs.Toast($"Saved {file.Filename}");
            }
            catch (Exception e)
            {
                userDialogs.Toast("Failed to save file");
            }
            finally
            {
                selectedFileDVM.IsLoading = false;
            }
        }

        private List<SkyFileDVM> GetUnzippedFileDVMs(IEnumerable<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDVM>();
            foreach (var skyFile in skyFiles)
                dvms.Add(GetUnzippedFileDVM(skyFile));

            return dvms;
        }

        private SkyFileDVM GetUnzippedFileDVM(SkyFile skyFile)
        {
            return new SkyFileDVM
            {
                SkyFile = skyFile,
                TapCommand = new MvxAsyncCommand(() => UnzippedFileTapped(skyFile)),
                LongPressCommand = new MvxCommand(() => FileExplorerViewUtil.ActivateSelectionMode(SkyFiles, skyFile))
            };
        }

        private async Task UnzippedFileTapped(SkyFile selectedFile)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(s => s.SkyFile.FullFilePath == selectedFile.FullFilePath);
            if (selectedFileDVM.IsSelectionActive)
            {
                FileExplorerViewUtil.ToggleFileSelected(selectedFile, SkyFiles);
                return;
            }

            //save the file
            await SaveUnzippedFile(selectedFile);
        }

        private List<SkyFileDVM> GetSkyFileDVMs(List<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDVM>();
            foreach (var skyFile in skyFiles)
                dvms.Add(GetSkyFileDVM(skyFile));

            return dvms;
        }

        private SkyFileDVM GetSkyFileDVM(SkyFile skyFile)
        {
            return new SkyFileDVM
            {
                SkyFile = skyFile,
                TapCommand = new MvxAsyncCommand(() => FileTapped(skyFile)),
                LongPressCommand = new MvxCommand(() => FileExplorerViewUtil.ActivateSelectionMode(SkyFiles, skyFile))
            };
        }

        private async Task FileTapped(SkyFile selectedFile)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);
            if (selectedFileDVM.IsSelectionActive)
            {
                FileExplorerViewUtil.ToggleFileSelected(selectedFile, SkyFiles);
                return;
            }

            //show the file
            await navigationService.Close(this, selectedFile);
        }

        public override void Prepare(NavParam parameter)
        {
            this.IsUnzippedFilesMode = parameter.IsUnzippedFilesMode;
            this.ArchiveUrl = parameter.ArchiveUrl;
        }
    }
}
