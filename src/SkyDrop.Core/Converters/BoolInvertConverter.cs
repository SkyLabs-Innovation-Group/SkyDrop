using System;
using System.Globalization;
using MvvmCross.Converters;

namespace SkyDrop.Core.Converters
{
    public class BoolInvertConverter : MvxValueConverter<bool, bool>
    {
        public const string Name = "BoolInvert";

        private bool invert;

        protected override bool Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }
    }
}