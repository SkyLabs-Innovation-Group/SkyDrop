
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.Droid.Styles;

namespace SkyDrop.Droid.Views.Onboarding
{
    [Activity(Label = "OnboardingView")]
    public class OnboardingView : BaseActivity<OnboardingViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.OnboardingView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var previousButton = FindViewById<MaterialCardView>(Resource.Id.PreviousButton);
            previousButton.StyleButton(Colors.Primary, true);
        }
    }
}

