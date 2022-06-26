using System;
using System.Drawing;
using Acr.UserDialogs;
using AndroidX.CardView.Widget;
using FFImageLoading.Cross;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    public class SkyFilePreviewImageBinding : MvxTargetBinding<MvxCachedImageView, SkyFile>
    {
        public static string Name => "SkyFilePreviewImage";

        public SkyFilePreviewImageBinding(MvxCachedImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(SkyFile value)
        {
            Target.ImagePath = null;

            if (value == null)
                return;

            if (Util.CanDisplayPreview(value.Filename))
            {
                var filePath = value?.GetSkylinkUrl();
                Target.ImagePath = filePath;
            }
        }
    }
}