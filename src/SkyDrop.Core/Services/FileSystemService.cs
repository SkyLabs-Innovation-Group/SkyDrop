using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    public class FileSystemService : IFileSystemService
    {
        public ILog log { get; }

        public FileSystemService(ILog log)
        {
            this.log = log;
        }

        public async Task<IEnumerable<FileResult>> PickFilesAsync()
        {
            var pickedFiles = await FilePicker.PickMultipleAsync();

            if (pickedFiles == null)
            {
                log.Trace("No file was picked");
            }

            return pickedFiles;
        }
    }
}
