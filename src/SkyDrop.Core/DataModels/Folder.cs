using System;
using System.Collections.Generic;
using MvvmCross.ViewModels;

namespace SkyDrop.Core.DataModels
{
    public class Folder : MvxNotifyPropertyChanged
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<string> SkyLinks { get; set; }
    }
}