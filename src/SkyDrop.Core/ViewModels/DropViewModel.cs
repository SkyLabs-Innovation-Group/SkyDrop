﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private readonly IFileSystemService fileSystemService;
        private readonly IBarcodeService barcodeService;
        private readonly IShareLinkService shareLinkService;
        private readonly IUploadTimerService uploadTimerService;

        public IMvxCommand SendCommand { get; set; }
        public IMvxCommand ReceiveCommand { get; set; }
        public IMvxCommand OpenFileCommand { get; set; }
        public IMvxCommand ShareCommand { get; set; }
        public IMvxCommand CopyLinkCommand { get; set; }
        public IMvxCommand HandleUploadErrorCommand { get; set; }
        public IMvxCommand ResetBarcodeCommand { get; set; }
        public IMvxCommand NavToSettingsCommand { get; set; }
        public IMvxCommand ShareLinkCommand { get; set; }
        public IMvxCommand OpenFileInBrowserCommand { get; set; }
        public IMvxCommand SlideSendButtonToCenterCommand { get; set; }
        //public IMvxCommand ResetAnimateCommand { get; set; }

        public string SkyFileJson { get; set; }
        public bool IsUploading { get; set; }
        public bool IsBarcodeLoading { get; set; }
        public bool IsBarcodeVisible { get; set; }
        public string SendButtonLabel => IsUploading ? "SENDING FILE" : "SEND FILE";
        public bool IsSendButtonGreen { get; set; } = true;
        public bool IsReceiveButtonGreen { get; set; } = true;
        public string UploadTimerText { get; set; }
        public bool IsAnimatingBarcodeOut { get; set; }
        public string FileSize { get; set; }
        public double UploadProgress { get; set; } //0-1

        private string errorMessage;

        public SkyFile SkyFile { get; set; }

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

            SendCommand = new MvxAsyncCommand(StartSendFile);
            ReceiveCommand = new MvxAsyncCommand(ReceiveFile);
            CopyLinkCommand = new MvxAsyncCommand(CopySkyLinkToClipboard);
            NavToSettingsCommand = new MvxAsyncCommand(NavToSettings);
            ShareLinkCommand = new MvxAsyncCommand(ShareLink);
        }

        public override void ViewAppeared()
        {
            Log.Trace($"{nameof(DropViewModel)} ViewAppeared()");

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
            FileSize = "";
            IsBarcodeVisible = false;
            IsAnimatingBarcodeOut = false;
            UploadProgress = 0;
        }

        private async Task StartSendFile()
        {
            //don't allow user to select file while a file is uploading
            if (IsUploading) return;

            IsSendButtonGreen = true;
            IsReceiveButtonGreen = false;

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
            else
            {
                SkyFilePickerType chosenType;
                if (fileType == image)
                    chosenType = SkyFilePickerType.Image;
                else if (fileType == video)
                    chosenType = SkyFilePickerType.Video;
                else
                    chosenType = SkyFilePickerType.Generic;

                var pickedFiles = await fileSystemService.PickFilesAsync(chosenType);
                SlideSendButtonToCenterCommand?.Execute();

                byte[] fileBytes = null;
                try
                {
                    var firstFile = pickedFiles.First();

                    using (var stream = await firstFile.OpenReadAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);

                        fileBytes = memoryStream.GetBuffer();
                    }
                }
                catch (NullReferenceException ex)
                {
                    Log.Exception(ex);
                    Log.Trace("No file was picked.");
                    HandleUploadErrorCommand?.Execute();

                    return;
                }

                SkyFile = new SkyFile
                {
                    Filename = pickedFiles.First().FileName,
                    Data = fileBytes,
                    FileSizeBytes = fileBytes.LongCount(),
                };

                await StageFile(SkyFile);
            }
        }

        private async Task FinishSendFile()
        {
            try
            {
                IsUploading = true;

                StartUploadTimer();
                SkyFile = await UploadFile();
                StopUploadTimer();

                //wait for progressbar to complete
                await Task.Delay(500);

                ResetBarcodeCommand?.Execute();

                //show QR code
                IsUploading = false;
                IsBarcodeLoading = true;
                SkyFileJson = JsonConvert.SerializeObject(SkyFile);
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
            this.SkyFile = stagedFile;
            UpdateFileSize();

            await FinishSendFile();
        }

        private async Task<SkyFile> UploadFile()
        {
            var skyFile = await apiService.UploadFile(this.SkyFile.Filename, this.SkyFile.Data, this.SkyFile.FileSizeBytes);
            return skyFile;
        }

        private void StartUploadTimer()
        {
            uploadTimerService.StartUploadTimer(SkyFile.FileSizeBytes, UpdateUploadProgress);
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
            var (uploadProgress, uploadTimerText) = uploadTimerService.GetUploadProgress();

            if (UploadProgress != 1)
            {
                UploadProgress = uploadProgress;
                UploadTimerText = uploadTimerText;
            }
        }

        private void UpdateFileSize()
        {
            var bytesCount = SkyFile.FileSizeBytes;
            FileSize = Util.GetFileSizeString(bytesCount);
        }

        public BitMatrix GenerateBarcode(string text, int width, int height)
        {
            return barcodeService.GenerateBarcode(text, width, height);
        }

        private string GetSkyLink()
        {
            if (SkyFile.Status == FileStatus.Staged)
            {
                Log.Error("User tried to copy skylink before file was uploaded");
                return null;
            }

            return Util.GetSkylinkUrl(SkyFile.Skylink);
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
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}
