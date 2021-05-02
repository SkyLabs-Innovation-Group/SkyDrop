using System;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.DataViewModels
{
    public class StagedFileDVM
    {
        public SkyFile SkyFile { get; set; }

        public IMvxCommand TapCommand { get; set; }

        public bool IsMoreFilesButton { get; set; }
    }
}
