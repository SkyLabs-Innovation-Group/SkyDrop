using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using System;
using MvvmCross;
using UIKit;
using Foundation;
using Serilog;
using SkyDrop.Core;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;

namespace SkyDrop.iOS.Bindings
{
    /// <summary>
    /// Binds SkyFile to an ImageView for image previews
    /// </summary>
    public class SkyFileImageViewBinding : MvxTargetBinding<UIImageView, SkyFile>
    {
        public static string Name => "ImagePreview";

        public SkyFileImageViewBinding(UIImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(SkyFile value)
        {
            try
            {
                if (value == null)
                {
                    Target.Image = null;
                    return;
                }

                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        await using var stream = value.GetStream();
                        var previewImage = UIImage.LoadFromData(NSData.FromStream(stream));
                        Target.Image = previewImage;
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("Error setting SkyFile preview", ex);
                    }
                }).Forget();
            }
            catch (Exception e)
            {
                var log = Mvx.IoCProvider.Resolve<ILog>();
                log.Exception(e);
                Target.Image = null;
            }
        }
    }
}