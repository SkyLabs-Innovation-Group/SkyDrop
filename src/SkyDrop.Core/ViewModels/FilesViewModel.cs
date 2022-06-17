using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.Services;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.ViewModels.Main
{
    public enum FileLayoutType
    {
        List = 0,
        Grid = 1
    }

    public class FilesViewModel : BaseViewModel<object, SkyFile>
    {
        public MvxObservableCollection<SkyFileDVM> SkyFiles { get; } = new MvxObservableCollection<SkyFileDVM>();

        public FileLayoutType LayoutType { get; set; } = FileLayoutType.Grid;

        private readonly IApiService apiService;
        private readonly IStorageService storageService;
        private readonly IUserDialogs userDialogs;
        private readonly IMvxNavigationService navigationService;
        private readonly ILog log;

        public IMvxCommand ToggleLayoutCommand { get; set; }
        public IMvxCommand BackCommand { get; set; }

        public FilesViewModel(ISingletonService singletonService,
                             IApiService apiService,
                             IStorageService storageService,
                             IUserDialogs userDialogs,
                             IMvxNavigationService navigationService,
                             ILog log) : base(singletonService)
        {
            Title = "File Storage";

            this.apiService = apiService;
            this.storageService = storageService;
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.log = log;

            ToggleLayoutCommand = new MvxCommand(() => LayoutType = LayoutType == FileLayoutType.List ? FileLayoutType.Grid : FileLayoutType.List);
            BackCommand = new MvxAsyncCommand(async () => await navigationService.Close(this));
        }

        public override Task Initialize()
        {
            LoadSkyFiles();

            return base.Initialize();
        }

        private void LoadSkyFiles()
        {
            var newSkyFiles = GetSkyFileDVMs(storageService.LoadSkyFiles());
            SkyFiles.SwitchTo(newSkyFiles);
        }

        private List<SkyFileDVM> GetSkyFileDVMs(List<SkyFile> skyFiles)
        {
            var dvms = new List<SkyFileDVM>();
            foreach (var skyFile in skyFiles)
                dvms.Add(GetSkyFileDVM(skyFile));

            return dvms;
        }

        private SkyFileDVM GetSkyFileDVM(SkyFile skyFile)
        {
            return new SkyFileDVM
            {
                SkyFile = skyFile,
                TapCommand = new MvxAsyncCommand(() => FileTapped(skyFile)),
                LongPressCommand = new MvxCommand(() => ActivateSelectionMode(skyFile))
            };
        }

        private async Task FileTapped(SkyFile selectedFile)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);
            if (selectedFileDVM.IsSelectionActive)
            {
                ToggleFileSelected(selectedFile);
                return;
            }

            //show the file
            await navigationService.Close(this, selectedFile);
        }

        private void ToggleFileSelected(SkyFile selectedFile)
        {
            var selectedFileDVM = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == selectedFile.Skylink);
            selectedFileDVM.IsSelected = !selectedFileDVM.IsSelected;

            //if no files are selected, exit selection mode
            if (!SkyFiles.Any(a => a.IsSelected))
            {
                foreach(var skyFile in SkyFiles)
                {
                    skyFile.IsSelectionActive = false;
                }
            }
        }

        private void ActivateSelectionMode(SkyFile skyFile)
        {
            //select the skyfile that was long pressed
            var selectedSkyFile = SkyFiles.FirstOrDefault(s => s.SkyFile.Skylink == skyFile.Skylink);
            selectedSkyFile.IsSelected = true;

            //show empty selection circles for all other skyfiles
            foreach(var skyfile in SkyFiles)
            {
                skyfile.IsSelectionActive = true;
            }
        }

        public override void Prepare(object parameter)
        {
            
        }
    }
}
