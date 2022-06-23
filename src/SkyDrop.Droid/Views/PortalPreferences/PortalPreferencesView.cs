﻿
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
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    [Activity(Label = "PortalPreferencesView")]
    public class PortalPreferencesView : BaseActivity<PortalPreferencesViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.PortalPreferencesView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
    }
}

