using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;

namespace SkyDrop.Core.Utility
{
	public static class FileExplorerViewUtil
	{
        public static void ActivateSelectionMode(MvxObservableCollection<SkyFileDVM> skyFiles, SkyFile selectedFile)
        {
            //select the skyfile that was long pressed
            var selectedSkyFile = skyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);
            selectedSkyFile.IsSelected = true;

            //show empty selection circles for all other skyfiles
            foreach (var skyfile in skyFiles)
            {
                skyfile.IsSelectionActive = true;
            }
        }

        public static void ToggleFileSelected(SkyFile selectedFile, IEnumerable<SkyFileDVM> skyFiles)
        {
            var selectedFileDVM = skyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);
            selectedFileDVM.IsSelected = !selectedFileDVM.IsSelected;

            //if no files are selected, exit selection mode
            if (!skyFiles.Any(a => a.IsSelected))
            {
                foreach (var skyFile in skyFiles)
                {
                    skyFile.IsSelectionActive = false;
                }
            }
        }
    }
}

