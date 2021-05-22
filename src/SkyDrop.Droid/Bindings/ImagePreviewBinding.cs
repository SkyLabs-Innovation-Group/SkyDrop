using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System;
using System.IO;
using FFImageLoading;
using MvvmCross;
using SkyDrop.Core;
using FFImageLoading.Cross;
using Serilog;

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
                        Log.Logger.Error("Error loading image binding", ex);
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