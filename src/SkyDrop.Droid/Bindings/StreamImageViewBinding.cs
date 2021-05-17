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
using System.IO;
using System.Threading;
using FFImageLoading.Cross;
using MvvmCross;
using SkyDrop;
using SkyDrop.Core;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds stream  images to an MvxCachedImageView
    /// </summary>
    public class StreamImageViewBinding : MvxTargetBinding<MvxCachedImageView, Stream>
    {
        private ILog _log;
        private ILog log => _log ??= Mvx.IoCProvider.Resolve<ILog>();
        
        public static string Name => "ImageStream";

        public StreamImageViewBinding(MvxCachedImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(Stream value)
        {
            try
            {
                if (value == null)
                {
                    Target.SetImageBitmap(null);
                    return;
                }
                
                MainThread.InvokeOnMainThreadAsync( () =>
                {
                    try
                    {
                        Target.ImageStream = c => Task.FromResult(value);
                    }
                    catch (Exception e)
                    {
                        log.Trace("Exception encountered while setting SkyFile's thumbnail in ImageStream binding");
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