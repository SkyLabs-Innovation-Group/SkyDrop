using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Bindings;
using SkyDrop.iOS.Styles;
using UIKit;

namespace SkyDrop.iOS.Views.Onboarding
{
    [MvxChildPresentation]
    public partial class OnboardingView : BaseViewController<OnboardingViewModel>
    {
        private bool isFirstConstraintUpdate = true;

        public OnboardingView() : base("OnboardingView", null)
        {
        }

        public string DescriptionText
        {
            get => "";
            set
            {
                MainTextView.Text = value;

                //resize text box to fit
                var totalHorizontalMargin = 22;
                var idealSize =
                    MainTextView.SizeThatFits(new CGSize(UIScreen.MainScreen.Bounds.Width - totalHorizontalMargin,
                        double.MaxValue));
                DescriptionHeightConstraint.Constant = idealSize.Height;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddBackButton(() => ViewModel.BackCommand.Execute());

            Icon.TintColor = Colors.LightGrey.ToNative();

            TitleLabel.TextColor = Colors.LightGrey.ToNative();
            MainTextView.TextColor = Colors.LightGrey.ToNative();

            NextButton.StyleButton(Colors.Primary, true);
            PreviousButton.StyleButton(Colors.Primary, true);

            var set = CreateBindingSet();
            set.Bind(TitleLabel).To(vm => vm.TitleText);
            set.Bind(this).For(t => t.DescriptionText).To(vm => vm.DescriptionText);
            set.Bind(NextButton).For(NextButtonStyleBinding.Name).To(vm => vm.IsLastPage);
            set.Bind(PreviousButton).For(a => a.Hidden).To(vm => vm.IsFirstPage);
            set.Bind(NextButton).For("Tap").To(vm => vm.NextPageCommand);
            set.Bind(PreviousButton).For("Tap").To(vm => vm.PreviousPageCommand);
            set.Bind(Icon).For(IconBinding.Name).To(vm => vm.Icon);
            set.Bind(this).For(v => v.Title).To(vm => vm.Title);
            set.Apply();
        }
    }
}