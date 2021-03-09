using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using Newtonsoft.Json;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Services;
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

        public string SkyFileJson { get; set; }
        public bool IsLoading { get; set; }

        private StagedFile stagedFile { get; set; }
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

                var skyFile = await UploadFile();

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

        public async Task StageFile(StagedFile stagedFile)
        {
            this.stagedFile = stagedFile;

            await FinishSendFile();
        }

        private async Task<SkyFile> UploadFile()
        {
            var skyFile = await apiService.UploadFile(stagedFile.Filename, stagedFile.Data);
            return skyFile;
        }

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            return barcodeService.GenerateBarcode(text, width, height);
        }
    }
}
