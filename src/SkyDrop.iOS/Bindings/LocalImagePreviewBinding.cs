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
using SkyDrop.iOS.Common;

namespace SkyDrop.iOS.Bindings
{
    /// <summary>
    /// Binds local file to an ImageView for image previews
    /// </summary>
    public class LocalImagePreviewBinding : MvxTargetBinding<UIImageView, SkyFile>
    {
        public static string Name => "ImagePreview";

        public LocalImagePreviewBinding(UIImageView target) : base(target)
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

                iOSUtil.LoadLocalImagePreview(value.FullFilePath, Target);
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