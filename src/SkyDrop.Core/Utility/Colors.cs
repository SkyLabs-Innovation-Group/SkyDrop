using System;
using System.Drawing;
using Xamarin.Essentials;

namespace SkyDrop.Core.Utility
{
    public static class Colors
    {
        public static Color MidGrey => ColorConverters.FromHex("#4c555a");
        public static Color DarkGrey => ColorConverters.FromHex("#242c31");
        public static Color LightGrey => ColorConverters.FromHex("#eeeeee");

        public static Color Primary => ColorConverters.FromHex("#58b560"); //official skynet color
        public static Color PrimaryDark => ColorConverters.FromHex("#4d9954");
        public static Color PrimaryLight => ColorConverters.FromHex("#70d379");

        // Gradient color scheme

    }
}