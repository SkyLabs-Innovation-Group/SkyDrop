using System;
using FFImageLoading.Cross;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.Utility;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Bindings
{
    /// <summary>
    ///     Binds a SkyFile to an MvxCachedImageView for file preview
    ///     FFImageLoading handles optimising the stream, so I am generating it only right before passing it to
    ///     Target.ImageStream.
    /// </summary>
    public class LocalImagePreviewBinding : MvxTargetBinding<MvxCachedImageView, SkyFile>
    {
        public LocalImagePreviewBinding(MvxCachedImageView target) : base(target)
        {
        }

        private static ILog Log => Mvx.IoCProvider.Resolve<ILog>();

        public static string Name => "ImagePreview";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(SkyFile value)
        {
            try
            {
                Target.SetImageBitmap(null);

                if (string.IsNullOrEmpty(value?.FullFilePath))
                    return;

                if (!value.FullFilePath.CanDisplayPreview())
                    return;

                AndroidUtil.LoadLocalImagePreview(value.FullFilePath, Target);
                /*
                Task.Run(async () =>
                {
                    try
                    {
                        var task = ImageService.Instance.LoadStream(
                                c => Task.FromResult((Stream) System.IO.File.OpenRead(value.FullFilePath)))
                            .DownSampleInDip()
                            .IntoAsync(Target);

                        await task;
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Error loading image binding");
                    }
                }).Forget();*/
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}