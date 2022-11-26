using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using ZXing.Common;

namespace SkyDrop.Core.ViewModels.Main
{
    public class BarcodeViewModel : BaseViewModel
    {
        public const int TextDelayerTimeMs = 300;
        public const string DefaultText = "QR text";

        public IMvxCommand GenerateBarcodeCommand { get; set; }
        public IMvxCommand ScanBarcodeCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }
        public IMvxCommand CloseKeyboardCommand { get; set; }

        private Func<Task> _generateBarcodeAsyncFunc;
        public Func<Task> GenerateBarcodeAsyncFunc
        {
            get => _generateBarcodeAsyncFunc;
            set => _generateBarcodeAsyncFunc = value;
        }

        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IBarcodeService barcodeService;
        private readonly IMvxNavigationService navigationService;

        private string barcodeMessage;

        public BarcodeViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             IBarcodeService barcodeService,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "Create QR";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.barcodeService = barcodeService;
            this.navigationService = navigationService;

            GenerateBarcodeCommand = new MvxAsyncCommand(async () => await GenerateBarcodeAsyncFunc());
            ScanBarcodeCommand = new MvxAsyncCommand(async () => await ScanBarcode());
            BackCommand = new MvxAsyncCommand(Done);
        }

        private async Task ScanBarcode()
        {
            barcodeMessage = await barcodeService.ScanBarcode();
        }

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            return barcodeService.GenerateBarcode(text, width, height);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (!string.IsNullOrEmpty(barcodeMessage))
                userDialogs.Toast(barcodeMessage, new TimeSpan(0, 0, 10));
        }

        private async Task Done()
        {
            await navigationService.Close(this);
            CloseKeyboardCommand?.Execute();
        }
    }
}
