using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Realms;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    public class ShareLinkService : IShareLinkService
    {
        private readonly ILog log;

        public ShareLinkService(ILog log)
        {
            this.log = log;
        }

        public async Task ShareLink(string skylink)
        {
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
