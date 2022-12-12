using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataViewModels;

namespace SkyDrop.Droid.Bindings
{
    public class ContactsDeleteVisibilityBinding : MvxTargetBinding<View, IContactItem>
    {
        public ContactsDeleteVisibilityBinding(View target) : base(target)
        {
        }

        public static string Name => "ContactsDeleteVisibility";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(IContactItem value)
        {
            Target.Visibility = value is AnyoneWithTheLinkItem ? ViewStates.Gone : ViewStates.Visible;
        }
    }
}