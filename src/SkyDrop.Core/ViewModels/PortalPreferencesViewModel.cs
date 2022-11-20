﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using static SkyDrop.Core.ViewModels.EditPortalViewModel;

namespace SkyDrop.Core.ViewModels
{
    public class PortalPreferencesViewModel : BaseViewModel
    {
        private readonly IApiService apiService;
        private readonly IMvxNavigationService navigationService;
        private readonly IStorageService storageService;

        public IMvxCommand BackCommand { get; set; }

        public IMvxCommand AddNewPortalCommand { get; set; }

        private readonly IPortalService portalService;

        public MvxObservableCollection<SkynetPortalDVM> UserPortals { get; } = new MvxObservableCollection<SkynetPortalDVM>();

        public PortalPreferencesViewModel(ISingletonService singletonService,
                                          IApiService apiService,
                                          IMvxNavigationService navigationService,
                                          IPortalService portalService,
                                          IStorageService storageService) : base(singletonService)
        {
            this.apiService = apiService;
            this.navigationService = navigationService;
            this.storageService = storageService;

            Title = "Portal Preferences";
            UserPortals = new MvxObservableCollection<SkynetPortalDVM>();
            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
            AddNewPortalCommand = new MvxAsyncCommand(async() => await AddNewPortal());
            this.portalService = portalService;
        }

        private Task AddNewPortal() => navigationService.Navigate<EditPortalViewModel, NavParam>(new NavParam());

        public override void ViewAppearing()
        {
            base.ViewAppearing();
            LoadUserPortals();
        }

        public void LoadUserPortals()
        {
            var savedPortals = storageService.LoadSkynetPortals();

            if (savedPortals.Count == 0)
            {
                storageService.SaveSkynetPortal(new SkynetPortal(SkynetPortal.DefaultWeb3PortalUrl) { Name = "Web3 Portal"});
                savedPortals = storageService.LoadSkynetPortals();
            }

            var savedPortalsDvms = portalService.ConvertSkynetPortalsToDVMs(savedPortals);
            UserPortals.SwitchTo(savedPortalsDvms);
        }

        public void ReorderPortals(int position, int newPosition)
        {
            if (position == newPosition)
                return;

            var copy = new MvxObservableCollection<SkynetPortalDVM>(UserPortals);
            var portalCopy = copy[position];

            int newIndex = Math.Max(0, Math.Min(UserPortals.Count - 1, newPosition));
            copy.Move(position, newIndex);

            foreach (var portal in copy)
            {
                storageService.EditSkynetPortal(portal.RealmId);
            }

            UserPortals.SwitchTo(copy);
        }

        public void EditPortal(int position)
        {
            string portalId = UserPortals[position].RealmId;
            navigationService.Navigate<EditPortalViewModel, NavParam>(new NavParam() { PortalId = portalId });
        }
    }
}
