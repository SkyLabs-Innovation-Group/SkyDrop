using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Realms;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.DataViewModels
{
    public class SkyFileDVM : MvxNotifyPropertyChanged
    {
        public SkyFileDVM(SkyFile skyFile, IMvxCommand tapCommand, IMvxCommand openCommand, IMvxCommand copySkyLinkCommand, IMvxCommand deleteCommand)
        {
            SkyFile = skyFile;
            TapCommand = tapCommand;
            OpenCommand = openCommand;
            CopySkyLinkCommand = copySkyLinkCommand;
            DeleteCommand = deleteCommand;
        }

        public SkyFile SkyFile { get; set; }

        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand OpenCommand { get; set; }
        public IMvxCommand CopySkyLinkCommand { get; set; }
        public IMvxCommand DeleteCommand { get; set; }

        public bool IsSelected { get; set; }
        public bool IsNew { get; set; }

        public Color FillColor => IsNew ? Colors.PrimaryDark : Colors.MidGrey;
    }
}
