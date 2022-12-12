using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.AppCenter.Crashes;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
using Xamarin.Essentials;
using ZXing.Common;
using static SkyDrop.Core.ViewModels.Main.FilesViewModel;
using Contact = SkyDrop.Core.DataModels.Contact;

namespace SkyDrop.Core.ViewModels.Main
{
    public class DropViewModel : BaseViewModel
    {
        public enum DropViewState
        {
            SendReceiveButtonState = 1,
            ConfirmFilesState = 2,
            QrCodeState = 3
        }

        public enum FileUploadResult
        {
            Success = 1,
            Fail = 2,
            Cancelled = 3
        }

        public enum HomeMenuItem
        {
            SkyDrive = 1,
            Portals = 2,
            Contacts = 3,
            Settings = 4
        }

        private const string ReceiveFileText = "RECEIVE";
        private const string ReceivingFileText = "RECEIVING...";
        private const string NoInternetPrompt = "Please check your internet connection";
        private readonly IApiService apiService;
        private readonly IBarcodeService barcodeService;
        private readonly IEncryptionService encryptionService;
        private readonly IFfImageService ffImageService;
        private readonly IFileSystemService fileSystemService;
        private readonly IMvxNavigationService navigationService;
        private readonly IShareLinkService shareLinkService;
        private readonly IStorageService storageService;
        private readonly IUploadTimerService uploadTimerService;
        private readonly IUserDialogs userDialogs;

        private DropViewState dropViewUiState;

        private string errorMessage;
        private TaskCompletionSource<SkyFile> iosMultipleImageSelectTask;
        private CancellationTokenSource uploadCancellationToken;

        public DropViewModel(ISingletonService singletonService,
            IApiService apiService,
            IStorageService storageService,
            IBarcodeService barcodeService,
            IShareLinkService shareLinkService,
            IUploadTimerService uploadTimerService,
            IUserDialogs userDialogs,
            IMvxNavigationService navigationService,
            IFileSystemService fileSystemService,
            IEncryptionService encryptionService,
            ILog log,
            IFfImageService fFImageService) : base(singletonService)
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
            this.encryptionService = encryptionService;
            ffImageService = fFImageService;

            //home state
            SendCommand = new MvxAsyncCommand(async () => await SendButtonTapped());
            ReceiveCommand = new MvxAsyncCommand(async () => await ReceiveFile());
            MenuSkyDriveCommand = new MvxAsyncCommand(() => HomeMenuTapped(HomeMenuItem.SkyDrive));
            MenuPortalsCommand = new MvxAsyncCommand(() => HomeMenuTapped(HomeMenuItem.Portals));
            MenuContactsCommand = new MvxAsyncCommand(() => HomeMenuTapped(HomeMenuItem.Contacts));
            MenuSettingsCommand = new MvxAsyncCommand(() => HomeMenuTapped(HomeMenuItem.Settings));

            //confirm upload state
            CancelUploadCommand = new MvxCommand(CancelUpload);
            ChooseRecipientCommand = new MvxAsyncCommand(() => OpenContactsMenu(true));
            ShowStagedFileMenuCommand =
                new MvxAsyncCommand<StagedFileDvm>(async stagedFile => await ShowStagedFileMenu(stagedFile.SkyFile));

            //QR code state
            CopyLinkCommand = new MvxAsyncCommand(async () => await CopySkyLinkToClipboard());
            ShareLinkCommand = new MvxAsyncCommand(async () => await ShareLink());
            CancelUploadCommand = new MvxCommand(CancelUpload);
            ShowStagedFileMenuCommand =
                new MvxAsyncCommand<StagedFileDvm>(async stagedFile => await ShowStagedFileMenu(stagedFile.SkyFile));
            OpenFileInBrowserCommand = new MvxAsyncCommand(async () => await OpenFileInBrowser());
            DownloadFileCommand = new MvxAsyncCommand(SaveOrUnzipFocusedFile);
            ShowBarcodeCommand = new MvxCommand(() => IsPreviewImageVisible = false);
            ShowPreviewImageCommand = new MvxCommand(() => IsPreviewImageVisible = true);
        }

        public IMvxCommand SendCommand { get; set; }
        public IMvxCommand ReceiveCommand { get; set; }
        public MvxAsyncCommand MenuSkyDriveCommand { get; }
        public MvxAsyncCommand MenuPortalsCommand { get; }
        public MvxAsyncCommand MenuContactsCommand { get; }
        public MvxAsyncCommand MenuSettingsCommand { get; }
        public IMvxCommand CopyLinkCommand { get; set; }
        public IMvxCommand ResetUiStateCommand { get; set; }
        public IMvxCommand ShareLinkCommand { get; set; }
        public IMvxCommand OpenFileInBrowserCommand { get; set; }
        public IMvxCommand DownloadFileCommand { get; set; }
        public IMvxCommand SlideSendButtonToCenterCommand { get; set; }
        public IMvxCommand SlideReceiveButtonToCenterCommand { get; set; }
        public IMvxCommand CancelUploadCommand { get; set; }
        public IMvxCommand ChooseRecipientCommand { get; }
        public IMvxCommand CheckUserIsSwipingCommand { get; set; }
        public IMvxCommand<StagedFileDvm> ShowStagedFileMenuCommand { get; set; }
        public IMvxCommand UpdateNavDotsCommand { get; set; }
        public IMvxCommand UploadStartedNotificationCommand { get; set; }
        public IMvxCommand<FileUploadResult> UploadFinishedNotificationCommand { get; set; }
        public IMvxCommand<double> UpdateNotificationProgressCommand { get; set; }
        public IMvxCommand IosSelectFileCommand { get; set; }
        public IMvxCommand ShowBarcodeCommand { get; set; }
        public IMvxCommand ShowPreviewImageCommand { get; set; }

        public bool IsUploading { get; set; }
        public bool IsStagingFiles { get; set; }
        public bool IsUploadArrowVisible => !IsUploading && !IsStagingFiles;
        public bool IsBarcodeLoading { get; set; }
        public bool IsBarcodeVisible { get; set; } //visibility for BarcodeContainer and BarcodeMenu
        public bool IsPreviewImageVisible { get; set; } //toggle for barcode / preview image
        public bool IsStagedFilesVisible => DropViewUiState == DropViewState.ConfirmFilesState;
        public bool IsReceiveButtonGreen { get; set; } = true;
        public string UploadTimerText { get; set; }
        public bool IsAnimatingBarcodeOut { get; set; }
        public string FileSize { get; set; }
        public double UploadProgress { get; set; } //0-1
        public bool SwipeNavigationEnabled { get; set; } //determines whether user can swipe to the QR code screen
        public bool UserIsSwipingResult { get; set; }
        public bool NavDotsVisible => DropViewUiState != DropViewState.ConfirmFilesState && SwipeNavigationEnabled;

        public string SendButtonLabel => IsEncrypting ? "ENCRYPTING" :
            IsUploading ? StagedFiles?.Count > 2 ? "SENDING FILES" : "SENDING FILE" : "SEND";

        public string ReceiveButtonLabel { get; set; } = ReceiveFileText;
        public bool IsReceivingFile { get; set; }
        public bool IsDownloadingFile { get; set; }
        public string PreviewImageUrl { get; set; }
        public bool CanDisplayPreview => FocusedFile?.Filename.CanDisplayPreview() ?? false;
        public bool IsShowBarcodeButtonVisible => CanDisplayPreview && IsPreviewImageVisible;
        public bool IsShowPreviewButtonVisible => CanDisplayPreview && !IsPreviewImageVisible;

        public bool IsFocusedFileAnArchive =>
            FocusedFile.Filename.ExtensionMatches(".zip") || FocusedFile.Filename.IsEncryptedZipFile();

        public string SaveButtonText => IsFocusedFileAnArchive ? "Unzip" : "Save";
        public string EncryptionText => GetVisibilityText();
        public bool IsEncrypting { get; set; }
        public Color EncryptionButtonColor => Recipient == null ? Colors.MidGrey : Colors.Primary;
        public Contact Recipient { get; set; }

        public List<StagedFileDvm> StagedFiles { get; set; }
        public SkyFile FocusedFile { get; set; } //most recently sent or received file
        public string FocusedFileUrl => FocusedFile?.GetSkylinkUrl();
        public SkyFile FileToUpload { get; set; }

        public DropViewState DropViewUiState
        {
            get => dropViewUiState;
            set
            {
                dropViewUiState = value;
                Log.Trace($"New UI State: {value}");

                UpdateNavDotsCommand?.Execute();
            }
        }

        public Func<string, Task> GenerateBarcodeAsyncFunc { get; set; }

        public bool UploadNotificationsEnabled { get; set; }

        public override async Task Initialize()
        {
            DropViewUiState = DropViewState.SendReceiveButtonState;

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
            ResetUi(true);

            //show error message after the qr code scanner view has closed to avoid exception
            if (!string.IsNullOrEmpty(errorMessage))
            {
                userDialogs.Toast(errorMessage);
                errorMessage = null;
            }

            if (!Preferences.Get(PreferenceKey.OnboardingComplete, false))
                //show onboarding
                navigationService.Navigate<OnboardingViewModel>();
        }

        public void ResetUi(bool leaveBarcode = false)
        {
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
            if (DropViewUiState == DropViewState.SendReceiveButtonState)
                await StartSendFile();
            else if (DropViewUiState == DropViewState.ConfirmFilesState) await FinishSendFile();
        }

        private async Task StartSendFile()
        {
            //don't allow user to select file while a file is uploading
            if (IsUploading) return;

            if (UserIsSwiping()) return;

            IsReceiveButtonGreen = false;

            //select file

            var pickedFiles = await SelectFiles();
            if (pickedFiles == null || pickedFiles.Count == 0)
            {
                ResetUi();
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

                if (Recipient != null)
                {
                    IsEncrypting = true;
                    var encryptedPath = await encryptionService.EncodeFileFor(FileToUpload.FullFilePath,
                        new List<Contact> { Recipient });

                    //we use a second property for the encrypted path so that we can preserve the original path, in case file needs to be encrypted again (after changing recipients)
                    FileToUpload.EncryptedFilePath = encryptedPath;
                    FileToUpload.EncryptedFilename = Path.GetFileName(encryptedPath);

                    IsEncrypting = false;
                }

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

                RaiseFocusedFileChanged();

                //show filename
                Title = FocusedFile.Filename;

                //clear cache
                fileSystemService.ClearCache();

                //wait for progressbar to complete
                await Task.Delay(500);

                //show QR code
                IsUploading = false;
                IsBarcodeLoading = true;
                await GenerateBarcodeAsyncFunc(FocusedFile.GetSkylinkUrl());

                IsPreviewImageVisible = false;

                UpdatePreviewImage();

                if (UploadNotificationsEnabled)
                    UploadFinishedNotificationCommand?.Execute(FileUploadResult.Success);
            }
            catch (TaskCanceledException tce)
            {
                HandleUploadError(tce, "Upload cancelled", FileUploadResult.Cancelled);
            }
            catch (Exception cancelledEx) when (cancelledEx.Message == "Socket is closed" ||
                                                (cancelledEx.Message == "Socket closed" &&
                                                 DeviceInfo.Platform == DevicePlatform.Android))
            {
                //catches Java.Net.SocketException when user cancels upload
                HandleUploadError(cancelledEx, "Upload cancelled", FileUploadResult.Cancelled);
            }
            catch (HttpRequestException httpEx) when (httpEx.Message.Contains("SSL") &&
                                                      DeviceInfo.Platform == DevicePlatform.Android)
            {
                HandleUploadError(httpEx, Strings.SslPrompt, FileUploadResult.Fail);
            }
            catch (UnauthorizedAccessException authEx)
            {
                HandleUploadError(authEx, "Access denied when reading selected files", FileUploadResult.Fail);
            }
            catch (Exception ex) // General error
            {
                HandleUploadError(ex, "Upload failed", FileUploadResult.Fail);
            }
            finally
            {
                StopUploadTimer(true);
                IsUploading = false;
                IsBarcodeLoading = false;
                IsEncrypting = false;
            }
        }

        private void HandleUploadError(Exception ex, string prompt, FileUploadResult result)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                prompt = NoInternetPrompt;

            if (result == FileUploadResult.Fail)
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
            string barcodeData = null;
            try
            {
                //don't allow user to scan barcode while a file is uploading
                if (IsUploading) return;

                //don't allow user to scan barcode while barcode is visible
                if (IsBarcodeVisible) return;

                //don't allow user to scan barcode from confirm upload screen
                if (DropViewUiState == DropViewState.ConfirmFilesState) return;

                IsReceiveButtonGreen = true;
                SlideReceiveButtonToCenterCommand.Execute();
                ReceiveButtonLabel = ReceivingFileText;
                IsReceivingFile = true;

                //open the QR code scan view
                barcodeData = await barcodeService.ScanBarcode();
                if (barcodeData == null)
                {
                    Log.Trace("barcodeData is null");
                    ResetUiStateCommand?.Execute();
                    return;
                }

                if (!SkyFile.IsSkyfile(barcodeData))
                {
                    //not a skylink
                    await OpenUrlInBrowser(barcodeData);
                    ResetUiStateCommand?.Execute();
                    return;
                }

                var skylink = barcodeData.Substring(barcodeData.Length - 46, 46);
                FocusedFile = new SkyFile { Skylink = skylink };

                IsPreviewImageVisible = true;

                await GenerateBarcodeAsyncFunc(FocusedFileUrl);

                var filename = await apiService.GetSkyFileFilename(FocusedFile.GetSkylinkUrl());
                FocusedFile.Filename = filename;
                storageService.SaveSkyFiles(FocusedFile);

                //show filename
                Title = FocusedFile.Filename;

                RaiseFocusedFileChanged();

                //can only do this after getting filename from Skynet
                UpdatePreviewImage();
            }
            catch (Exception e)
            {
                Log.Exception(e);

                var dict = new Dictionary<string, string>();
                dict.Add("Reason", "User scanned a QR code using Receive button");
                Crashes.TrackError(e);

                //avoid crashing android by NOT showing a toast before the scanner activity has closed
                var error = "Not a link, QR code content was copied to clipboard";
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    userDialogs.Toast(error);
                else
                    errorMessage = error;

                ResetUiStateCommand.Execute();

                await Clipboard.SetTextAsync(barcodeData);
            }
            finally
            {
                IsReceivingFile = false;
                ReceiveButtonLabel = ReceiveFileText;
                IsReceiveButtonGreen = true;
            }
        }

        private void StageFiles(List<SkyFile> userFiles, bool keepExisting)
        {
            var newStagedFiles = userFiles.Select(s => new StagedFileDvm
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
                newStagedFiles = newStagedFiles.GroupBy(x => x.SkyFile?.FullFilePath).Select(group => group.First())
                    .ToList();

                StagedFiles = newStagedFiles;

                //file size will be recalculated on next upload
                FileSize = "";
            }
            else
            {
                //staging for the first time

                //add more files button 
                newStagedFiles.Add(new StagedFileDvm
                {
                    IsMoreFilesButton = true,
                    TapCommand = new MvxAsyncCommand(AddMoreFiles)
                });

                StagedFiles = newStagedFiles;
            }

            DropViewUiState = DropViewState.ConfirmFilesState;
        }

        private async Task<SkyFile> UploadFile()
        {
            uploadCancellationToken = new CancellationTokenSource();
            var skyFile = await apiService.UploadFile(FileToUpload, uploadCancellationToken);
            return skyFile;
        }

        private SkyFile MakeZipFile()
        {
            var archiveName = new StringBuilder(Guid.NewGuid().ToString("N")) + ".zip";
            var compressedFilePath = Path.Combine(Path.GetTempPath(), archiveName);

            var filesToUpload = StagedFiles.Where(s => !s.IsMoreFilesButton).Select(s => s.SkyFile).ToList();
            var compressSuccess = fileSystemService.CreateZipArchive(filesToUpload, compressedFilePath);
            if (!compressSuccess)
                throw new Exception("Failed to create archive");

            var skyFile = new SkyFile
            {
                FullFilePath = compressedFilePath,
                Filename = archiveName,
                FileSizeBytes = new FileInfo(compressedFilePath).Length
            };

            return skyFile;
        }

        private async Task<List<SkyFile>> SelectFiles()
        {
            var isIos = DeviceInfo.Platform == DevicePlatform.iOS;
            var file = isIos ? "Select Files" : "Select Multiple Files";
            var image = isIos ? "Select Images" : "Select Image File";
            var video = isIos ? "Select Video" : "Select Video File";
            var text = "Text";
            var cancel = "cancel";
            var fileType = await userDialogs.ActionSheetAsync("", cancel, null, null, file, image, video, text);
            if (fileType == cancel)
                return null;

            if (fileType == text)
            {
                await navigationService.Navigate<BarcodeViewModel>();
                return null;
            }

            SkyFilePickerType chosenType;
            if (fileType == image)
            {
                if (IosSelectFileCommand != null)
                {
                    var imageFile = await IosPickImages(); //native multi image selection iOS
                    if (imageFile == null)
                        return null;

                    return new List<SkyFile> { imageFile };
                }

                chosenType = SkyFilePickerType.Image;
            }
            else if (fileType == video)
            {
                chosenType = SkyFilePickerType.Video;
            }
            else
            {
                chosenType = SkyFilePickerType.Generic;
            }

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

                    var skyFile = new SkyFile
                    {
                        FullFilePath = pickedFile.FullPath,
                        Filename = pickedFile.FileName,
                        FileSizeBytes = new FileInfo(pickedFile.FullPath).Length
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
                ResetUiStateCommand?.Execute();
                return null;
            }

            IsStagingFiles = false;

            return userSkyFiles;
        }

        private async Task<SkyFile> IosPickImages()
        {
            IosSelectFileCommand?.Execute();
            iosMultipleImageSelectTask = new TaskCompletionSource<SkyFile>();
            var skyFile = await iosMultipleImageSelectTask.Task;
            if (skyFile == null)
                return null;

            return skyFile;
        }

        private SkyFile IosConvertFilePathToSkyFile(string path)
        {
            return new SkyFile
            {
                FullFilePath = path,
                Filename = Path.GetFileName(path),
                FileSizeBytes = new FileInfo(path).Length
            };
        }

        public void IosStageImage(string path)
        {
            var skyFile = IosConvertFilePathToSkyFile(path);
            if (!iosMultipleImageSelectTask.Task.IsCompleted)
                iosMultipleImageSelectTask
                    .TrySetResult(skyFile); //first file(s) loaded causes UI to continue to next stage
            else
                StageFiles(new List<SkyFile> { skyFile }, true); //subsequent files get staged in a "lazy" manner
        }

        public void IosImagePickerFailed()
        {
            iosMultipleImageSelectTask.TrySetResult(null);
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

        private void StopUploadTimer(bool ignoreResult = false)
        {
            uploadTimerService.StopUploadTimer(ignoreResult);
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

            ResetUiStateCommand?.Execute();
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

                var skyLink = GetUploadedSkyLink();
                if (skyLink == null)
                    return;

                await Clipboard.SetTextAsync(skyLink);

                Log.Trace("Set clipboard text to " + skyLink);
                userDialogs.Toast("Copied SkyLink to clipboard");
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private Task OpenSettings()
        {
            return navigationService.Navigate<SettingsViewModel>();
        }

        private async Task ShareLink()
        {
            try
            {
                if (UserIsSwiping()) return;

                var skyLink = GetUploadedSkyLink();
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
        ///     Asks DropView whether the user is currently performing a swipe gesture
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
            var menuResult = await userDialogs.ActionSheetAsync("", cancel, "", null, rename, remove);

            switch (menuResult)
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

            var skylinkUrl = FocusedFile.GetSkylinkUrl();
            Log.Trace("Opening Skylink " + skylinkUrl);
            await Browser.OpenAsync(skylinkUrl, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show
            });
        }

        /// <summary>
        ///     Open a non-skylink URL
        ///     For opening SkyFiles, Use OpenFileInBrowser instead
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

        public async Task OpenSkyDrive()
        {
            if (IsUploading)
                return;

            var selectedFile = await navigationService.Navigate<FilesViewModel, NavParam, SkyFile>(new NavParam());
            if (selectedFile == null)
                return;

            SwipeNavigationEnabled = true;
            FocusedFile = selectedFile;

            //show filename
            Title = FocusedFile.Filename;

            RaiseFocusedFileChanged();

            UpdatePreviewImage();

            //show QR code
            IsBarcodeLoading = true;
            await GenerateBarcodeAsyncFunc(FocusedFile.GetSkylinkUrl());
        }

        public Task OpenPortalPreferences()
        {
            return navigationService.Navigate<PortalPreferencesViewModel>();
        }

        private async Task SaveOrUnzipFocusedFile()
        {
            try
            {
                if (IsFocusedFileAnArchive)
                {
                    //unzip
                    DownloadAndUnzipArchive();
                    return;
                }

                if (FocusedFile.Filename == null)
                {
                    //filename head request not yet completed, or failed
                    //we must have the filename in order to know how we should save the file

                    //retry getting filename
                    IsDownloadingFile = true;
                    var filename = await apiService.GetSkyFileFilename(FocusedFile.GetSkylinkUrl());
                    FocusedFile.Filename = filename;
                }

                IsDownloadingFile = false;
                var saveType = await Util.GetSaveType(FocusedFile.Filename);
                if (saveType == Util.SaveType.Cancel)
                    return;

                IsDownloadingFile = true;
                await apiService.DownloadAndSaveSkyfile(FocusedFileUrl, saveType);
            }
            catch (Exception e)
            {
                userDialogs.Toast(e.Message, TimeSpan.FromSeconds(4));
            }
            finally
            {
                IsDownloadingFile = false;
            }
        }

        private void DownloadAndUnzipArchive()
        {
            navigationService.Navigate<FilesViewModel, NavParam>(new NavParam
                { IsUnzippedFilesMode = true, ArchiveUrl = FocusedFileUrl, ArchiveName = FocusedFile.Filename });
        }

        private void UpdatePreviewImage()
        {
            PreviewImageUrl = null; //clear last preview image
            if (CanDisplayPreview)
                PreviewImageUrl = FocusedFileUrl; //load new preview image
            else
                IsPreviewImageVisible = false;
        }

        private void RaiseFocusedFileChanged()
        {
            RaisePropertyChanged(() => IsShowBarcodeButtonVisible).Forget();
            RaisePropertyChanged(() => IsShowPreviewButtonVisible).Forget();
            RaisePropertyChanged(() => IsFocusedFileAnArchive).Forget();
            RaisePropertyChanged(() => SaveButtonText).Forget();
        }

        private async Task HomeMenuTapped(HomeMenuItem menuItem)
        {
            switch (menuItem)
            {
                case HomeMenuItem.SkyDrive:
                    await OpenSkyDrive();
                    break;
                case HomeMenuItem.Portals:
                    await OpenPortalPreferences();
                    // var apiKey = await navigationService.Navigate<PortalLoginViewModel, string, string>("https://web3portal.com");
                    break;
                case HomeMenuItem.Contacts:
                    var isSelecting = DropViewUiState == DropViewState.ConfirmFilesState;
                    await OpenContactsMenu(isSelecting);
                    break;
                case HomeMenuItem.Settings:
                    await OpenSettings();
                    break;
            }
        }

        private async Task OpenContactsMenu(bool isSelecting)
        {
            var item = await navigationService.Navigate<ContactsViewModel, ContactsViewModel.NavParam, IContactItem>(
                new ContactsViewModel.NavParam { IsSelecting = isSelecting });
            if (item == null)
                return; //user tapped back button

            //set encryptionContact to null if item is AnyoneWithTheLinkItem
            Recipient = item is ContactDvm contactDvm ? contactDvm.Contact : null;
            RaisePropertyChanged(() => EncryptionText).Forget();
        }

        public string GetVisibilityText()
        {
            if (Recipient == null)
                return "Anyone with the link";

            return Recipient.Name;
        }
    }
}