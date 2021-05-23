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
using System.Threading.Tasks;
using System;
using System.IO;
using System.Net;
using System.Threading;
using FFImageLoading;
using MvvmCross;
using SkyDrop;
using SkyDrop.Core;
using SkyDrop.Core.DataModels;
using FFImageLoading.Cross;
using Serilog;
using Serilog.Core;
using File = Java.IO.File;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds a SkyFile to an MvxCachedImageView for file preview
    ///
    /// FFImageLoading handles optimising the stream, so I am generating it only right before passing it to Target.ImageStream.
    /// </summary>
    public class ImagePreviewBinding : MvxTargetBinding<MvxCachedImageView, string>
    {
        private ILog _log;
        private ILog log => _log ??= Mvx.IoCProvider.Resolve<ILog>();
        
        public static string Name => "ImagePreview";

        public ImagePreviewBinding(MvxCachedImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    Target.SetImageBitmap(null);
                    return;
                }
                
                if (!Target.DownsampleUseDipUnits)
                    Target.DownsampleUseDipUnits = true;


                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await ImageService.Instance.LoadStream(
                            c => Task.FromResult((Stream)System.IO.File.OpenRead(value)))
                            .IntoAsync(Target);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Error loading image binding");
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