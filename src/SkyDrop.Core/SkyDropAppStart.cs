using System;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.ViewModels.Main;

namespace SkyDrop.Core
{
    public class SkyDropAppStart : MvxAppStart
    {
        private readonly IMvxNavigationService navigationService;
        private readonly IFileSystemService fileSystemService;

        public SkyDropAppStart(IMvxApplication application,
            IMvxNavigationService navigationService,
            IFileSystemService fileSystemService,
            ILog log) : base(application, navigationService)
        {
            this.navigationService = navigationService;
            this.fileSystemService = fileSystemService;

            log.Trace(""); //blank line to separate launch sessions
            log.Trace("App launched");
        }

        protected override Task NavigateToFirstViewModel(object hint = null)
        {
            fileSystemService.ClearCache();

            return navigationService.Navigate<DropViewModel>();
        }
    }
}

