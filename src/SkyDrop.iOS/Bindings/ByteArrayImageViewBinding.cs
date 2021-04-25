using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using System.Threading.Tasks;
using System;
using MvvmCross;
using SkyDrop;
using UIKit;
using Foundation;

namespace SkyDrop.iOS.Bindings
{
    /// <summary>
    /// Binds byte array images to an ImageView
    /// </summary>
    public class ByteArrayImageViewBinding : MvxTargetBinding<UIImageView, byte[]>
    {
        public static string Name => "Bytes";

        public ByteArrayImageViewBinding(UIImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(byte[] value)
        {
            try
            {
                var previewImage = UIImage.LoadFromData(NSData.FromArray(value));
                Target.Image = previewImage;
            }
            catch (Exception e)
            {
                var log = Mvx.IoCProvider.Resolve<ILog>();
                log.Exception(e);
            }
        }
    }
}