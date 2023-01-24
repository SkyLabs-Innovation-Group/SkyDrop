using System.Threading;
using FFImageLoading.Cross;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    public class SkyFilePreviewImageBinding : MvxTargetBinding<MvxCachedImageView, SkyFile>
    {
        public SkyFilePreviewImageBinding(MvxCachedImageView target) : base(target)
        {
        }

        public static string Name => "SkyFilePreviewImage";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        private CancellationTokenSource tcs = new CancellationTokenSource();

        protected override void SetValue(SkyFile value)
        {
            tcs.Cancel();
            tcs = new CancellationTokenSource();

            Target.ImagePath = null;

            if (value == null)
                return;

            if (!value.Filename.CanDisplayPreview())
                return;

            if (value.Skylink.IsNullOrEmpty())
            {
                AndroidUtil.LoadLocalImagePreview(value.FullFilePath, Target, tcs.Token);
                return;
            }

            var filePath = value?.GetSkylinkUrl();
            Target.ImagePath = filePath;
        }
    }
}