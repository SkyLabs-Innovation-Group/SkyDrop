using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace SkyDrop.Core.DataViewModels
{
    public class SkynetPortalDVM : MvxNotifyPropertyChanged
    {
        public SkynetPortalDVM(string baseUrl, string name)
        {
            this.BaseUrl = baseUrl;
            this.Name = name;
        }

        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand LongPressCommand { get; set; }
    }
}

