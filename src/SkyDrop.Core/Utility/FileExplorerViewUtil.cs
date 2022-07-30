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
        public static void ActivateSelectionMode(MvxObservableCollection<SkyFileDVM> skyFiles, SkyFile selectedFile, Action selectionStartedAction)
        {
            //select the skyfile that was long pressed
            SkyFileDVM selectedFileDVM;
            if(selectedFile.Skylink.IsNullOrEmpty())
                selectedFileDVM = skyFiles.FirstOrDefault(s => s.SkyFile.FullFilePath == selectedFile.FullFilePath); //unzipped file
            else
                selectedFileDVM = skyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink); //skyfile

            selectedFileDVM.IsSelected = true;

            //show empty selection circles for all other skyfiles
            foreach (var skyfile in skyFiles)
            {
                skyfile.IsSelectionActive = true;
            }

            selectionStartedAction?.Invoke();
        }

        public static void ToggleFileSelected(SkyFile selectedFile, IEnumerable<SkyFileDVM> skyFiles, Action selectionEndedAction)
        {
            //select the skyfile that was long pressed
            SkyFileDVM selectedFileDVM;
            if (selectedFile.Skylink.IsNullOrEmpty())
                selectedFileDVM = skyFiles.FirstOrDefault(s => s.SkyFile.FullFilePath == selectedFile.FullFilePath); //unzipped file
            else
                selectedFileDVM = skyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink); //skyfile

            selectedFileDVM.IsSelected = !selectedFileDVM.IsSelected;

            //if no files are selected, exit selection mode
            if (!skyFiles.Any(a => a.IsSelected))
            {
                foreach (var skyFile in skyFiles)
                {
                    skyFile.IsSelectionActive = false;
                }

                selectionEndedAction?.Invoke();
            }
        }
    }
}

