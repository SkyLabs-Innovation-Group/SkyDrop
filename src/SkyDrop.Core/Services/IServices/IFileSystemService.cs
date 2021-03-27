using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    public interface IFileSystemService
    {
        Task<IEnumerable<FileResult>> PickFilesAsync();
    }
}
