using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Realms;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    public class ShareLinkService : IShareLinkService
    {
        private readonly ILog log;
        private readonly IUserDialogs userDialogs;

        public ShareLinkService(ILog log,
                                IUserDialogs userDialogs)
        {
            this.log = log;
            this.userDialogs = userDialogs;
        }

        public async Task ShareLink(string skylink)
        {
            userDialogs.Toast("Sharing");

            await Share.RequestAsync(new ShareTextRequest
            {
                Text = skylink,
                Title = "SkyLink"
            });
        }
    }

    public interface IShareLinkService
    {
        Task ShareLink(string skylink);
    }
}
