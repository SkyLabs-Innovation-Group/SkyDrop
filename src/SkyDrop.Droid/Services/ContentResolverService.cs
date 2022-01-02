using System;
using System.IO;
using Acr.UserDialogs;
using Android.Content;
using Android.Views;
using MvvmCross;
using SkyDrop.Core.Services;

namespace SkyDrop.Droid.Services
{
    public class ContentResolverService : IContentResolverService
    {
        public Stream GetContentStream(string contentUri)
        {
            try
            {
                var context = Android.App.Application.Context;
                return context.ContentResolver.OpenInputStream(Android.Net.Uri.Parse(contentUri));
            }
            catch(Exception e)
            {
                var userDialogs = Mvx.IoCProvider.Resolve<IUserDialogs>();
                userDialogs.Alert(e.Message);
                return null;
            }
        }
    }
}
