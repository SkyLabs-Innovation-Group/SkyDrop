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
using SkyDrop.Core;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds byte array images to an ImageView
    /// </summary>
    public class ByteArrayImageViewBinding : MvxTargetBinding<ImageView, byte[]>
    {
        private ILog _log;
        private ILog log => _log ??= Mvx.IoCProvider.Resolve<ILog>();
        
        public static string Name => "Bytes";

        public ByteArrayImageViewBinding(ImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(byte[] value)
        {
            try
            {
                if (value == null)
                {
                    Target.SetImageBitmap(null);
                    return;
                }

                // GC.Collect();

                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        var bitmap = await BitmapFactory.DecodeByteArrayAsync(value, 0, value.Length);
                        Target.SetImageBitmap(bitmap);
                    }
                    catch (Exception e)
                    {
                        log.Exception(e);
                    }
                }).Forget();
            }
            catch(Exception e)
            {
                log.Exception(e);
            }
        }
    }
}