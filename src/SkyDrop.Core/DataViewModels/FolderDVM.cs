using System;
using System.Drawing;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.DataViewModels
{
	public class FolderDVM : MvxNotifyPropertyChanged, IFolderItem, ISelectableItem
	{
		public string Name => Folder.Name;

		public Folder Folder { get; set; }

		public IMvxCommand TapCommand { get; set; }
		public IMvxCommand LongPressCommand { get; set; }

		public bool IsSelected { get; set; }
		public bool IsSelectionActive { get; set; }

		public Color FillColor => Colors.Primary;
		public Color SelectionIndicatorColor => IsSelected ? FillColor : Colors.LightGrey;
    }

	public class SentFolder : IFolderItem
	{
		public string Name => "Sent Files";

		public IMvxCommand TapCommand { get; set; }
	}

	public class ReceivedFolder : IFolderItem
	{
		public string Name => "Received Files";

		public IMvxCommand TapCommand { get; set; }
	}

	public interface IFolderItem
	{
		public string Name { get; }

		public IMvxCommand TapCommand { get; set; }
	}
}

