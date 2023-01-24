using System;
using System.Threading;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.iOS.Common;
using UIKit;

namespace SkyDrop.iOS.Bindings
{
    /// <summary>
    /// Binds local file to an ImageView for image previews
    /// </summary>
    public class LocalImagePreviewBinding : MvxTargetBinding<UIImageView, SkyFile>
    {
        public LocalImagePreviewBinding(UIImageView target) : base(target)
        {
        }

        public static string Name => "ImagePreview";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        private CancellationTokenSource tcs = new CancellationTokenSource();

        protected override void SetValue(SkyFile value)
        {
            try
            {
                tcs.Cancel();
                tcs = new CancellationTokenSource();

                Target.Image = null;

                if (value == null)
                    return;

                if (!value.FullFilePath.CanDisplayPreview())
                    return;

                IOsUtil.LoadLocalImagePreview(value.FullFilePath, Target, tcs.Token);
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