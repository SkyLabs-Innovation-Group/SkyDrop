using System;
using System.Threading.Tasks;
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

        public IMvxCommand SendCommand { get; set; }
        public IMvxCommand ReceiveCommand { get; set; }
        public IMvxCommand<SkyFile> OpenFileCommand { get; set; }
        public IMvxCommand ShareCommand { get; set; }
        public IMvxCommand CopyLinkCommand { get; set; }

        public string SkyFileJson { get; set; }
        public bool IsLoading { get; set; }
        public bool IsBarcodeHidden { get; set; } = true;
        public string SendButtonLabel => IsLoading ? "SENDING FILE" : "SEND FILE";
        public bool SendButtonState { get; set; } = true;
        public bool ReceiveButtonState { get; set; } = true;

        private SkyFile skyFile { get; set; }
        private string errorMessage;

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
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "Drop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.barcodeService = barcodeService;

            SendCommand = new MvxAsyncCommand(StartSendFile);
            ReceiveCommand = new MvxAsyncCommand(ReceiveFile);
            CopyLinkCommand = new MvxAsyncCommand(CopyFileLinkToClipboard);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            //show error message after the qr code scanner view has closed to avoid exception
            if (!string.IsNullOrEmpty(errorMessage))
            {
                userDialogs.Toast(errorMessage);
                errorMessage = null;
            }
        }

        private async Task StartSendFile()
        {
            //don't allow user to select file while a file is uploading
            if (IsLoading) return;

            SendButtonState = true;
            ReceiveButtonState = false;

            var file = "Select File";
            var image = "Select Image";
            var cancel = "cancel";
            var fileType = await userDialogs.ActionSheetAsync("", cancel, "", null, file, image);
            if (fileType == cancel)
                return;

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
                IsLoading = true;
                _ = RaisePropertyChanged(() => IsLoading);

                skyFile = await UploadFile();

                //show QR code
                SkyFileJson = JsonConvert.SerializeObject(skyFile);
                await GenerateBarcodeAsyncFunc();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            finally
            {
                IsLoading = false;
                _ = RaisePropertyChanged(() => IsLoading);
            }
        }

        private async Task ReceiveFile()
        {
            try
            {
                //don't allow user to scan barcode code while a file is uploading
                if (IsLoading) return;

                //don't allow user to scan barcode code while barcode is visible
                if (!IsBarcodeHidden) return;

                SendButtonState = false;
                ReceiveButtonState = true;

                //prompt user with instructions
                var message = "Scan the QR code on the sender device to receive the file";
                await userDialogs.AlertAsync(message);

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

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            return barcodeService.GenerateBarcode(text, width, height);
        }

        private async Task CopyFileLinkToClipboard()
        {
            if (skyFile.Status == FileStatus.Staged)
            {
                Log.Error("User tried to copy skylink before file was uploaded");
                return;
            }

            string skyLink = Util.GetSkylinkUrl(skyFile.Skylink);
            await Xamarin.Essentials.Clipboard.SetTextAsync(skyLink);

            Log.Trace("Set clipboard text to " + skyLink);
            userDialogs.Toast("Copied SkyLink to clipboard");
        }
    }
}
