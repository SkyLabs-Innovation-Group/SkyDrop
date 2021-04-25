using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
using Xamarin.Essentials;
using ZXing.Common;

namespace SkyDrop.Core.ViewModels.Main
{
    public class DropViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly IFileSystemService fileSystemService;
        private readonly IBarcodeService barcodeService;
        private readonly IShareLinkService shareLinkService;
        private readonly IUploadTimerService uploadTimerService;

        public IMvxCommand SendCommand { get; set; }
        public IMvxCommand ReceiveCommand { get; set; }
        public IMvxCommand ShareCommand { get; set; }
        public IMvxCommand CopyLinkCommand { get; set; }
        public IMvxCommand HandleUploadErrorCommand { get; set; }
        public IMvxCommand ResetBarcodeCommand { get; set; }
        public IMvxCommand NavToSettingsCommand { get; set; }
        public IMvxCommand ShareLinkCommand { get; set; }
        public IMvxCommand OpenFileInBrowserCommand { get; set; }

        public IMvxCommand SlideSendButtonToCenterCommand { get; set; }

        //public IMvxCommand ResetAnimateCommand { get; set; }
        public IMvxCommand CancelUploadCommand { get; set; }
        public IMvxCommand CheckUserIsSwipingCommand { get; set; }
        public IMvxCommand<SkyFile> RenameStagedFileCommand { get; set; }

        public string SkyFileFullUrl { get; set; }
        public bool IsUploading { get; set; }
        public bool IsStagingFiles { get; set; }
        public bool IsUploadArrowVisible => !IsUploading && !IsStagingFiles;
        public bool IsBarcodeLoading { get; set; }
        public bool IsBarcodeVisible { get; set; }
        public bool IsStagedFilesVisible => DropViewUIState == DropViewState.ConfirmFilesState;

        public string SendButtonLabel => IsUploading ? StagedFiles?.Count > 1 ? "SENDING FILES" :
            "SENDING FILE" :
            StagedFiles?.Count > 1 ? "SEND FILES" : "SEND FILE";

        public bool IsSendButtonGreen { get; set; } = true;
        public bool IsReceiveButtonGreen { get; set; } = true;
        public string UploadTimerText { get; set; }
        public bool IsAnimatingBarcodeOut { get; set; }
        public string FileSize { get; set; }
        public double UploadProgress { get; set; } //0-1
        public bool FirstFileUploaded { get; set; }
        public bool UserIsSwipingResult { get; set; }

        public List<SkyFile> StagedFiles { get; set; }
        public SkyFile UploadedFile { get; set; }
        public SkyFile FileToUpload { get; set; }

        private DropViewState _dropViewUIState;

        public DropViewState DropViewUIState
        {
            get => _dropViewUIState;
            set
            {
                _dropViewUIState = value;
                Log.Trace($"New UI State: {value}");
            }
        }

        public enum DropViewState
        {
            SendReceiveButtonState = 1,
            ConfirmFilesState = 2,
            QRCodeState = 3
        }

        private string errorMessage;
        private CancellationTokenSource uploadCancellationToken;

        // private Func<Task> _selectFileAsyncFunc;
        // public Func<Task> SelectFileAsyncFunc
        // {
        //     get => _selectFileAsyncFunc;
        //     set => _selectFileAsyncFunc = value;
        // }

        // private Func<Task> _selectImageAsyncFunc;
        // public Func<Task> SelectImageAsyncFunc
        // {
        //     get => _selectImageAsyncFunc;
        //     set => _selectImageAsyncFunc = value;
        // }

        private Func<Task> _generateBarcodeAsyncFunc;

        public Func<Task> GenerateBarcodeAsyncFunc
        {
            get => _generateBarcodeAsyncFunc;
            set => _generateBarcodeAsyncFunc = value;
        }

        public DropViewModel(ISingletonService singletonService,
            IApiService apiService,
            IStorageService storageService,
            IBarcodeService barcodeService,
            IShareLinkService shareLinkService,
            IUploadTimerService uploadTimerService,
            IUserDialogs userDialogs,
            IMvxNavigationService navigationService,
            IFileSystemService fileSystemService,
            ILog log) : base(singletonService)
        {
            Title = "SkyDrop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.fileSystemService = fileSystemService;
            this.barcodeService = barcodeService;
            this.shareLinkService = shareLinkService;
            this.uploadTimerService = uploadTimerService;

            SendCommand = new MvxAsyncCommand(SendButtonTapped);
            ReceiveCommand = new MvxAsyncCommand(ReceiveFile);
            CopyLinkCommand = new MvxAsyncCommand(CopySkyLinkToClipboard);
            NavToSettingsCommand = new MvxAsyncCommand(NavToSettings);
            ShareLinkCommand = new MvxAsyncCommand(ShareLink);
            CancelUploadCommand = new MvxCommand(CancelUpload);
            RenameStagedFileCommand = new MvxAsyncCommand<SkyFile>(skyFile => RenameStagedFile(skyFile));
            OpenFileInBrowserCommand = new MvxAsyncCommand(async () => await OpenFileInBrowser());
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            DropViewUIState = DropViewState.SendReceiveButtonState;
        }

        public override void ViewAppeared()
        {
            Log.Trace($"{nameof(DropViewModel)} ViewAppeared()");

            base.ViewAppeared();

            //make sure buttons return to green when returning from QR scanner
            ResetUI(leaveBarcode: true);

            //show error message after the qr code scanner view has closed to avoid exception
            if (!string.IsNullOrEmpty(errorMessage))
            {
                userDialogs.Toast(errorMessage);
                errorMessage = null;
            }
        }

        public void ResetUI(bool leaveBarcode = false)
        {
            IsSendButtonGreen = true;
            IsReceiveButtonGreen = true;
            UploadTimerText = "";
            FileSize = "";
            IsAnimatingBarcodeOut = false;
            UploadProgress = 0;

            if (!leaveBarcode)
                IsBarcodeVisible = false;
        }

        private async Task SendButtonTapped()
        {
            if (DropViewUIState == DropViewState.SendReceiveButtonState)
            {
                await StartSendFile();
            }
            else if (DropViewUIState == DropViewState.ConfirmFilesState)
            {
                await FinishSendFile();
            }
        }

        private async Task StartSendFile()
        {
            //don't allow user to select file while a file is uploading
            if (IsUploading) return;

            if (UserIsSwiping()) return;

            IsSendButtonGreen = true;
            IsReceiveButtonGreen = false;

            //select file

            var file = "Select Files";
            var image = "Select Image";
            var video = "Select Video";
            var cancel = "cancel";
            var fileType = await userDialogs.ActionSheetAsync("", cancel, "", null, file, image, video);
            if (fileType == cancel)
            {
                ResetUI();
                return;
            }

            SkyFilePickerType chosenType;
            if (fileType == image)
                chosenType = SkyFilePickerType.Image;
            else if (fileType == video)
                chosenType = SkyFilePickerType.Video;
            else
                chosenType = SkyFilePickerType.Generic;

            var pickedFiles = await fileSystemService.PickFilesAsync(chosenType);
            if (pickedFiles == null)
            {
                ResetUI();
                return;
            }

            SlideSendButtonToCenterCommand?.Execute();

            //read contents of the selected files

            //TODO: optimise this, currently files' bytes are held in memory prior to upload
            IsStagingFiles = true;
            var userSkyFiles = new List<SkyFile>();

            try
            {
                foreach (var pickedFile in pickedFiles)
                {
                    if (pickedFile == null)
                        continue;

                    using var stream = await pickedFile.OpenReadAsync();
                    using var memoryStream = new MemoryStream();

                    await stream.CopyToAsync(memoryStream);

                    var fileBytes = memoryStream.GetBuffer();
                    var skyFile = new SkyFile()
                    {
                        Data = fileBytes,
                        FullFilePath = pickedFile.FullPath,
                        Filename = pickedFile.FileName,
                        FileSizeBytes = fileBytes.LongCount(),
                    };

                    userSkyFiles.Add(skyFile);
                }
            }
            catch (NullReferenceException ex)
            {
                Log.Exception(ex);
                Log.Trace("Error picking file.");

                IsStagingFiles = false;

                //reset the UI
                HandleUploadErrorCommand?.Execute();
                return;
            }

            //stage the files

            if (userSkyFiles.Count > 0)
            {
                StageFiles(userSkyFiles);
            }
            else
            {
                //reset the UI
                HandleUploadErrorCommand?.Execute();
            }

            IsStagingFiles = false;
        }

        private async Task FinishSendFile()
        {
            try
            {
                IsUploading = true;

                if (StagedFiles.Count() > 1)
                    FileToUpload = MakeZipFile();
                else
                    FileToUpload = StagedFiles.First();
                
                UpdateFileSize();

                StartUploadTimer(FileToUpload.FileSizeBytes);
                UploadedFile = await UploadFile();
                StopUploadTimer();

                FirstFileUploaded = true;

                //wait for progressbar to complete
                await Task.Delay(500);

                ResetBarcodeCommand?.Execute();

                //show QR code
                IsUploading = false;
                IsBarcodeLoading = true;
                SkyFileFullUrl = Util.GetSkylinkUrl(UploadedFile.Skylink);
                await GenerateBarcodeAsyncFunc();
            }
            catch (Exception e) when (e.Message == "Socket closed")
            {
                //user cancelled the upload
                userDialogs.Toast("Upload cancelled");

                //reset the UI
                HandleUploadErrorCommand?.Execute();
            }
            catch (Exception ex)
            {
                //an error occurred
                Log.Exception(ex);
                userDialogs.Toast("Could not upload file");

                //reset the UI
                HandleUploadErrorCommand?.Execute();
            }
            finally
            {
                StopUploadTimer();
                IsUploading = false;
                IsBarcodeLoading = false;
            }
        }

        private async Task ReceiveFile()
        {
            try
            {
                //don't allow user to scan barcode code while a file is uploading
                if (IsUploading) return;

                //don't allow user to scan barcode code while barcode is visible
                if (IsBarcodeVisible) return;

                IsSendButtonGreen = false;
                IsReceiveButtonGreen = true;

                //open the QR code scan view
                var barcodeData = await barcodeService.ScanBarcode();
                if (barcodeData == null)
                {
                    Log.Trace("barcodeData is null");
                    return;
                }

                var skyFile = new SkyFile() { Skylink = Util.GetRawSkylink(barcodeData) };
                await OpenFileInBrowser(skyFile);
            }
            catch (JsonException e)
            {
                Log.Exception(e);

                //show error message after the qr code scanner view has closed to avoid exception
                errorMessage = "Error: Invalid QR code";
                Log.Error(errorMessage);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void StageFiles(List<SkyFile> userFiles)
        {
            StagedFiles = userFiles;

            DropViewUIState = DropViewState.ConfirmFilesState;
        }

        private async Task<SkyFile> UploadFile()
        {
            uploadCancellationToken = new CancellationTokenSource();
            var skyFile = await apiService.UploadFile(FileToUpload.Filename, FileToUpload.Data,
                FileToUpload.FileSizeBytes, uploadCancellationToken);
            return skyFile;
        }

        private SkyFile MakeZipFile()
        {
            var filePaths = StagedFiles.Select(f => f.FullFilePath).ToArray();

            // TODO: add option to rename zip file in the renaming dialog
            string skyArchive = "skydrop_archive.zip";

            string compressedFilePath = Path.Combine(Path.GetTempPath(), skyArchive);

            bool compressSuccess = fileSystemService.CompressX(filePaths, compressedFilePath);

            // TODO: remove need for storing file bytes in SkyFiles, upload from file path instead
            var fileBytes = File.ReadAllBytes(compressedFilePath);

            var skyFile = new SkyFile()
            {
                Data = fileBytes,
                FullFilePath = compressedFilePath,
                Filename = skyArchive,
                FileSizeBytes = fileBytes.LongCount(),
            };

            return skyFile;
        }

        private void StartUploadTimer(long fileSizeBytes)
        {
            uploadTimerService.StartUploadTimer(fileSizeBytes, UpdateUploadProgress);
        }

        private void StopUploadTimer()
        {
            //fill progress bar
            UploadProgress = 1;
            UploadTimerText = "100%";

            uploadTimerService.StopUploadTimer();
        }

        private void UpdateUploadProgress()
        {
            var (newUploadProgress, newUploadTimerText) = uploadTimerService.GetUploadProgress();

            if (UploadProgress == 1)
                return;

            //scale the progress so it fits within 85% of the bar
            var maxProgress = 0.85;
            newUploadProgress *= maxProgress;
            if (newUploadProgress > maxProgress)
                newUploadProgress = maxProgress;

            UploadProgress = newUploadProgress;
            UploadTimerText = newUploadTimerText;
        }

        private void UpdateFileSize()
        {
            var bytesCount = FileToUpload.FileSizeBytes;
            FileSize = Util.GetFileSizeString(bytesCount);
        }

        private void CancelUpload()
        {
            uploadCancellationToken?.Cancel();
        }

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            return barcodeService.GenerateBarcode(text, width, height);
        }

        private string GetSkyLink()
        {
            if (UploadedFile == null)
            {
                Log.Error("User tried to copy skylink before file was uploaded");
                return null;
            }

            return Util.GetSkylinkUrl(UploadedFile.Skylink);
        }

        private async Task CopySkyLinkToClipboard()
        {
            try
            {
                if (UserIsSwiping()) return;

                string skyLink = GetSkyLink();
                if (skyLink == null)
                    return;

                await Xamarin.Essentials.Clipboard.SetTextAsync(skyLink);

                Log.Trace("Set clipboard text to " + skyLink);
                userDialogs.Toast("Copied SkyLink to clipboard");
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private Task NavToSettings()
        {
            return navigationService.Navigate<MenuViewModel>();
        }

        private async Task ShareLink()
        {
            try
            {
                if (UserIsSwiping()) return;

                string skyLink = GetSkyLink();
                if (skyLink == null)
                    return;

                await shareLinkService.ShareLink(skyLink);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        /// <summary>
        /// Asks DropView whether the user is currently performing a swipe gesture
        /// </summary>
        public bool UserIsSwiping()
        {
            CheckUserIsSwipingCommand?.Execute();

            return UserIsSwipingResult;
        }

        private async Task RenameStagedFile(SkyFile skyFile)
        {
            var fileExtension = skyFile.Filename.Split('.')?.LastOrDefault();

            var result = await userDialogs.PromptAsync("Rename file");
            if (string.IsNullOrEmpty(result.Value)) return;

            var newName = $"{result.Value}.{fileExtension}";

            skyFile.Filename = newName;
        }

        private async Task OpenFileInBrowser(SkyFile skyFile = null)
        {
            if (UserIsSwiping())
                return;

            var file = skyFile ?? UploadedFile;
            await Browser.OpenAsync(Util.GetSkylinkUrl(file.Skylink), new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show
            });
        }
    }
}