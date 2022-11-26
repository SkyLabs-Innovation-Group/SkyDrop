
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using SkyDrop.Droid.Styles;

namespace SkyDrop.Droid.Views.Barcode
{
    [Activity(Label = "BarcodeView")]
    public class BarcodeView : BaseActivity<BarcodeViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.BarcodeView;

        private ImageView barcodeImageView;
        private Timer textTimer;
        private EditText editText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            editText = FindViewById<EditText>(Resource.Id.BarcodeEditText);
            editText.RequestFocus();

            barcodeImageView = FindViewById<ImageView>(Resource.Id.BarcodeImageView);

            var textInputContainer = FindViewById<MaterialCardView>(Resource.Id.TextInputContainer);
            textInputContainer.Click += (s, e) =>
            {
                editText.RequestFocus();
                this.ShowKeyboard();
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
            catch(Exception e)
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

