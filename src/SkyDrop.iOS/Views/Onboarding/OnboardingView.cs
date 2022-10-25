using System;
using Acr.UserDialogs;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using UIKit;

namespace SkyDrop.iOS.Views.Onboarding
{
    [MvxChildPresentation]
    public partial class OnboardingView : BaseViewController<OnboardingViewModel>
    {
        public OnboardingView() : base("OnboardingView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            MainTextView.TextColor = Colors.LightGrey.ToNative();

            StyleButton(GotItButton);

            var set = this.CreateBindingSet();
            set.Bind(MainTextView).To(vm => vm.OnboardingText);
            set.Bind(GotItButton).To(vm => vm.BackCommand);
            set.Bind(this).For(v => v.Title).To(vm => vm.Title);
            set.Apply();
        }
    }
}


