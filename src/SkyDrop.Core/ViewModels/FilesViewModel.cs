using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IApiService apiService;
        private readonly IEncryptionService encryptionService;
        private readonly IFileSystemService fileSystemService;
        private readonly ILog log;
        private readonly IMvxNavigationService navigationService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;

        private TaskCompletionSource<IFolderItem> moveFilesCompletionSource;

        public FilesViewModel(ISingletonService singletonService,
            IApiService apiService,
            IStorageService storageService,
            IFileSystemService fileSystemService,
            IUserDialogs userDialogs,
            IMvxNavigationService navigationService,
            IEncryptionService encryptionService,
            ILog log) : base(singletonService)
        {
            Title = "SkyDrive";

            this.apiService = apiService;
            this.storageService = storageService;
            this.fileSystemService = fileSystemService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.encryptionService = encryptionService;
            this.log = log;

            ToggleLayoutCommand = new MvxCommand(() =>
                LayoutType = LayoutType == FileLayoutType.List ? FileLayoutType.Grid : FileLayoutType.List);
            BackCommand = new MvxAsyncCommand(GoBack);
            AddFolderCommand = new MvxAsyncCommand(AddFolder);
            MoveFileCommand = new MvxAsyncCommand(MoveFiles);
            DeleteFileCommand = new MvxAsyncCommand(DeleteFiles);
            SelectAllCommand = new MvxCommand(SelectAllFiles);
            SaveSelectedUnzippedFilesCommand = new MvxAsyncCommand(SaveSelectedUnzippedFiles);
            SaveArchiveCommand = new MvxAsyncCommand(SaveArchive);
            ExtractArchiveCommand = new MvxAsyncCommand(ExtractArchive);
        }

        public MvxObservableCollection<SkyFileDvm> SkyFiles { get; } = new MvxObservableCollection<SkyFileDvm>();
        public MvxObservableCollection<IFolderItem> Folders { get; } = new MvxObservableCollection<IFolderItem>();
        public FileLayoutType LayoutType { get; set; } = FileLayoutType.Grid;
        public IMvxCommand ToggleLayoutCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand AddFolderCommand { get; set; }
        public IMvxCommand MoveFileCommand { get; set; }
        public IMvxCommand DeleteFileCommand { get; set; }
        public IMvxCommand SelectAllCommand { get; set; }
        public IMvxCommand SaveSelectedUnzippedFilesCommand { get; set; }
        public IMvxCommand SaveArchiveCommand { get; set; }
        public IMvxCommand ExtractArchiveCommand { get; set; }
        public IMvxCommand CloseKeyboardCommand { get; set; }
        public IMvxCommand UpdateTopBarButtonsCommand { get; set; }
        public bool IsUnzippedFilesMode { get; set; }
        public string ArchiveUrl { get; set; }
        public bool IsError { get; set; }
        public bool IsUnzipError { get; set; }
        public bool IsLoading { get; set; }
        public bool IsLoadingLabelVisible => IsLoading || IsError;
        public string LoadingLabelText { get; set; }
        public string ErrorDetailText { get; set; }
        public bool IsFoldersVisible { get; set; } = true;
        public bool IsSelectionActive => GetIsSelectionActive();
        public bool IsMovingFile { get; set; }
        public bool IsSavingArchive { get; set; }
        public bool IsExtractingArchive { get; set; }
        public List<SkyFileDvm> FilesToMove { get; set; }
        public IFolderItem CurrentFolder { get; set; }

        //for top bar buttons
        public bool IsDeleteButtonVisible => IsSelectionActive && !IsUnzippedFilesMode;
        public bool IsMoveButtonVisible => IsSelectionActive && !IsFoldersVisible && !IsUnzippedFilesMode;
        public bool IsLayoutButtonVisible => !IsSelectionActive && !IsFoldersVisible && !IsLoading && !IsError;
        public bool IsSelectAllButtonVisible => IsUnzippedFilesMode && !IsLoading && !IsError;
        public bool IsAddFolderButtonVisible => !IsSelectionActive && IsFoldersVisible;
        public bool IsSaveButtonVisible => IsSelectionActive && IsUnzippedFilesMode;

        public override async Task Initialize()
        {
            await base.Initialize();

            try
            {
                if (IsUnzippedFilesMode)
                {
                    IsFoldersVisible = false;
                    var unzippedFiles = await DownloadAndUnzipArchive();
                    SkyFiles.SwitchTo(unzippedFiles);
                }
                else
                {
                    LoadFolders();
                }
            }
            catch (Exception e)
            {
                if (!IsUnzippedFilesMode)
                    userDialogs.Toast(e.Message);
            }

            UpdateTopBarButtonsCommand?.Execute();
        }

        private void LoadFolders()
        {
            var folders = storageService.LoadFolders();
            var folderItems = folders.Select(GetFolderDvm).ToList();
            folderItems.Insert(0, GetSentFolderItem());
            folderItems.Insert(1, GetReceivedFolderItem());
            Folders.SwitchTo(folderItems);
        }

        private async Task<List<SkyFile>> DownloadAndUnzipArchive()
        {
            const string downloadingText = "Downloading...";
            const string decryptingText = "Decrypting...";
            const string unzippingText = "Unzipping...";

            try
            {
                IsLoading = true;
                LoadingLabelText = downloadingText;

                List<SkyFile> files;
                var (stream, filename) = await apiService.DownloadFile(ArchiveUrl);
                using (stream)
                {
                    Stream decryptedStream;
                    if (filename.IsEncryptedZipFile())
                    {
                        //decrypt and unzip
                        LoadingLabelText = decryptingText;
                        await Task.Delay(500);
                        var decryptedPath = await encryptionService.DecodeZipFile(stream, filename);
                        decryptedStream = File.OpenRead(decryptedPath);

                        LoadingLabelText = unzippingText;
                        await Task.Delay(500);
                        return fileSystemService.UnzipArchive(decryptedStream));
                    }

                    if (filename.IsEncryptedFile())
                    {
                        //no need to unzip
                        var filePath = await fileSystemService.SaveFile(stream, filename, false);
                        var decryptedPath = await encryptionService.DecodeFile(filePath, false);
                        return new List<SkyFile> { new SkyFile { Filename = Path.GetFileName(decryptedPath), FullFilePath = decryptedPath } };
                    }

                    if (filename.GetFileCategory() == Util.FileCategory.Zip)
                    {
                        //no need to decrypt
                        decryptedStream = stream;

                        LoadingLabelText = unzippingText;
                        await Task.Delay(500);
                        return fileSystemService.UnzipArchive(decryptedStream);
                    }

                    //just a single plaintext file
                    var savedPath = await fileSystemService.SaveFile(stream, filename, false);
                    return new List<SkyFile> { new SkyFile { Filename = Path.GetFileName(savedPath), FullFilePath = filename } };
                }
            }
            catch (Exception e)
            {
                IsError = true;
                var actionName = LoadingLabelText == downloadingText ? "download" :
                    LoadingLabelText == decryptingText ? "decrypt" : "unzip";
                if (actionName == "unzip")
                    IsUnzipError = true;

                LoadingLabelText = $"Failed to {actionName} file";
                ErrorDetailText = e.Message;
                log.Exception(e);
                return null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveSelectedUnzippedFiles()
        {
            var selectedFilesDvMs = SkyFiles.Where(s => s.IsSelected).ToList();

            try
            {
                var selectedFilenames = selectedFilesDvMs.Select(s => s.SkyFile.Filename).ToList();
                var saveType = await Util.GetSaveTypeForMultiple(selectedFilenames);
                if (saveType == Util.SaveType.Cancel)
                    return;

                //show spinners
                foreach (var dvm in selectedFilesDvMs) dvm.IsLoading = true;

                await Task.Delay(1000); //wait for animation

                //save files
                foreach (var dvm in selectedFilesDvMs)
                {
                    //if user selected "Photos" (or is using Android) and the file is an image, save it to gallery
                    var fileSaveType = dvm.SkyFile.Filename.CanDisplayPreview() && saveType == Util.SaveType.Photos
                        ? Util.SaveType.Photos
                        : Util.SaveType.Files;
                    await fileSystemService.SaveToGalleryOrFiles(dvm.SkyFile.GetStream(), dvm.SkyFile.Filename,
                        fileSaveType);
                }

                //reset selection
                foreach (var file in SkyFiles)
                {
                    file.IsSelectionActive = false;
                    file.IsSelected = false;
                }

                UpdateTopBarButtonsCommand?.Execute();

                var s = selectedFilesDvMs.Count == 1 ? "" : "s";
                userDialogs.Toast($"Saved {selectedFilesDvMs.Count} file{s}");
            }
            catch (Exception e)
            {
                userDialogs.Toast("Failed to save files");
            }
            finally
            {
                //hide spinners
                foreach (var dvm in selectedFilesDvMs) dvm.IsLoading = false;
            }
        }

        private List<SkyFileDvm> GetUnzippedFileDvMs(IEnumerable<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDvm>();
            foreach (var skyFile in skyFiles)
                dvms.Add(GetUnzippedFileDvm(skyFile));

            return dvms;
        }

        private SkyFileDvm GetUnzippedFileDvm(SkyFile skyFile)
        {
            var dvm = new SkyFileDvm { SkyFile = skyFile };
            dvm.TapCommand = new MvxCommand(() => UnzippedFileTapped(dvm));
            dvm.LongPressCommand = new MvxCommand(() =>
                FileExplorerViewUtil.ActivateSelectionMode(SkyFiles, dvm, UpdateTopBarButtonsCommand));
            return dvm;
        }

        private void UnzippedFileTapped(SkyFileDvm selectedFile)
        {
            if (selectedFile.IsSelectionActive)
            {
                FileExplorerViewUtil.ToggleItemSelected(selectedFile, SkyFiles, UpdateTopBarButtonsCommand);
                return;
            }

            //activate selection
            selectedFile.LongPressCommand.Execute();
        }

        private List<SkyFileDvm> GetSkyFileDvMs(List<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDvm>();
            foreach (var skyFile in skyFiles)
                dvms.Add(GetSkyFileDvm(skyFile));

            return dvms;
        }

        private SkyFileDvm GetSkyFileDvm(SkyFile skyFile)
        {
            var dvm = new SkyFileDvm
            {
                SkyFile = skyFile,
                TapCommand = new MvxAsyncCommand(() => FileTapped(skyFile))
            };

            dvm.LongPressCommand = new MvxCommand(() =>
                FileExplorerViewUtil.ActivateSelectionMode(SkyFiles, dvm, UpdateTopBarButtonsCommand));
            return dvm;
        }

        private IFolderItem GetFolderDvm(Folder folder)
        {
            var dvm = new FolderDvm { Folder = folder };
            dvm.TapCommand = new MvxCommand(() => FolderTapped(dvm));
            dvm.LongPressCommand = new MvxCommand(() =>
                FileExplorerViewUtil.ActivateSelectionMode(ConvertFolderCollectionType(Folders), dvm,
                    UpdateTopBarButtonsCommand));
            return dvm;
        }

        private MvxObservableCollection<FolderDvm> ConvertFolderCollectionType(
            MvxObservableCollection<IFolderItem> folders)
        {
            return new MvxObservableCollection<FolderDvm>(folders.Where(a => a is FolderDvm).Select(a => a as FolderDvm)
                .ToList());
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

        private void FolderTapped(IFolderItem item)
        {
            if (item is FolderDvm folder && folder.IsSelectionActive)
            {
                FileExplorerViewUtil.ToggleItemSelected(folder, ConvertFolderCollectionType(Folders),
                    UpdateTopBarButtonsCommand);
                return;
            }

            SelectFolder(item);
        }

        private void SelectFolder(IFolderItem folder)
        {
            if (IsMovingFile)
            {
                moveFilesCompletionSource.TrySetResult(folder);
                return;
            }

            if (folder is FolderDvm folderDvm)
            {
                //show files for this folder
                var files = storageService.LoadSkyFilesWithSkylinks(folderDvm.Folder.SkyLinks);
                SkyFiles.SwitchTo(GetSkyFileDvMs(files));
            }
            else if (folder is SentFolder)
            {
                //show sent files
                var sentFiles = storageService.LoadSentSkyFiles();
                SkyFiles.SwitchTo(GetSkyFileDvMs(sentFiles));
            }
            else if (folder is ReceivedFolder)
            {
                //show received files
                var receivedFiles = storageService.LoadReceivedSkyFiles();
                SkyFiles.SwitchTo(GetSkyFileDvMs(receivedFiles));
            }

            Title = folder.Name;
            IsFoldersVisible = false;
            CurrentFolder = folder;
            UpdateTopBarButtonsCommand?.Execute();
        }

        private async Task FileTapped(SkyFile selectedFile)
        {
            var selectedFileDvm = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);
            if (selectedFileDvm.IsSelectionActive)
            {
                FileExplorerViewUtil.ToggleItemSelected(selectedFileDvm, SkyFiles, UpdateTopBarButtonsCommand);
                return;
            }

            //show the file
            await navigationService.Close(this, selectedFile);
        }

        //currently only used for unzipped files
        private void SelectAllFiles()
        {
            var shouldSelectAll =
                SkyFiles.Any(s => !s.IsSelected); //if one is not selected, select all, otherwise deselect all

            foreach (var file in SkyFiles)
            {
                file.IsSelectionActive = shouldSelectAll;
                file.IsSelected = shouldSelectAll;
            }

            UpdateTopBarButtonsCommand?.Execute();
        }

        private async Task GoBack()
        {
            if (IsMovingFile)
            {
                IsMovingFile = false;
                Title = "SkyDrive";
                moveFilesCompletionSource.SetResult(null);
                return;
            }

            if (IsSelectionActive)
            {
                ExitSelection();
                return;
            }

            if (!IsFoldersVisible && !IsUnzippedFilesMode)
            {
                //go back to folders
                SkyFiles.Clear();
                IsFoldersVisible = true;
                Title = "SkyDrive";
                UpdateTopBarButtonsCommand?.Execute();
                return;
            }

            await navigationService.Close(this);
        }

        private void ExitSelection()
        {
            if (IsFoldersVisible)
            {
                //exit folder selection
                foreach (var folderItem in Folders)
                    if (folderItem is FolderDvm folder)
                    {
                        folder.IsSelectionActive = false;
                        folder.IsSelected = false;
                    }
            }
            else
            {
                //exit file selection
                foreach (var file in SkyFiles)
                {
                    file.IsSelectionActive = false;
                    file.IsSelected = false;
                }
            }

            UpdateTopBarButtonsCommand?.Execute();
        }

        private async Task AddFolder()
        {
            var result = await userDialogs.PromptAsync("Folder name");
            CloseKeyboardCommand?.Execute();

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

        private async Task MoveFiles()
        {
            //move all selected files to new folder
            var selectedFiles = SkyFiles.Where(s => s.IsSelected).ToList();
            var s = selectedFiles.Count == 1 ? "" : "s";

            //exit selection so that nav bar buttons get reset
            ExitSelection();

            //ask user to select a folder
            FilesToMove = selectedFiles;
            IsMovingFile = true;
            IsFoldersVisible = true;
            Title = $"Moving {selectedFiles.Count} file{s}";
            UpdateTopBarButtonsCommand?.Execute();

            moveFilesCompletionSource = new TaskCompletionSource<IFolderItem>();
            var folder = await moveFilesCompletionSource.Task;
            IsMovingFile = false;

            if (folder == null)
                //user tapped back button to exit this action
                return;

            if (folder is SentFolder || folder is ReceivedFolder)
            {
                userDialogs.Toast($"Cannot move file{s} to {folder.Name}");
                Title = "SkyDrive";
                return;
            }

            var folderDvm = folder as FolderDvm;
            storageService.MoveSkyFiles(selectedFiles.Select(s => s.SkyFile).ToList(), folderDvm.Folder);

            userDialogs.Toast($"Moved {selectedFiles.Count} file{s} to {folder.Name}");

            SelectFolder(folder);
        }

        private async Task DeleteFiles()
        {
            if (IsFoldersVisible)
            {
                //delete all selected folders

                var selectedFolders = ConvertFolderCollectionType(Folders).Where(s => s.IsSelected).ToList();
                var s = selectedFolders.Count == 1 ? "" : "s";
                if (!await userDialogs.ConfirmAsync(
                        $"Are you sure you want to delete {selectedFolders.Count} folder{s}?"))
                    return;

                foreach (var folder in selectedFolders)
                {
                    storageService.DeleteFolder(folder.Folder);
                    Folders.Remove(folder);
                }
            }
            else
            {
                //delete all selected files

                var selectedFiles = SkyFiles.Where(s => s.IsSelected).ToList();
                var s = selectedFiles.Count == 1 ? "" : "s";

                var isSentOrReceivedFolder = CurrentFolder is SentFolder || CurrentFolder is ReceivedFolder;
                var folder = isSentOrReceivedFolder ? null : (CurrentFolder as FolderDvm).Folder;
                var folderName = isSentOrReceivedFolder ? "all folders" : folder.Name;

                if (!await userDialogs.ConfirmAsync(
                        $"Are you sure you want to delete {selectedFiles.Count} file{s} from {folderName}?"))
                    return;

                foreach (var file in selectedFiles)
                {
                    storageService.DeleteSkyFile(file.SkyFile, folder);
                    SkyFiles.Remove(file);
                }
            }

            ExitSelection();
        }

        private async Task SaveArchive()
        {
            try
            {
                IsSavingArchive = true;
                await apiService.DownloadAndSaveSkyfile(ArchiveUrl, Util.SaveType.Files);
            }
            catch (Exception e)
            {
                log.Exception(e);
                userDialogs.Toast(e.Message, TimeSpan.FromSeconds(4));
            }
            finally
            {
                IsSavingArchive = false;
            }
        }

        private async Task ExtractArchive()
        {
            try
            {
                IsExtractingArchive = true;
                var (stream, filename) = await apiService.DownloadFile(ArchiveUrl);
                using (stream)
                {
                    fileSystemService.ExtractArchiveToDevice(stream, filename);
                }
            }
            catch (Exception e)
            {
                log.Exception(e);
                userDialogs.Toast(e.Message);
            }
            finally
            {
                IsExtractingArchive = false;
            }
        }

        private bool GetIsSelectionActive()
        {
            return IsFoldersVisible
                ? Folders.Where(f => f is FolderDvm)?.Select(f => f as FolderDvm).FirstOrDefault()?.IsSelectionActive ??
                  false
                : SkyFiles.FirstOrDefault()?.IsSelectionActive ?? false;
        }

        public override void Prepare(NavParam parameter)
        {
            IsUnzippedFilesMode = parameter.IsUnzippedFilesMode;
            ArchiveUrl = parameter.ArchiveUrl;
            if (IsUnzippedFilesMode)
                Title = parameter.ArchiveName;
        }

        public class NavParam
        {
            public bool IsUnzippedFilesMode { get; set; }
            public string ArchiveUrl { get; set; }
            public string ArchiveName { get; set; }
        }
    }
}