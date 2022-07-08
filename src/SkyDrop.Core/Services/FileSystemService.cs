using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Acr.UserDialogs;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using Xamarin.Essentials;

namespace SkyDrop.Core.Services
{
    public class FileSystemService : IFileSystemService
    {
        public string DownloadsFolderPath { get; set; }
        public string CacheFolderPath { get; set; }
        public ILog Log { get; }

        private readonly IUserDialogs userDialogs;

        public FileSystemService(ILog log, IUserDialogs userDialogs)
        {
            this.Log = log;
            this.userDialogs = userDialogs;
        }

        public async Task<IEnumerable<FileResult>> PickFilesAsync(SkyFilePickerType fileType)
        {
            var permissionResult = await Permissions.RequestAsync<Permissions.StorageRead>();

            if (permissionResult != PermissionStatus.Granted)
            {
                Log.Error("StorageRead permission not granted.");
                return null;
            }
            else
            {
                Log.Trace("PickFilesAsync() was called, with StorageRead permission granted");
            }

            IEnumerable<FileResult> pickedFiles = null;
            switch (fileType)
            {
                case SkyFilePickerType.Generic:
                    pickedFiles = await FilePicker.PickMultipleAsync();

                    if (pickedFiles == null)
                        break;
                    
                    break;
        
                case SkyFilePickerType.Image:
                    var pickPhoto = await MediaPicker.PickPhotoAsync();

                    if (pickPhoto == null)
                        break;
                    
                    pickedFiles = new List<FileResult>() { pickPhoto };
                    break;
        
                case SkyFilePickerType.Video:
                    var pickVideo = await MediaPicker.PickVideoAsync();

                    if (pickVideo == null)
                        break;
                    
                    pickedFiles = new List<FileResult>() { pickVideo };
                    break;
        
                default:
                    throw new ArgumentException(nameof(fileType));
            }
        
            if (pickedFiles == null)
            {
                Log.Trace("No file was picked");
            }
        
            return pickedFiles;
        }

        public bool CompressX(IEnumerable<SkyFile> filesToZip, string destinationZipFullPath)
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
                        zip.CreateEntryFromFile(file.FullFilePath, file.Filename, CompressionLevel.Optimal);
                    }               
                }

                return File.Exists(destinationZipFullPath);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return false;
            }
        }

        public async Task SaveFile(Stream data, string fileName, bool isPersistent)
        {
            string directory = isPersistent ? DownloadsFolderPath : CacheFolderPath;
            string filePath = Path.Combine(directory, fileName);
            using var fileStream = File.OpenWrite(filePath);
            await data.CopyToAsync(fileStream);
            data.Dispose();
        }

        public void ClearCache()
        {
            try
            {
                var cachePath = CacheFolderPath;
                var di = new DirectoryInfo(cachePath);
                foreach (var file in di.GetFiles())
                {
                    //don't delete realm files
                    if (file.Name.ExtensionMatches("realm", "realm.lock"))
                        continue;

                    file.Delete();
                }
                    
                foreach (var dir in di.GetDirectories())
                    dir.Delete(true);
            }
            catch (Exception e)
            {
                userDialogs.Toast("Failed to clear cache");
            }
        }
    }

    public enum SkyFilePickerType { Generic = 0, Image = 1, Video = 2 }

    public interface IFileSystemService
    {
        string DownloadsFolderPath { get; set; }
        string CacheFolderPath { get; set; }

        Task<IEnumerable<FileResult>> PickFilesAsync(SkyFilePickerType fileType);

        bool CompressX(IEnumerable<SkyFile> filesToZip, string destinationZipFullPath);

        void ClearCache();

        Task SaveFile(Stream data, string fileName, bool isPersistent);
    }
}
