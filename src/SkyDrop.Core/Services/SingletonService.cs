using System;
using System.Diagnostics;
using Acr.UserDialogs;
using MvvmCross;

namespace SkyDrop.Core.Services
{
    // Uses this simple pattern, to lazily initialize all our singletons and expose them publicly
    // private T _instance;
    // public T Instance => _instance ??= ResolveSingleton<T>();

    // Due to how Mvx.IocProvider works, it is necessary before trying to access services from SingletonService, to make sure that
    // the service has already been initialized via dependency injection in a VM's constructor, or else you will get runtime errors.

    public class SingletonService : ISingletonService
    {
        private IApiService apiService;
        private ILog log;

        private IStorageService storageService;

        private IUserDialogs userDialogs;
        public ILog Log => log ??= ResolveSingleton<ILog>();
        public IStorageService StorageService => storageService ??= ResolveSingleton<IStorageService>();
        public IApiService ApiService => apiService ??= ResolveSingleton<IApiService>();
        public IUserDialogs UserDialogs => userDialogs ??= ResolveSingleton<IUserDialogs>();

        /// <summary>
        /// This method will try to return the singleton resolved using Mvx.IocProvider, and will log and re-throw any caught
        /// exceptions.
        /// </summary>
        private T ResolveSingleton<T>() where T : class
        {
            try
            {
                var resolved = Mvx.IoCProvider.Resolve<T>();

                if (resolved == null)
                    throw new ArgumentNullException(nameof(T), "Error - resolved singleton was null");

                return resolved;
            }
            catch (Exception ex)
            {
                var message = "Exception caught resolving Singleton for type " + nameof(T) + ". Returning null...";
                if (log == null)
                    Debug.WriteLine(message);
                else
                    log.Trace(message);

                Log.Exception(ex);

                throw;
            }
        }
    }

    // Remember to expose new singletons on the public interface
    public interface ISingletonService
    {
        public ILog Log { get; }

        public IStorageService StorageService { get; }

        public IApiService ApiService { get; }

        public IUserDialogs UserDialogs { get; }
    }
}