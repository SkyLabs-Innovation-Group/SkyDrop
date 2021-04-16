using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    public enum SkyFilePickerType { Generic = 0, Image = 1, Video = 2 }

    public interface IFileSystemService
    {
        Task<IEnumerable<FileResult>> PickFilesAsync(SkyFilePickerType fileType);

        bool CompressX(string[] filesToZip, string destinationZipFullPath);
    }
}
