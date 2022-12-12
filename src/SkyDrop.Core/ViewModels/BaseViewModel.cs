using MvvmCross.ViewModels;
using SkyDrop.Core.Services;

namespace SkyDrop.Core.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        public readonly ISingletonService SingletonService;

        public BaseViewModel(ISingletonService singletonService)
        {
            this.SingletonService = singletonService;
            Log = singletonService.Log;
        }

        // Expose to views
        public ILog Log { get; protected set; }

        public string Title { get; set; }
    }
}