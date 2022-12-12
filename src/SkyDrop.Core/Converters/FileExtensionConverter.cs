using System;
using System.Globalization;
using MvvmCross.Converters;

namespace SkyDrop.Core.Converters
{
    public class FileExtensionConverter : MvxValueConverter<string, string>
    {
        public const string Name = "FileExtension";

        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var extension = value.Substring(value.LastIndexOf('.') + 1).ToUpper();
            return extension;
        }
    }
}