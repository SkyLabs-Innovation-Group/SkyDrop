using MvvmCross.IoC;
using MvvmCross.ViewModels;
using SkyDrop.Core.ViewModels;

namespace SkyDrop.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();
            
            RegisterAppStart<DropViewModel>();
        }
    }
}
