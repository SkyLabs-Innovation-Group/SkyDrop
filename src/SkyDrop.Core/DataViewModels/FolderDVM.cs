using System;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.DataViewModels
{
	public class FolderDVM : IFolderItem
	{
		public string Name => Folder.Name;

		public Folder Folder { get; set; }

		public IMvxCommand TapCommand { get; set; }
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

