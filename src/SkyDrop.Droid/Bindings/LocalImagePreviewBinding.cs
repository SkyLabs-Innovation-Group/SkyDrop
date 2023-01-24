using System;
using System.Threading;
using FFImageLoading.Cross;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds a SkyFile to an MvxCachedImageView for file preview
    /// FFImageLoading handles optimising the stream, so I am generating it only right before passing it to
    /// Target.ImageStream.
    /// </summary>
    public class LocalImagePreviewBinding : MvxTargetBinding<MvxCachedImageView, SkyFile>
    {
        public LocalImagePreviewBinding(MvxCachedImageView target) : base(target)
        {
        }

        private static ILog Log => Mvx.IoCProvider.Resolve<ILog>();

        public static string Name => "ImagePreview";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        private CancellationTokenSource tcs = new CancellationTokenSource();

        protected override void SetValue(SkyFile value)
        {
            try
            {
                tcs.Cancel();
                tcs = new CancellationTokenSource();

                Target.SetImageBitmap(null);

                if (string.IsNullOrEmpty(value?.FullFilePath))
                    return;

                if (!value.FullFilePath.CanDisplayPreview())
                    return;

                AndroidUtil.LoadLocalImagePreview(value.FullFilePath, Target, tcs.Token);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}