using Android.Widget;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.ViewModels.Main;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Maps a local drawable onto the layout type toggle in files view
    /// </summary>
    public class LayoutImageBinding : MvxTargetBinding<ImageView, FileLayoutType>
    {
        public LayoutImageBinding(ImageView target) : base(target)
        {
        }

        private ILog Log => Mvx.IoCProvider.Resolve<ILog>();

        public static string Name => "LayoutImage";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(FileLayoutType value)
        {
            Target.SetImageResource(
                value == FileLayoutType.List ? Resource.Drawable.ic_grid : Resource.Drawable.ic_list);
        }
    }
}