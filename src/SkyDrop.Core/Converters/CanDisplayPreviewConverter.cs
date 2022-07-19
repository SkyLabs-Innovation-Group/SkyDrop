using System;
using System.Globalization;
using MvvmCross;
using MvvmCross.Converters;
using SkyDrop.Core.Utility;

namespace SkyDrop.Core.Converters
{
    public class CanDisplayPreviewConverter : MvxValueConverter<string, bool>
    {
        public const string Name = "CanDisplayPreview";
        public const string InvertName = "CannotDisplayPreview";

        private bool invert;

        public CanDisplayPreviewConverter(bool invert)
        {
            this.invert = invert;
        }

        protected override bool Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var canDisplay = value.CanDisplayPreview();
            return invert ? !canDisplay : canDisplay;
        }
    }
}
