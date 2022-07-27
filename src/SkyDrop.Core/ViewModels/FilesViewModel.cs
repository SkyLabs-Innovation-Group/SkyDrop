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
            public string ArchiveName { get; set; }
        }

        public MvxObservableCollection<SkyFileDVM> SkyFiles { get; } = new MvxObservableCollection<SkyFileDVM>();
        public MvxObservableCollection<IFolderItem> Folders { get; } = new MvxObservableCollection<IFolderItem>();
        public FileLayoutType LayoutType { get; set; } = FileLayoutType.Grid;
        public IMvxCommand ToggleLayoutCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand AddFolderCommand { get; set; }
        public bool IsUnzippedFilesMode { get; set; }
        public string ArchiveUrl { get; set; }
        public bool IsError { get; set; }
        public bool IsLoading { get; set; }
        public bool IsLoadingLabelVisible => IsLoading || IsError;
        public string LoadingLabelText { get; set; }
        public bool IsFoldersVisible { get; set; } = true;

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
            Title = "SkyDrive";

            this.apiService = apiService;
            this.storageService = storageService;
            this.fileSystemService = fileSystemService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.log = log;

            ToggleLayoutCommand = new MvxCommand(() => LayoutType = LayoutType == FileLayoutType.List ? FileLayoutType.Grid : FileLayoutType.List);
            BackCommand = new MvxAsyncCommand(GoBack);
            AddFolderCommand = new MvxAsyncCommand(AddFolder);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            try
            {
                LoadFolders();
            }
            catch(Exception e)
            {
                userDialogs.Toast(e.Message);
            }
        }

        private void LoadFolders()
        {
            var folders = storageService.LoadFolders();
            var folderItems = folders.Select(GetFolderDVM).ToList();
            folderItems.Insert(0, GetSentFolderItem());
            folderItems.Insert(1, GetReceivedFolderItem());
            Folders.SwitchTo(folderItems);
        }
        /*
        private async Task LoadSkyFiles()
        {
            if (IsUnzippedFilesMode)
                allSkyFiles = await DownloadAndUnzipArchive();
            else
                allSkyFiles = GetSkyFileDVMs(storageService.LoadAllSkyFiles());
        }
        */
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
                var newFileName = await fileSystemService.SaveFile(file.GetStream(), file.Filename, isPersistent: true);
                userDialogs.Toast($"Saved {newFileName}");
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

        private IFolderItem GetFolderDVM(Folder folder)
        {
            var dvm = new FolderDVM { Folder = folder };
            dvm.TapCommand = new MvxCommand(() => SelectFolder(dvm));
            return dvm;
        }

        private IFolderItem GetSentFolderItem()
        {
            var sentFolder = new SentFolder();
            sentFolder.TapCommand = new MvxCommand(() => SelectFolder(sentFolder));
            return sentFolder;
        }

        private IFolderItem GetReceivedFolderItem()
        {
            var receivedFolder = new ReceivedFolder();
            receivedFolder.TapCommand = new MvxCommand(() => SelectFolder(receivedFolder));
            return receivedFolder;
        }

        private void SelectFolder(IFolderItem folder)
        {
            if (folder is FolderDVM folderDVM)
            {
                //show files for this folder
                var files = storageService.LoadSkyFilesWithSkylinks(folderDVM.Folder.SkyLinks);
                SkyFiles.SwitchTo(GetSkyFileDVMs(files));
            }
            else if (folder is SentFolder)
            {
                //show sent files
                var sentFiles = storageService.LoadSentSkyFiles();
                SkyFiles.SwitchTo(GetSkyFileDVMs(sentFiles));
            }
            else if(folder is ReceivedFolder)
            {
                //show received files
                var receivedFiles = storageService.LoadReceivedSkyFiles();
                SkyFiles.SwitchTo(GetSkyFileDVMs(receivedFiles));
            }

            Title = folder.Name;
            IsFoldersVisible = false;
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

        private async Task GoBack()
        {
            if (!IsFoldersVisible)
            {
                //go back to folders
                SkyFiles.Clear();
                IsFoldersVisible = true;
                Title = "SkyDrive";
                return;
            }

            await navigationService.Close(this);
        }

        private async Task AddFolder()
        {
            var result = await userDialogs.PromptAsync("Folder name");
            if (!result.Ok)
                return;

            if (result.Value.IsNullOrEmpty())
                return;

            var folderName = result.Value.Trim();
            var newFolder = new Folder { Name = folderName, Id = Guid.NewGuid(), SkyLinks = new List<string>() };
            storageService.SaveFolder(newFolder);

            LoadFolders();

            userDialogs.Toast($"Added folder {folderName}");
        }

        public override void Prepare(NavParam parameter)
        {
            this.IsUnzippedFilesMode = parameter.IsUnzippedFilesMode;
            this.ArchiveUrl = parameter.ArchiveUrl;
            if (IsUnzippedFilesMode)
                Title = parameter.ArchiveName;
        }
    }
}
