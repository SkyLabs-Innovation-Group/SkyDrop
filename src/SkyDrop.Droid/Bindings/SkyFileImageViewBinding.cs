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
using Java.IO;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using FFImageLoading.Cross;
using MvvmCross;
using SkyDrop;
using SkyDrop.Core;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds a SkyFile's stream to an MvxCachedImageView.
    ///
    /// FFImageLoading handles optimising the stream, so I am generating it only right before passing it to Target.ImageStream.
    /// </summary>
    public class SkyFileImageViewBinding : MvxTargetBinding<MvxCachedImageView, SkyFile>
    {
        private ILog _log;
        private ILog log => _log ??= Mvx.IoCProvider.Resolve<ILog>();
        
        public static string Name => "ImageStream";

        public SkyFileImageViewBinding(MvxCachedImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(SkyFile value)
        {
            try
            {
                if (value?.FullFilePath == null)
                {
                    Target.SetImageBitmap(null);
                    return;
                }
                
                MainThread.InvokeOnMainThreadAsync( () =>
                {
                    try
                    {
                        using var stream = value.GetStream();
                        Target.ImageStream = c => Task.FromResult(stream);
                    }
                    catch (Exception e)
                    {
                        log.Trace("Exception encountered while setting SkyFile's thumbnail in ImageStream binding");
                        log.Exception(e);
                    }
                }).Forget();
            }
            catch(Exception e)
            {
                log.Exception(e);
            }
        }
    }
}