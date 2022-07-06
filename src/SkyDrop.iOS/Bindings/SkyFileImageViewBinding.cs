using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using System.Threading.Tasks;
using System;
using System.IO;
using FFImageLoading;
using MvvmCross;
using SkyDrop;
using UIKit;
using Foundation;
using Serilog;
using SkyDrop.Core;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;
using SkyDrop.Core.Utility;

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
                Target.Image = null;

                if (value == null)
                    return;
                
                if (!Util.CanDisplayPreview(value.FullFilePath))
                    return;

                Task.Run(async () =>
                {
                    try
                    {
                        var task = ImageService.Instance.LoadStream(
                                c => Task.FromResult((Stream) System.IO.File.OpenRead(value.FullFilePath)))
                            .DownSampleInDip()
                            .IntoAsync(Target);

                        await task;
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Error setting SkyFile preview");
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