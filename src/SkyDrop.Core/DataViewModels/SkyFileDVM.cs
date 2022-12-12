using System.Drawing;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.DataViewModels
{
    public class SkyFileDVM : MvxNotifyPropertyChanged, ISelectableItem
    {
        public SkyFile SkyFile { get; set; }
        public IMvxCommand CopySkyLinkCommand { get; set; }
        public IMvxCommand DeleteCommand { get; set; }
        public bool IsLoading { get; set; }

        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand LongPressCommand { get; set; }

        public bool IsSelected { get; set; }
        public bool IsSelectionActive { get; set; }

        public Color FillColor => SkyFile.WasSent ? Colors.Primary : Colors.GradientOcean;
        public Color SelectionIndicatorColor => IsSelected ? FillColor : Colors.LightGrey;

        public void SetUploaded(SkyFile skyFile)
        {
            SkyFile.BitField = skyFile.BitField;
            SkyFile.Skylink = skyFile.Skylink;
            SkyFile.Merkelroot = skyFile.Merkelroot;
            SkyFile.Filename = skyFile.Filename;
            SkyFile.Status = skyFile.Status;
            SkyFile.FullFilePath = null;

            IsLoading = false;
        }
    }
}