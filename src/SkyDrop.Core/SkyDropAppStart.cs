using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.ViewModels.Main;

namespace SkyDrop.Core
{
    public class SkyDropAppStart : MvxAppStart
    {
        private readonly IFFImageService ffImageService;
        private readonly IFileSystemService fileSystemService;
        private readonly IMvxNavigationService navigationService;

        public SkyDropAppStart(IMvxApplication application,
            IMvxNavigationService navigationService,
            IFileSystemService fileSystemService,
            IFFImageService ffImageService,
            ILog log) : base(application, navigationService)
        {
            this.navigationService = navigationService;
            this.fileSystemService = fileSystemService;
            this.ffImageService = ffImageService;

            log.Trace(""); //blank line to separate launch sessions
            log.Trace("App launched");
        }

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            ffImageService.Initialize();

            await navigationService.Navigate<DropViewModel>();
        }
    }
}