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
using MvvmCross;
using SkyDrop;
using SkyDrop.Core;
using SkyDrop.Core.DataModels;
using FFImageLoading.Cross;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    /// Binds a SkyFile to an MvxCachedImageView for file preview
    ///
    /// FFImageLoading handles optimising the stream, so I am generating it only right before passing it to Target.ImageStream.
    /// </summary>
    public class SkyFileImageViewBinding : MvxTargetBinding<MvxCachedImageView, SkyFile>
    {
        private ILog _log;
        private ILog log => _log ??= Mvx.IoCProvider.Resolve<ILog>();
        
        public static string Name => "ImagePreview";

        public SkyFileImageViewBinding(MvxCachedImageView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(SkyFile value)
        {
            try
            {
                if (value == null)
                {
                    Target.SetImageBitmap(null);
                    return;
                }

                using (var stream = value.GetStream())
                {
                    //this line should work but doesn't
                    Target.ImageStream = async c => await Task.FromResult(stream);
                }

                /*
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
                */
            }
            catch(Exception e)
            {
                log.Exception(e);
            }
        }
    }
}