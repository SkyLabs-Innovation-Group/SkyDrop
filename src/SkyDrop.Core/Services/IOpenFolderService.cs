using System;
using static SkyDrop.Core.Utility.Util;

namespace SkyDrop.Core.Services
{
    public interface IOpenFolderService
    {
        void OpenFolder(SaveType saveType);
    }
}

