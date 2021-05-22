using System.Drawing;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
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

        public Color FillColor => SkyFile.Status == FileStatus.Uploaded ? Colors.GradientGreen : Colors.MidGrey;

        public void SetUploaded(SkyFile skyFile)
        {
            this.SkyFile.BitField = skyFile.BitField;
            this.SkyFile.Skylink = skyFile.Skylink;
            this.SkyFile.Merkelroot = skyFile.Merkelroot;
            this.SkyFile.Filename = skyFile.Filename;
            this.SkyFile.Status = skyFile.Status;
            this.SkyFile.FullFilePath = null; 

            IsLoading = false;
        }
    }
}
