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

namespace SkyDrop.Core.DataViewModels
{
    public class StagedFileDVM : MvxNotifyPropertyChanged
    {
        public StagedFileDVM(StagedFile stagedFile)
        {
            StagedFile = stagedFile;
        }

        public StagedFile StagedFile { get; set; }

        public bool IsSelected { get; set; }
        public bool IsLoading { get; set; }
    }
}
