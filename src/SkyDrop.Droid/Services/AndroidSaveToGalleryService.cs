using System;
using System.IO;
using System.Threading.Tasks;
using SkyDrop.Core.Services;

namespace SkyDrop.Droid.Services
{
	public class AndroidSaveToGalleryService : ISaveToGalleryService
	{
        public Task SaveToGallery(Stream imageData)
        {
            throw new NotImplementedException("Save to gallery not available on Android");
        }
	}
}

