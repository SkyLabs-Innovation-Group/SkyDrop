using System;
using System.Drawing;
using MvvmCross.Commands;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.DataViewModels
{
	public interface ISelectableItem
	{
		IMvxCommand TapCommand { get; set; }
		IMvxCommand LongPressCommand { get; set; }

		bool IsSelected { get; set; }
		bool IsSelectionActive { get; set; }

		Color FillColor { get; }
		Color SelectionIndicatorColor { get; }
	}
}

