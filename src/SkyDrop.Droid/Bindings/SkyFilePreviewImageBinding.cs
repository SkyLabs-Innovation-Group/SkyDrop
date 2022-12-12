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

        protected override void SetValue(SkyFile value)
        {
            Target.ImagePath = null;

            if (value == null)
                return;

            if (!value.Filename.CanDisplayPreview())
                return;

            if (value.Skylink.IsNullOrEmpty())
            {
                AndroidUtil.LoadLocalImagePreview(value.FullFilePath, Target);
                return;
            }

            var filePath = value?.GetSkylinkUrl();
            Target.ImagePath = filePath;
        }
    }
}