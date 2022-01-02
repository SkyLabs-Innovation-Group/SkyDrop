using System;
using System.IO;
using Android.Content;
using Android.Views;
using SkyDrop.Core.Services;

namespace SkyDrop.Droid.Services
{
    public class ContentResolverService : IContentResolverService
    {
        public Stream GetContentStream(string contentUri)
        {
            var context = Android.App.Application.Context;
            return context.ContentResolver.OpenInputStream(Android.Net.Uri.Parse(contentUri));
        }
    }
}
