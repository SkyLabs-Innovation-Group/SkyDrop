using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;
using ZXing.Common;

namespace SkyDrop.Core.ViewModels.Main
{
    public class DropViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly IBarcodeService barcodeService;
        private readonly IShareLinkService shareLinkService;

        public IMvxCommand SendCommand { get; set; }
        public IMvxCommand ReceiveCommand { get; set; }
        public IMvxCommand<SkyFile> OpenFileCommand { get; set; }
        public IMvxCommand ShareCommand { get; set; }
        public IMvxCommand CopyLinkCommand { get; set; }
        public IMvxCommand HandleUploadErrorCommand { get; set; }
        public IMvxCommand ResetBarcodeCommand { get; set; }
        public IMvxCommand NavToSettingsCommand { get; set; }
        public IMvxCommand ShareLinkCommand { get; set; }

        public string SkyFileJson { get; set; }
        public bool IsUploading { get; set; }
        public bool IsBarcodeLoading { get; set; }
        public bool IsBarcodeVisible { get; set; }
        public string SendButtonLabel => IsUploading ? "SENDING FILE" : "SEND FILE";
        public bool IsSendButtonGreen { get; set; } = true;
        public bool IsReceiveButtonGreen { get; set; } = true;
        public string UploadTimerText { get; set; }
        public bool IsAnimatingBarcodeOut { get; set; }

        private SkyFile skyFile { get; set; }
        private string errorMessage;
        private Stopwatch stopwatch;
        private Timer timer;

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
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "SkyDrop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.barcodeService = barcodeService;
            this.shareLinkService = shareLinkService;

            SendCommand = new MvxAsyncCommand(StartSendFile);
            ReceiveCommand = new MvxAsyncCommand(ReceiveFile);
            CopyLinkCommand = new MvxAsyncCommand(CopySkyLinkToClipboard);
            NavToSettingsCommand = new MvxAsyncCommand(NavToSettings);
            ShareLinkCommand = new MvxAsyncCommand(ShareLink);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            //make sure buttons return to green when returning from QR scanner
            ResetUI();

            //show error message after the qr code scanner view has closed to avoid exception
            if (!string.IsNullOrEmpty(errorMessage))
            {
                userDialogs.Toast(errorMessage);
                errorMessage = null;
            }
        }

        public void ResetUI()
        {
            IsSendButtonGreen = true;
            IsReceiveButtonGreen = true;
            UploadTimerText = "";
            IsBarcodeVisible = false;
            IsAnimatingBarcodeOut = false;
        }

        private async Task StartSendFile()
        {
            //don't allow user to select file while a file is uploading
            if (IsUploading) return;

            IsSendButtonGreen = true;
            IsReceiveButtonGreen = false;

            var file = "Select File";
            var image = "Select Image";
            var cancel = "cancel";
            var fileType = await userDialogs.ActionSheetAsync("", cancel, "", null, file, image);
            if (fileType == cancel)
            {
                ResetUI();
                return;
            }

            if (fileType == file)
            {
                await SelectFileAsyncFunc();
            }
            else
            {
                await SelectImageAsyncFunc();
            }
        }

        private async Task FinishSendFile()
        {
            try
            {
                IsUploading = true;
                _ = RaisePropertyChanged(() => IsUploading);

                StartUploadTimer();
                skyFile = await UploadFile();
                StopUploadTimer();

                ResetBarcodeCommand?.Execute();

                //show QR code
                IsUploading = false;
                _ = RaisePropertyChanged(() => IsUploading);
                IsBarcodeLoading = true;
                _ = RaisePropertyChanged(() => IsBarcodeLoading);
                SkyFileJson = JsonConvert.SerializeObject(skyFile);
                await GenerateBarcodeAsyncFunc();
            }
            catch (Exception e)
            {
                Log.Exception(e);
                userDialogs.Toast("Could not upload file");
                HandleUploadErrorCommand?.Execute();
            }
            finally
            {
                StopUploadTimer();
                IsUploading = false;
                _ = RaisePropertyChanged(() => IsUploading);
                IsBarcodeLoading = false;
                _ = RaisePropertyChanged(() => IsBarcodeLoading);
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
                var codeJson = await barcodeService.ScanBarcode();
                if (codeJson == null)
                    return;

                var skyFile = JsonConvert.DeserializeObject<SkyFile>(codeJson);

                //open the file in browser
                OpenFileCommand.Execute(skyFile);
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

        public async Task StageFile(SkyFile stagedFile)
        {
            this.skyFile = stagedFile;

            await FinishSendFile();
        }

        private async Task<SkyFile> UploadFile()
        {
            var skyFile = await apiService.UploadFile(this.skyFile.Filename, this.skyFile.Data);
            return skyFile;
        }

        private void StartUploadTimer()
        {
            void UpdateTimerText()
            {
                UploadTimerText = stopwatch.Elapsed.ToString(@"mm\:ss");
            }

            stopwatch = new Stopwatch();
            timer = new Timer();
            timer.Elapsed += (s, e) => UpdateTimerText();
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Start();
            stopwatch.Start();
            UpdateTimerText();
        }

        private void StopUploadTimer()
        {
            stopwatch.Stop();
            timer.Stop();
        }

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            return barcodeService.GenerateBarcode(text, width, height);
        }

        private string GetSkyLink()
        {
            if (skyFile.Status == FileStatus.Staged)
            {
                Log.Error("User tried to copy skylink before file was uploaded");
                return null;
            }

            return Util.GetSkylinkUrl(skyFile.Skylink);
        }

        private async Task CopySkyLinkToClipboard()
        {
            string skyLink = GetSkyLink();
            if (skyLink == null)
                return;

            await Xamarin.Essentials.Clipboard.SetTextAsync(skyLink);

            Log.Trace("Set clipboard text to " + skyLink);
            userDialogs.Toast("Copied SkyLink to clipboard");
        }

        private Task NavToSettings()
        {
            return navigationService.Navigate<MenuViewModel>();
        }

        private async Task ShareLink()
        {
            try
            {
                string skyLink = GetSkyLink();
                if (skyLink == null)
                    return;

                await shareLinkService.ShareLink(skyLink);
            }
            catch(Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}
