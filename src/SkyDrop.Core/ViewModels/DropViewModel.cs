using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
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
        public IMvxCommand CopyLinkCommand { get; set; }
        public IMvxCommand ResetUIStateCommand { get; set; }
        public IMvxCommand ResetBarcodeCommand { get; set; }
        public IMvxCommand NavToSettingsCommand { get; set; }
        public IMvxCommand ShareLinkCommand { get; set; }
        public IMvxCommand OpenFileInBrowserCommand { get; set; }
        public IMvxCommand SlideSendButtonToCenterCommand { get; set; }
        public IMvxCommand SlideReceiveButtonToCenterCommand { get; set; }
        public IMvxCommand CancelUploadCommand { get; set; }
        public IMvxCommand CheckUserIsSwipingCommand { get; set; }
        public IMvxCommand<StagedFileDVM> ShowStagedFileMenuCommand { get; set; }
        public IMvxCommand UpdateNavDotsCommand { get; set; }
        public IMvxCommand UploadStartedNotificationCommand { get; set; }
        public IMvxCommand<FileUploadResult> UploadFinishedNotificationCommand { get; set; }
        public IMvxCommand<double> UpdateNotificationProgressCommand { get; set; }
        public IMvxCommand ShowReceivedFileCommand { get; set; }

        public bool IsUploading { get; set; }
        public bool IsStagingFiles { get; set; }
        public bool IsUploadArrowVisible => !IsUploading && !IsStagingFiles;
        public bool IsBarcodeLoading { get; set; }
        public bool IsBarcodeVisible { get; set; }
        public bool IsStagedFilesVisible => DropViewUIState == DropViewState.ConfirmFilesState;
        public bool IsSendButtonGreen { get; set; } = true;
        public bool IsReceiveButtonGreen { get; set; } = true;
        public string UploadTimerText { get; set; }
        public bool IsAnimatingBarcodeOut { get; set; }
        public string FileSize { get; set; }
        public double UploadProgress { get; set; } //0-1
        public bool SwipeNavigationEnabled { get; set; } //determines whether user can swipe to the QR code screen
        public bool UserIsSwipingResult { get; set; }
        public bool NavDotsVisible => DropViewUIState != DropViewState.ConfirmFilesState && SwipeNavigationEnabled;
        public string SendButtonLabel => IsUploading ? StagedFiles?.Count > 2 ? "SENDING FILES" :
            "SENDING FILE" :
            DropViewUIState == DropViewState.ConfirmFilesState && StagedFiles?.Count > 2 ? "SEND FILES" : "SEND FILE";
        public string ReceiveButtonLabel { get; set; } = receiveFileText;
        public bool IsReceivingFile { get; set; }
        public bool CurrentFileOrigin { get; set; }

        public List<StagedFileDVM> StagedFiles { get; set; }
        public SkyFile FocusedFile { get; set; } //most recently sent or received file
        public SkyFile FileToUpload { get; set; }

        private DropViewState _dropViewUIState;

        public DropViewState DropViewUIState
        {
            get => _dropViewUIState;
            set
            {
                _dropViewUIState = value;
                Log.Trace($"New UI State: {value}");

                UpdateNavDotsCommand?.Execute();
            }
        }

        public enum DropViewState
        {
            SendReceiveButtonState = 1,
            ConfirmFilesState = 2,
            QRCodeState = 3
        }

        public enum FileUploadResult
        {
            Success = 1,
            Fail = 2,
            Cancelled = 3
        }

        private const string receiveFileText = "RECEIVE FILE";
        private const string receivingFileText = "RECEIVING FILE...";
        private string errorMessage;
        private CancellationTokenSource uploadCancellationToken;

        private Func<string, Task> _generateBarcodeAsyncFunc;
        public Func<string, Task> GenerateBarcodeAsyncFunc
        {
            get => _generateBarcodeAsyncFunc;
            set => _generateBarcodeAsyncFunc = value;
        }
        
        public bool UploadNotificationsEnabled { get; set; }

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
            Log = log;
            Title = "SkyDrop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.fileSystemService = fileSystemService;
            this.barcodeService = barcodeService;
            this.shareLinkService = shareLinkService;
            this.uploadTimerService = uploadTimerService;

            SendCommand = new MvxAsyncCommand(async () => await SendButtonTapped());
            ReceiveCommand = new MvxAsyncCommand(async () => await ReceiveFile());
            CopyLinkCommand = new MvxAsyncCommand(async () => await CopySkyLinkToClipboard());
            NavToSettingsCommand = new MvxAsyncCommand(async () => await NavToSettings());
            ShareLinkCommand = new MvxAsyncCommand(async () => await ShareLink());
            CancelUploadCommand = new MvxCommand(CancelUpload);
            ShowStagedFileMenuCommand = new MvxAsyncCommand<StagedFileDVM>(async stagedFile => await ShowStagedFileMenu(stagedFile.SkyFile));
            OpenFileInBrowserCommand = new MvxAsyncCommand(async () => await OpenFileInBrowser());
        }

        public override async Task Initialize()
        {
            DropViewUIState = DropViewState.SendReceiveButtonState;
            await base.Initialize();
        }

        public override void ViewAppearing()
        {
            UploadNotificationsEnabled = Preferences.Get(PreferenceKey.UploadNotificationsEnabled, true);
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

            var pickedFiles = await SelectFiles();
            if (pickedFiles == null || pickedFiles.Count == 0)
            {
                ResetUI();
                return;
            }

            //stage the files

            SlideSendButtonToCenterCommand?.Execute();
            StageFiles(pickedFiles, false);
        }

        private async Task FinishSendFile()
        {
            try
            {
                IsUploading = true;

                if (StagedFiles.Count() > 2) //file and add more files button
                    FileToUpload = MakeZipFile();
                else
                    FileToUpload = StagedFiles.First().SkyFile;
                
                var portal = SkynetPortal.SelectedPortal;
                
                FileToUpload.SetSkynetPortalUploadedTo(portal);
                
                UpdateFileSize();

                if (UploadNotificationsEnabled)
                    UploadStartedNotificationCommand?.Execute();

                StartUploadTimer(FileToUpload.FileSizeBytes);
                FocusedFile = await UploadFile();
                StopUploadTimer();

                //fill progress bar
                UploadProgress = 1;
                UploadTimerText = "100%";

                SwipeNavigationEnabled = true;

                //save skylink locally
                FocusedFile.WasSent = true;
                storageService.SaveSkyFiles(FocusedFile);

                //wait for progressbar to complete
                await Task.Delay(500);

                ResetBarcodeCommand?.Execute();

                //show QR code
                IsUploading = false;
                IsBarcodeLoading = true;
                await GenerateBarcodeAsyncFunc(FocusedFile.GetSkylinkUrl());
                
                if (UploadNotificationsEnabled)
                    UploadFinishedNotificationCommand?.Execute(FileUploadResult.Success);
            }
            catch (TaskCanceledException tce)
            {
                HandleUploadError(tce, "Upload cancelled", FileUploadResult.Cancelled);
            }
            catch (Exception cancelledEx) when (cancelledEx.Message == "Socket is closed" || cancelledEx.Message == "Socket closed" && DeviceInfo.Platform == DevicePlatform.Android)
            {
                //catches Java.Net.SocketException when user cancels upload
                HandleUploadError(cancelledEx, "Upload cancelled", FileUploadResult.Cancelled);
            }
            catch (HttpRequestException httpEx) when (httpEx.Message.Contains("SSL") && DeviceInfo.Platform == DevicePlatform.Android)
            {
                HandleUploadError(httpEx, Strings.SslPrompt, FileUploadResult.Fail);
            }
            catch (Exception ex) // General error
            {
                HandleUploadError(ex, "Could not upload file", FileUploadResult.Fail);
            }
            finally
            {
                StopUploadTimer();
                IsUploading = false;
                IsBarcodeLoading = false;
            }
        }

        private void HandleUploadError(Exception ex, string prompt, FileUploadResult result)
        {
            if(result == FileUploadResult.Fail)
            {
                userDialogs.Alert(prompt);
                Log.Exception(ex);
            }
            else if (result == FileUploadResult.Cancelled)
            {
                userDialogs.Toast(prompt);
            }

            //reset progress indicator for next attempt
            UploadProgress = 0;
            UploadTimerText = "";

            if (UploadNotificationsEnabled)
                UploadFinishedNotificationCommand?.Execute(result);
        }

        private async Task ReceiveFile()
        {
            try
            {
                //don't allow user to scan barcode while a file is uploading
                if (IsUploading) return;

                //don't allow user to scan barcode while barcode is visible
                if (IsBarcodeVisible) return;

                //don't allow user to scan barcode from confirm upload screen
                if (DropViewUIState == DropViewState.ConfirmFilesState) return;

                IsSendButtonGreen = false;
                IsReceiveButtonGreen = true;
                SlideReceiveButtonToCenterCommand.Execute();
                ReceiveButtonLabel = receivingFileText;
                IsReceivingFile = true;

                //open the QR code scan view
                var barcodeData = await barcodeService.ScanBarcode();
                if (barcodeData == null)
                {
                    Log.Trace("barcodeData is null");
                    return;
                }

                if (!SkyFile.IsSkyfile(barcodeData))
                {
                    //not a skylink
                    await OpenUrlInBrowser(barcodeData);
                    return;
                }

                string skylink = barcodeData.Substring(barcodeData.Length - 46, 46);
                FocusedFile = new SkyFile() { Skylink = skylink };
                ShowReceivedFileCommand.Execute();

                var filename = await apiService.GetSkyFileFilename(FocusedFile);
                FocusedFile.Filename = filename;
                storageService.SaveSkyFiles(FocusedFile);
            }
            catch (Exception e)
            {
                Log.Exception(e);

                //avoid crashing android by NOT showing a toast before the scanner activity has closed
                var error = "Invalid QR code";
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    userDialogs.Toast(error);
                else
                    errorMessage = error;

                ResetUIStateCommand.Execute();
            }
            finally
            {
                IsReceivingFile = false;
                ReceiveButtonLabel = receiveFileText;
                IsSendButtonGreen = true;
                IsReceiveButtonGreen = true;
            }
        }

        private void StageFiles(List<SkyFile> userFiles, bool keepExisting)
        {
            var newStagedFiles = userFiles.Select(s => new StagedFileDVM
            {
                SkyFile = s,
                TapCommand = new MvxAsyncCommand(async () => await ShowStagedFileMenu(s))
            }).ToList();

            if (keepExisting)
            {
                //adding more files

                var stagedFiles = StagedFiles;
                newStagedFiles.AddRange(stagedFiles);

                //remove duplicates
                newStagedFiles = newStagedFiles.GroupBy(x => x.SkyFile?.FullFilePath).Select(group => group.First()).ToList();

                StagedFiles = newStagedFiles;

                //file size will be recalculated on next upload
                FileSize = "";
            }
            else
            {
                //staging for the first time

                //add more files button 
                newStagedFiles.Add(new StagedFileDVM
                {
                    IsMoreFilesButton = true,
                    TapCommand = new MvxAsyncCommand(AddMoreFiles)
                });

                StagedFiles = newStagedFiles;
            }

            DropViewUIState = DropViewState.ConfirmFilesState;
        }

        private async Task<SkyFile> UploadFile()
        {
            uploadCancellationToken = new CancellationTokenSource();
            var skyFile = await apiService.UploadFile(FileToUpload, uploadCancellationToken);
            return skyFile;
        }

        private SkyFile MakeZipFile()
        {
            //TODO: add option to rename zip file in the renaming dialog
            string skyArchive = "skydrop_archive.zip";

            string compressedFilePath = Path.Combine(Path.GetTempPath(), skyArchive);

            var filesToUpload = StagedFiles.Where(s => !s.IsMoreFilesButton).Select(s => s.SkyFile).ToList();
            bool compressSuccess = fileSystemService.CompressX(filesToUpload, compressedFilePath);
            if (!compressSuccess)
                throw new Exception("Failed to create archive");

            var fileStream = File.OpenRead(compressedFilePath);
            var skyFile = new SkyFile()
            {
                FullFilePath = compressedFilePath,
                Filename = skyArchive,
                FileSizeBytes = fileStream.Length,
            };
            
            return skyFile;
        }

        private async Task<List<SkyFile>> SelectFiles()
        {
            var file = "Select Files";
            var image = "Select Image";
            var video = "Select Video";
            var cancel = "cancel";
            var fileType = await userDialogs.ActionSheetAsync("", cancel, null, null, file, image, video);
            if (fileType == cancel)
            {
                return null;
            }

            SkyFilePickerType chosenType;
            if (fileType == image)
                chosenType = SkyFilePickerType.Image;
            else if (fileType == video)
                chosenType = SkyFilePickerType.Video;
            else
                chosenType = SkyFilePickerType.Generic;

            var pickedFiles = await fileSystemService.PickFilesAsync(chosenType);

            //read contents of the selected files

            IsStagingFiles = true;
            var userSkyFiles = new List<SkyFile>();

            try
            {
                if (pickedFiles == null)
                    throw new ArgumentNullException(nameof(pickedFiles));
                
                foreach (var pickedFile in pickedFiles)
                {
                    if (pickedFile == null)
                        continue;

                    using var stream = await pickedFile.OpenReadAsync();
                    var skyFile = new SkyFile()
                    {
                        FullFilePath = pickedFile.FullPath,
                        Filename = pickedFile.FileName,
                        FileSizeBytes = stream.Length,
                    };

                    userSkyFiles.Add(skyFile);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                Log.Trace("Error picking file.");

                IsStagingFiles = false;

                //reset the UI
                ResetUIStateCommand?.Execute();
                return null;
            }

            IsStagingFiles = false;

            return userSkyFiles;
        }

        private async Task AddMoreFiles()
        {
            if (IsUploading)
                return;

            var pickedFiles = await SelectFiles();
            if (pickedFiles == null)
                return;

            StageFiles(pickedFiles, true);
        }

        private void StartUploadTimer(long fileSizeBytes)
        {
            UploadProgress = 0;
            uploadTimerService.StartUploadTimer(fileSizeBytes, UpdateUploadProgress);
        }

        private void StopUploadTimer()
        {
            uploadTimerService.StopUploadTimer();
        }

        private void UpdateUploadProgress()
        {
            var (newUploadProgress, newUploadTimerText) = uploadTimerService.GetUploadProgress();

            if (UploadProgress >= 1)
                return;

            //scale the progress so it fits within 85% of the bar
            var maxProgress = 0.85;
            newUploadProgress *= maxProgress;
            if (newUploadProgress > maxProgress)
                newUploadProgress = maxProgress;

            UploadProgress = newUploadProgress;
            UploadTimerText = newUploadTimerText;

            if (UploadNotificationsEnabled)
                UpdateNotificationProgressCommand?.Execute(newUploadProgress);
        }

        private void UpdateFileSize()
        {
            var bytesCount = FileToUpload.FileSizeBytes;
            FileSize = Util.GetFileSizeString(bytesCount);
        }

        private void CancelUpload()
        {
            if (IsUploading)
            {
                uploadCancellationToken?.Cancel();
                return;
            }

            ResetUIStateCommand?.Execute();
        }

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            return barcodeService.GenerateBarcode(text, width, height);
        }

        private string GetUploadedSkyLink()
        {
            if (FocusedFile == null)
            {
                Log.Error("User tried to copy skylink before file was uploaded");
                return null;
            }

            return FocusedFile.GetSkylinkUrl();
        }

        private async Task CopySkyLinkToClipboard()
        {
            try
            {
                if (UserIsSwiping()) return;

                string skyLink = GetUploadedSkyLink();
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

                string skyLink = GetUploadedSkyLink();
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

        private async Task ShowStagedFileMenu(SkyFile skyFile)
        {
            if (skyFile == null)
            {
                //add more files button clicked on Android
                await AddMoreFiles();
                return;
            }

            const string cancel = "Cancel";
            const string rename = "Rename";
            const string remove = "Remove";
            var menuResult = await userDialogs.ActionSheetAsync("", cancel, "", null, new[] { rename, remove });

            switch(menuResult)
            {
                case cancel:
                    break;
                case rename:
                    await RenameStagedFile(skyFile);
                    break;
                case remove:
                    DeleteStagedFile(skyFile);
                    break;
            }
        }

        private async Task RenameStagedFile(SkyFile skyFile)
        {
            var fileExtension = skyFile.Filename.Split('.')?.LastOrDefault();

            var result = await userDialogs.PromptAsync("Rename file");
            if (string.IsNullOrEmpty(result.Value)) return;

            var newName = $"{result.Value}.{fileExtension}";

            skyFile.Filename = newName;
        }

        private void DeleteStagedFile(SkyFile skyFile)
        {
            if (StagedFiles.Count == 2) //last file + add more files button
            {
                //cancel the send operation
                CancelUploadCommand?.Execute();
                return;
            }

            //make a new list without the specified skyFile
            StagedFiles = StagedFiles.Where(s => s.SkyFile?.FullFilePath != skyFile.FullFilePath).ToList();

            //file size will be recalculated on next upload
            FileSize = "";
        }

        private async Task OpenFileInBrowser()
        {
            if (UserIsSwiping())
                return;

            var fileToOpen = FocusedFile ?? FocusedFile;

            string skylinkUrl = fileToOpen.GetSkylinkUrl();
            Log.Trace("Opening Skylink " + skylinkUrl);
            await Browser.OpenAsync(skylinkUrl, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show
            });
        }

        /// <summary>
        /// Open a non-skylink URL
        /// For opening SkyFiles, Use OpenFileInBrowser instead
        /// </summary>
        private async Task OpenUrlInBrowser(string url)
        {
            await Browser.OpenAsync(url, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show
            });
        }

        public Task NavigateToSettings()
        {
            return navigationService.Navigate<SettingsViewModel>();
        }

        public async Task NavigateToFiles()
        {
            var selectedFile = await navigationService.Navigate<FilesViewModel, object, SkyFile>(null);
            if (selectedFile == null)
                return;

            SwipeNavigationEnabled = true;
            FocusedFile = selectedFile;

            //show QR code
            IsBarcodeLoading = true;
            await GenerateBarcodeAsyncFunc(FocusedFile.GetSkylinkUrl());
        }
    }
}