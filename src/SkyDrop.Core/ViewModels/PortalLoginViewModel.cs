﻿using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public class PortalLoginViewModel : BaseViewModel, IMvxViewModel<bool, string>
    {
        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;

        public string PortalUrl { get; set; } = "https://account.web3portal.com";
        public bool DidSetApiKey { get; set; }
        public bool IsLoggedIn { get; set; }
        public TaskCompletionSource<object> CloseCompletionSource { get; set; }

        public PortalLoginViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "SkyDrop";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
        }

        public void SetApiKey(string apiKey)
        {
            if (DidSetApiKey)
                return;

            DidSetApiKey = true;

            Console.WriteLine(apiKey);

            userDialogs.Toast("Logged in");

            navigationService.Close(this, apiKey);
        }

        public void Prepare(bool parameter)
        {
        }
    }
}

