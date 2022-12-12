using System;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using MvvmCross.Commands;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.iOS.Common;
using UIKit;
using Xamarin.Essentials;

namespace SkyDrop.iOS.Views.Barcode
{
    public partial class BarcodeView : BaseViewController<BarcodeViewModel>
    {
        private const int TextInputConstraintShort = 12;
        private const int TextInputConstraintLong = 60;
        private UIBarButtonItem closeKeyboardButton;
        private Timer textTimer;

        public BarcodeView() : base("BarcodeView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddBackButton(() => ViewModel.BackCommand.Execute());

            ViewModel.CloseKeyboardCommand = new MvxAsyncCommand(async () =>
            {
                View.EndEditing(true);
                NavigationItem.RightBarButtonItem = null;
                TextInputRightConstraint.Constant = TextInputConstraintShort;
            });

            TextInput.EditingDidBegin += (s, e) =>
            {
                NavigationItem.RightBarButtonItem = closeKeyboardButton;
                TextInputRightConstraint.Constant = TextInputConstraintLong;
            };

            BarcodeContainer.ClipsToBounds = true;
            BarcodeContainer.BackgroundColor = Colors.MidGrey.ToNative();
            BarcodeContainer.AddGestureRecognizer(new UITapGestureRecognizer(() => TextInput.BecomeFirstResponder()));
            TextInputContainer.BackgroundColor = Colors.MidGrey.ToNative();
            TextInputContainer.Layer.CornerRadius = 8;
            TextInput.TextColor = Colors.LightGrey.ToNative();
            TextInput.Layer.BorderWidth = 0;
            TextInput.BorderStyle = UITextBorderStyle.None;
            TextInput.Text = BarcodeViewModel.DefaultText;
            TextInput.BecomeFirstResponder();

            closeKeyboardButton = new UIBarButtonItem { Image = UIImage.FromBundle("ic_tick") };
            closeKeyboardButton.Clicked += (s, e) => ViewModel.CloseKeyboardCommand.Execute();

            InitTextTimer();

            var set = CreateBindingSet();
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
            set.Apply();
        }

        /// <summary>
        ///     Generate and display QR code
        /// </summary>
        private async Task ShowBarcode()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (TextInput.Text.IsNullOrEmpty())
                    {
                        //clear image view
                        BarcodeImage.Image = null;
                        return;
                    }

                    var screenDensity = (int)UIScreen.MainScreen.Scale;
                    var matrix = ViewModel.GenerateBarcode(TextInput.Text,
                        (int)BarcodeImage.Frame.Width * screenDensity, (int)BarcodeImage.Frame.Height * screenDensity);
                    var image = await IOsUtil.BitMatrixToImage(matrix);
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
        ///     Delays the barcode generation so it doesn't need to render constantly while typing
        /// </summary>
        private void InitTextTimer()
        {
            textTimer = new Timer();
            textTimer.Interval = BarcodeViewModel.TextDelayerTimeMs;
            textTimer.Elapsed += TextTimer_Tick;
            TextInput.EditingChanged += (s, e) => textTimer.Start();
            textTimer.Start();
        }

        private void TextTimer_Tick(object sender, EventArgs e)
        {
            _ = ShowBarcode();
            textTimer.Stop();
        }
    }
}