﻿using System;
using System.Drawing;
using System.Globalization;
using Acr.UserDialogs;
using CoreGraphics;
using MvvmCross.Converters;
using UIKit;

namespace SkyDrop.iOS.Converters
{
    public class NativeColorConverter : MvxValueConverter<Color, UIColor>
    {
        public const string Name = "NativeColor";

        protected override UIColor Convert(Color value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value.ToNative();
        }
    }

    public class CGColorConverter : MvxValueConverter<Color, CGColor>
    {
        public const string Name = "CGColor";

        protected override CGColor Convert(Color value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value.ToNative().CGColor;
        }
    }
}
