using System.Drawing;
using Acr.UserDialogs;
using Android.Views;
using AndroidX.CardView.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataViewModels;

namespace SkyDrop.Droid.Bindings
{
    public class ContactsDeleteVisibilityBinding : MvxTargetBinding<View, IContactItem>
    {
        public static string Name => "ContactsDeleteVisibility";

        public ContactsDeleteVisibilityBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(IContactItem value)
        {
            Target.Visibility = value is AnyoneWithTheLinkItem ? ViewStates.Gone : ViewStates.Visible;
        }
    }
}