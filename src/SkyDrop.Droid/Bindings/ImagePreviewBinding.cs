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
using Path = System.IO.Path;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds a SkyFile to an MvxCachedImageView for file preview
    ///
    /// FFImageLoading handles optimising the stream, so I am generating it only right before passing it to Target.ImageStream.
    /// </summary>
    public class ImagePreviewBinding : MvxTargetBinding<MvxCachedImageView, SkyFile>
    {
        private ILog _log;
        private ILog log => _log ??= Mvx.IoCProvider.Resolve<ILog>();
        
        public static string Name => "ImagePreview";

        public ImagePreviewBinding(MvxCachedImageView target) : base(target)
        {
        }
        
        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(SkyFile value)
        {
            try
            {
                Target.SetImageBitmap(null);
                
                if (string.IsNullOrEmpty(value?.FullFilePath))
                    return;

                string extension = Path.GetExtension(value.FullFilePath).ToLowerInvariant();

                bool shouldSetImagePreview;
                switch (extension) 
                {
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".bmp":
                    case ".tiff":
                        shouldSetImagePreview = true;
                        break;
                    default:
                        shouldSetImagePreview = false;
                        break;
                }
                
                if (!shouldSetImagePreview)
                    return;

                Task.Run(async () =>
                {
                    try
                    {
                        var task = ImageService.Instance.LoadFile(value.FullFilePath)
                            .DownSampleInDip()
                            .IntoAsync(Target);

                        await task;
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