using System.Drawing;
using Acr.UserDialogs;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Styles
{
    public static class StyleExtensions
    {
        private const int CornerRadius = 8;
        private const int BorderWidth = 2;

        public static void StyleButton(this UIView button, Color color, bool isOutline = false)
        {
            button.Layer.CornerRadius = CornerRadius;

            if (isOutline)
            {
                button.Layer.BorderWidth = BorderWidth;
                button.Layer.BorderColor = color.ToNative().CGColor;
                button.BackgroundColor = Colors.DarkGrey.ToNative();
            }
            else
            {
                button.Layer.BorderWidth = 0;
                button.BackgroundColor = color.ToNative();
            }
        }
    }
}