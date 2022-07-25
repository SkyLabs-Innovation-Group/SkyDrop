using System;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.DataViewModels
{
	public class ContactDVM : IContactItem
	{
		public Contact Contact { get; set; }

		public IMvxCommand TapCommand { get; set; }

		public string Name => Contact.Name;

		public bool IsSelectionActive { get; set; }

		public bool IsSelected { get; set; }
	}

	public interface IContactItem
	{
		
	}

	public class AnyoneWithTheLinkItem : IContactItem
	{
		public string Name => "Anyone with the link";

		public IMvxCommand TapCommand { get; set; }
	}
}

