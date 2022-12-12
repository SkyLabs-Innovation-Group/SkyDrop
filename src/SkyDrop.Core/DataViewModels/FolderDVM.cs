using System.Drawing;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.DataViewModels
{
    public class FolderDvm : MvxNotifyPropertyChanged, IFolderItem, ISelectableItem
    {
        public Folder Folder { get; set; }
        public string Name => Folder.Name;

        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand LongPressCommand { get; set; }

        public bool IsSelected { get; set; }
        public bool IsSelectionActive { get; set; }

        public Color FillColor => Colors.Primary;
        public Color SelectionIndicatorColor => IsSelected ? FillColor : Colors.LightGrey;
    }

    public class SentFolder : IFolderItem
    {
        public bool IsSelected => false;
        public bool IsSelectionActive => false;
        public string Name => "Sent Files";

        public IMvxCommand TapCommand { get; set; }
    }

    public class ReceivedFolder : IFolderItem
    {
        public bool IsSelected => false;
        public bool IsSelectionActive => false;
        public string Name => "Received Files";

        public IMvxCommand TapCommand { get; set; }
    }

    public interface IFolderItem
    {
        public string Name { get; }

        public IMvxCommand TapCommand { get; set; }
    }
}