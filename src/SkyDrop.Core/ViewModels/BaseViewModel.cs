using MvvmCross.ViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        protected readonly ISingletonService singletonService;

        // Expose to views
        public ILog Log { get; protected set; }

        public BaseViewModel(ISingletonService singletonService)
        {
            this.singletonService = singletonService;
            this.Log = singletonService.Log;
        }

        public string Title { get; set; }
    }
}
