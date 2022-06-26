
using Acr.UserDialogs;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Card;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.Utility;
using SkyDrop.Droid;
using System.Drawing;
using Xamarin.Essentials;
using Android.Graphics;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Net;
using System.Threading;
using FFImageLoading;
using MvvmCross;
using SkyDrop;
using SkyDrop.Core;
using SkyDrop.Core.DataModels;
using FFImageLoading.Cross;
using Serilog;
using Serilog.Core;
using File = Java.IO.File;
using Path = System.IO.Path;
using SkyDrop.Core.ViewModels.Main;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Maps a local drawable onto the layout type toggle in files view
    /// </summary>
    public class LayoutImageBinding : MvxTargetBinding<ImageView, FileLayoutType>
    {
        private ILog _log;
        private ILog log => _log ??= Mvx.IoCProvider.Resolve<ILog>();

        public static string Name => "LayoutImage";

        public LayoutImageBinding(ImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(FileLayoutType value)
        {
            Target.SetImageResource(value == FileLayoutType.List ? Resource.Drawable.ic_grid : Resource.Drawable.ic_list);
        }
    }
}