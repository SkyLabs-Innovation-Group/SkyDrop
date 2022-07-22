using System;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.DataViewModels
{
	public class ContactDVM
	{
		public Contact Contact { get; set; }

		public IMvxCommand TapCommand { get; set; }
	}
}

