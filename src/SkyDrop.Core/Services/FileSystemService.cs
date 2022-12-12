using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using SkyDrop.Core.DataModels;
using Xamarin.Essentials;
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.Core.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly ISaveToGalleryService saveToGalleryService;

        private readonly IUserDialogs userDialogs;
        private string cacheFolderPath;

        public FileSystemService(ILog log, IUserDialogs userDialogs, ISaveToGalleryService saveToGalleryService)
        {
            Log = log;
            this.userDialogs = userDialogs;
            this.saveToGalleryService = saveToGalleryService;
        }

        public ILog Log { get; }

        public string CacheFolderPath
        {
            get => cacheFolderPath;
            set
            {
                cacheFolderPath = value;
                ClearCache();
            }
        }

        public string DownloadsFolderPath { get; set; }

        public async Task<IEnumerable<FileResult>> PickFilesAsync(SkyFilePickerType fileType)
        {
            var permissionResult = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (permissionResult != PermissionStatus.Granted)
            {
                Log.Error("StorageRead permission not granted.");
                return null;
            }

            Log.Trace("PickFilesAsync() called with StorageRead permission granted");

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

                    pickedFiles = new List<FileResult> { pickPhoto };
                    break;

                case SkyFilePickerType.Video:
                    var pickVideo = await MediaPicker.PickVideoAsync();

                    if (pickVideo == null)
                        break;

                    pickedFiles = new List<FileResult> { pickVideo };
                    break;

                default:
                    throw new ArgumentException(nameof(fileType));
            }

            if (pickedFiles == null) Log.Trace("No file was picked");

            return pickedFiles;
        }

        public bool CreateZipArchive(IEnumerable<SkyFile> filesToZip, string destinationZipFullPath)
        {
            try
            {
                // Delete existing zip file if exists
                if (File.Exists(destinationZipFullPath))
                    File.Delete(destinationZipFullPath);

                var filenames = new List<string>();
                using (var zip = ZipFile.Open(destinationZipFullPath, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToZip)
                    {
                        //ensure filenames are unique to avoid extraction errors
                        var filename = GetNextZipFilename(file.Filename, filenames);
                        filenames.Add(filename);
                        zip.CreateEntryFromFile(file.FullFilePath, filename, CompressionLevel.Optimal);
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

        public List<SkyFile> UnzipArchive(Stream data)
        {
            var unzipFolder = Path.Combine(CacheFolderPath, "unzipped/");
            if (!Directory.Exists(unzipFolder))
                Directory.CreateDirectory(unzipFolder);

            //clear unzip folder
            var di = new DirectoryInfo(unzipFolder);
            foreach (var file in di.GetFiles())
                file.Delete();
            foreach (var dir in di.GetDirectories())
                dir.Delete(true);

            //extract
            var zipFile = new ZipArchive(data);
            zipFile.ExtractToDirectory(unzipFolder);

            di = new DirectoryInfo(unzipFolder);
            if (di.GetDirectories().Count() > 0)
                throw new Exception("SkyDrop doesn't support viewing archives which contain folders");

            var files = di.GetFiles().Select(f => new SkyFile { Filename = f.Name, FullFilePath = f.FullName })
                .ToList();
            if (files.Count == 0)
                throw new Exception("The archive appears to be empty");

            return files;
        }

        public void ExtractArchiveToDevice(Stream data, string archiveName)
        {
            var folderName = Path.GetFileNameWithoutExtension(archiveName);
            var unzipFolder = Path.Combine(DownloadsFolderPath, $"{folderName}/");
            if (Directory.Exists(unzipFolder))
                throw new Exception($"Directory already exists {unzipFolder}");

            Directory.CreateDirectory(unzipFolder);

            //extract
            var zipFile = new ZipArchive(data);
            zipFile.ExtractToDirectory(unzipFolder);

            userDialogs.Toast($"Extracted archive to /{folderName}");
        }

        public async Task<string> SaveFile(Stream data, string fileName, bool isPersistent)
        {
            var directory = isPersistent ? DownloadsFolderPath : CacheFolderPath;
            var filePath = Path.Combine(directory, fileName);

            //ensure path is unique by adding a number at the end if it already exists
            filePath = GetNextFilename(filePath);

            using var fileStream = File.OpenWrite(filePath);
            await data.CopyToAsync(fileStream);
            data.Dispose();
            return filePath;
        }

        public async Task<string> SaveToGalleryOrFiles(Stream data, string filename, SaveType saveType)
        {
            var newFileName = "";
            if (saveType == SaveType.Photos)
            {
                //save to photos gallery
                var newPath = await saveToGalleryService.SaveToGallery(data, filename);
                newFileName = Path.GetFileName(newPath);
            }
            else
            {
                //save to downloads folder
                var newPath = await SaveFile(data, filename, true);
                newFileName = Path.GetFileName(newPath);
            }

            return newFileName;
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

        private string GetNextZipFilename(string filename, List<string> filenames)
        {
            var i = 1;
            var extension = GetFullExtension(filename);
            var file = Path.GetFileName(filename);
            var fileWithoutExtension = file.Substring(0, file.Length - extension.Length) + " {0}";

            while (filenames.Contains(filename))
                filename = string.Format(fileWithoutExtension, "(" + i++ + ")") + extension;

            return filename;
        }

        private string GetNextFilename(string filename)
        {
            var i = 1;
            var dir = Path.GetDirectoryName(filename);
            var extension = GetFullExtension(filename);
            var file = Path.GetFileName(filename);
            var fileWithoutExtension = file.Substring(0, file.Length - extension.Length) + " {0}";

            while (File.Exists(filename))
                filename = Path.Combine(dir, string.Format(fileWithoutExtension, "(" + i++ + ")") + extension);

            return filename;
        }
    }

    public enum SkyFilePickerType
    {
        Generic = 0,
        Image = 1,
        Video = 2
    }

    public interface IFileSystemService
    {
        string DownloadsFolderPath { get; set; }
        string CacheFolderPath { get; set; }

        Task<IEnumerable<FileResult>> PickFilesAsync(SkyFilePickerType fileType);

        bool CreateZipArchive(IEnumerable<SkyFile> filesToZip, string destinationZipFullPath);

        List<SkyFile> UnzipArchive(Stream data);

        void ExtractArchiveToDevice(Stream data, string archiveName);

        void ClearCache();

        Task<string> SaveFile(Stream data, string fileName, bool isPersistent);

        Task<string> SaveToGalleryOrFiles(Stream data, string filename, SaveType saveType);
    }
}