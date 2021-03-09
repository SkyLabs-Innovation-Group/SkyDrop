using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Realms;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.DataViewModels
{
    public class SkyFileDVM : MvxNotifyPropertyChanged
    {
        public SkyFile SkyFile { get; set; }

        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand OpenCommand { get; set; }
        public IMvxCommand CopySkyLinkCommand { get; set; }
        public IMvxCommand DeleteCommand { get; set; }

        public bool IsSelected { get; set; }
        public bool IsLoading { get; set; }

        public Color FillColor => SkyFile.Status == FileStatus.Uploaded ? Colors.PrimaryDark : Colors.MidGrey;

        public void SetUploaded(SkyFile skyFile)
        {
            this.SkyFile.BitField = skyFile.BitField;
            this.SkyFile.Skylink = skyFile.Skylink;
            this.SkyFile.Merkelroot = skyFile.Merkelroot;
            this.SkyFile.Filename = skyFile.Filename;
            this.SkyFile.Status = skyFile.Status;
            this.SkyFile.Data = null; //clear file data from memory

            IsLoading = false;
        }
    }
}
