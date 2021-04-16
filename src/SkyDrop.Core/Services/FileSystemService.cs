using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
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


        public async Task<IEnumerable<FileResult>> PickFilesAsync(SkyFilePickerType fileType)
        {
            IEnumerable<FileResult> pickedFiles;
            switch (fileType)
            {
                case SkyFilePickerType.Generic:
                    pickedFiles = await FilePicker.PickMultipleAsync();
                    break;
        
                case SkyFilePickerType.Image:
                    var pickPhoto = await MediaPicker.PickPhotoAsync();
                    pickedFiles = new List<FileResult>() { pickPhoto };
                    break;
        
                case SkyFilePickerType.Video:
                    var pickVideo = await MediaPicker.PickVideoAsync();
                    pickedFiles = new List<FileResult>() { pickVideo };
                    break;
        
                default:
                    throw new ArgumentException(nameof(fileType));
            }
        
            if (pickedFiles == null)
            {
                log.Trace("No file was picked");
            }
        
            return pickedFiles;
        }

        public bool CompressX(string[] filesToZip, string destinationZipFullPath)
        {
            try
            {
                // Delete existing zip file if exists
                if (File.Exists(destinationZipFullPath))
                    File.Delete(destinationZipFullPath);

                using (ZipArchive zip = ZipFile.Open(destinationZipFullPath, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToZip)
                    {
                        zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    }               
                }

                return File.Exists(destinationZipFullPath);
            }
            catch (Exception e)
            {
                log.Exception(e);
                return false;
            }
        }
    }
}
