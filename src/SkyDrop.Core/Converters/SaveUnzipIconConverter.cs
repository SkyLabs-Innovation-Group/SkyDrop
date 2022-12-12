using System;
using System.Globalization;
using MvvmCross.Converters;

namespace SkyDrop.Core.Converters
{
    public class SaveUnzipIconConverter : MvxValueConverter<bool, string>
    {
        public const string Name = "SaveUnzipIcon";

        protected override string Convert(bool isFocusedFileAnArchive, Type targetType, object parameter,
            CultureInfo culture)
        {
            return isFocusedFileAnArchive ? "res:ic_zip" : "res:ic_download";
        }
    }
}