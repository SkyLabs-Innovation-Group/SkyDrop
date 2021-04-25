using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using SkyDrop.Droid;
using System.Drawing;
using Xamarin.Essentials;
using Android.Graphics;
using Java.IO;
using System.Threading.Tasks;
using System;
using MvvmCross;
using SkyDrop;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds byte array images to an ImageView
    /// </summary>
    public class ByteArrayImageViewBinding : MvxTargetBinding<ImageView, byte[]>
    {
        public static string Name => "Bytes";

        public ByteArrayImageViewBinding(ImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override async void SetValue(byte[] value)
        {
            try
            {
                if (value == null)
                    return;

                var bitmap = await Task.Run(() =>
                {
                    return BitmapFactory.DecodeByteArray(value, 0, value.Length);
                });

                if (bitmap == null)
                    return;

                Target.SetImageBitmap(bitmap);
            }
            catch(Exception e)
            {
                var log = Mvx.IoCProvider.Resolve<ILog>();
                log.Exception(e);
            }
        }
    }
}