﻿using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;

namespace SkyDrop.Core.DataViewModels
{
    public class SkynetPortalDVM : MvxNotifyPropertyChanged
    {
        private SkynetPortal portal;

        public SkynetPortalDVM(SkynetPortal portal)
        {
          this.portal = portal;
          this.Name = portal.Name;
          this.BaseUrl = portal.BaseUrl;
          this.RealmId = portal.Id;
          this.PortalPreferencesPosition = portal.PortalPreferencesPosition;
          this.ApiTokenPrefKey = portal.GetApiTokenPrefKey(); 
        }

        public string RealmId { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public int PortalPreferencesPosition { get; set; }
        public string ApiTokenPrefKey { get; set; }
        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand LongPressCommand { get; set; }

        public Task<string> GetApiToken() => SecureStorage.GetAsync(ApiTokenPrefKey);
    }
}

