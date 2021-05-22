using System.Drawing;
using Xamarin.Essentials;

namespace SkyDrop.Core.Utility
{
    public static class Colors
    {
        public static Color MidGrey => ColorConverters.FromHex("#4c555a");
        public static Color DarkGrey => ColorConverters.FromHex("#242c31");
        public static Color LightGrey => ColorConverters.FromHex("#eeeeee");

        //gradient color scheme
        public static Color Primary => ColorConverters.FromHex("#58b560"); //official skynet color
        public static Color GradientGreen => ColorConverters.FromHex("#02A275");
        public static Color GradientTurqouise => ColorConverters.FromHex("#008C80");
        public static Color GradientOcean => ColorConverters.FromHex("#00757F");
        public static Color GradientDeepBlue => ColorConverters.FromHex("#165E70");
        public static Color GradientDark => ColorConverters.FromHex("#2F4858");
        
        public static Color NewBlue => ColorConverters.FromHex("#004db6");

    }
}