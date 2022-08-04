using System;
using System.IO;
using System.Threading.Tasks;

namespace SkyDrop.Core.Services
{
	public interface ISaveToGalleryService
	{
		Task SaveToGallery(Stream imageData);
	}
}

