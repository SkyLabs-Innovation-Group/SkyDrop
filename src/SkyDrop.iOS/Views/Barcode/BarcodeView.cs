﻿using System;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.iOS.Common;
using UIKit;
using Xamarin.Essentials;

namespace SkyDrop.iOS.Views.Barcode
{
    public partial class BarcodeView : BaseViewController<BarcodeViewModel>
    {
        private Timer textTimer;

        public BarcodeView() : base("BarcodeView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BarcodeContainer.ClipsToBounds = true;
            BarcodeContainer.BackgroundColor = Colors.MidGrey.ToNative();
            TextInputContainer.BackgroundColor = Colors.MidGrey.ToNative();
            TextInputContainer.Layer.CornerRadius = 8;
            TextInput.TextColor = Colors.LightGrey.ToNative();
            TextInput.Layer.BorderWidth = 0;
            TextInput.BorderStyle = UITextBorderStyle.None;

            InitTextTimer();
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var screenDensity = (int)UIScreen.MainScreen.Scale;
                    var matrix = ViewModel.GenerateBarcode(TextInput.Text, (int)BarcodeImage.Frame.Width * screenDensity, (int)BarcodeImage.Frame.Height * screenDensity);
                    var image = await iOSUtil.BitMatrixToImage(matrix);
                    BarcodeImage.Image = image;
                }
                catch (Exception ex)
                {
                    ViewModel.Log.Error("Error in ShowBarcode(): ");
                    ViewModel.Log.Exception(ex);
                }
            });
        }

        /// <summary>
        /// Delays the barcode generation so it doesn't need to render constantly while typing
        /// </summary>
        private void InitTextTimer()
        {
            textTimer = new Timer();
            textTimer.Interval = BarcodeViewModel.TextDelayerTimeMs;
            textTimer.Elapsed += TextTimer_Tick;
            TextInput.EditingChanged += (s, e) => textTimer.Start();
        }

        private void TextTimer_Tick(object sender, EventArgs e)
        {
            _ = ShowBarcode();
            textTimer.Stop();
        }
    }
}


