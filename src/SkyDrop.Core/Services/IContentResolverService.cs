using System;
using System.IO;

namespace SkyDrop.Core.Services
{
    public interface IContentResolverService
    {
        Stream GetContentStream(string contentUri);
    }
}
