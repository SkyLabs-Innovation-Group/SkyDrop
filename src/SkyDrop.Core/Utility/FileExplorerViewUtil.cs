using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;

namespace SkyDrop.Core.Utility
{
	public static class FileExplorerViewUtil
	{
        public static void ActivateSelectionMode<T>(MvxObservableCollection<T> items, T selectedItem, IMvxCommand selectionStartedCommand) where T : ISelectableItem
        {
            //select the skyfile that was long pressed
            selectedItem.IsSelected = true;

            //show empty selection circles for all other skyfiles
            foreach (var skyfile in items)
            {
                skyfile.IsSelectionActive = true;
            }

            selectionStartedCommand?.Execute();
        }

        public static void ToggleItemSelected<T>(T selectedFile, IEnumerable<T> skyFiles, IMvxCommand selectionEndedCommand) where T : ISelectableItem
        {
            selectedFile.IsSelected = !selectedFile.IsSelected;

            //if no files are selected, exit selection mode
            if (!skyFiles.Any(a => a.IsSelected))
            {
                foreach (var skyFile in skyFiles)
                {
                    skyFile.IsSelectionActive = false;
                }

                selectionEndedCommand?.Execute();
            }
        }
    }
}

