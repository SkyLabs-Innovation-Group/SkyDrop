using System;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using MvvmCross.Commands;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Barcode
{
    [Activity(Label = "BarcodeView", Exported = true)]
    public class BarcodeView : BaseActivity<BarcodeViewModel>
    {
        private ImageView barcodeImageView;
        private ImageView closeKeyboardButton;
        private EditText editText;
        private Timer textTimer;
        protected override int ActivityLayoutId => Resource.Layout.BarcodeView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ViewModel.CloseKeyboardCommand = new MvxAsyncCommand(this.HideKeyboard);

            editText = FindViewById<EditText>(Resource.Id.BarcodeEditText);
            editText.RequestFocus();

            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImageView);

            closeKeyboardButton = FindViewById<ImageView>(Resource.Id.CloseKeyboardButton);

            var textInputContainer = FindViewById<MaterialCardView>(Resource.Id.TextInputContainer);
            textInputContainer.Click += (s, e) =>
            {
                editText.RequestFocus();
                this.ShowKeyboard();
            };

            editText.FocusChange += (s, e) =>
            {
                closeKeyboardButton.Visibility = editText.IsFocused ? ViewStates.Visible : ViewStates.Gone;
            };

            InitTextTimer();

            this.ShowKeyboard();
        }

        /// <summary>
        /// Generate and display QR code
        /// </summary>
        private async Task ShowBarcode(string text)
        {
            try
            {
                if (text.IsNullOrEmpty())
                {
                    //clear image view
                    RunOnUiThread(() => barcodeImageView.SetImageResource(Resource.Color.clear));
                    return;
                }

                var matrix = ViewModel.GenerateBarcode(text, barcodeImageView.Width, barcodeImageView.Height);
                var bitmap = await AndroidUtil.BitMatrixToBitmap(matrix);

                RunOnUiThread(() => barcodeImageView.SetImageBitmap(bitmap));
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        /// <summary>
        /// Delays the barcode generation so it doesn't need to render constantly while typing
        /// </summary>
        private void InitTextTimer()
        {
            textTimer = new Timer();
            textTimer.Interval = BarcodeViewModel.TextDelayerTimeMs;
            textTimer.Elapsed += TextTimer_Tick;
            editText.AfterTextChanged += (s, e) => textTimer.Start();
            textTimer.Start();
        }

        private void TextTimer_Tick(object sender, EventArgs e)
        {
            if (editText.IsFocused)
            {
                _ = ShowBarcode(editText.Text?.Trim());
                textTimer.Stop();
            }
        }
    }
}