using MvvmCross.ViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        public readonly ISingletonService singletonService;

        public BaseViewModel(ISingletonService singletonService)
        {
            this.singletonService = singletonService;
            Log = singletonService.Log;
        }

        // Expose to views
        public ILog Log { get; protected set; }

        public string Title { get; set; }
    }
}