using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using SkyDrop.Core.Services;
using ZXing.Common;

namespace SkyDrop.Core.ViewModels
{
    public class BarcodeViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IBarcodeService barcodeService;

        private string barcodeMessage;

        public IMvxCommand GenerateBarcodeCommand { get; set; }
        public IMvxCommand ScanBarcodeCommand { get; set; }

        public Func<Task> GenerateBarcodeAsyncFunc { get; set; }

        public BarcodeViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             IBarcodeService barcodeService,
                             ILog log) : base(singletonService)
        {
            Log = log;
            Title = "Scan";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.barcodeService = barcodeService;

            GenerateBarcodeCommand = new MvxAsyncCommand(async () => await GenerateBarcodeAsyncFunc());
            ScanBarcodeCommand = new MvxAsyncCommand(async () => await ScanBarcode());
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
    }
}
