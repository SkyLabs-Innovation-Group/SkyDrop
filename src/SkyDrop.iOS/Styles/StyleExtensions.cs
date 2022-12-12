using System.Drawing;
using Acr.UserDialogs;
using SkyDrop.Core.Utility;
using UIKit;

namespace SkyDrop.iOS.Styles
{
    public static class StyleExtensions
    {
        private const int cornerRadius = 8;
        private const int borderWidth = 2;

        public static void StyleButton(this UIView button, Color color, bool isOutline = false)
        {
            button.Layer.CornerRadius = cornerRadius;

            if (isOutline)
            {
                button.Layer.BorderWidth = borderWidth;
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