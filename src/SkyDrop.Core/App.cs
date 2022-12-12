using MvvmCross.IoC;
using MvvmCross.ViewModels;

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

            RegisterCustomAppStart<SkyDropAppStart>();
        }
    }
}