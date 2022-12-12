using System.Drawing;
using Acr.UserDialogs;
using Google.Android.Material.Card;
using SkyDrop.Core.Utility;
using static SkyDrop.Droid.Helper.AndroidUtil;

namespace SkyDrop.Droid.Styles
{
    public static class StyleExtensions
    {
        private const int CornerRadius = 8;
        private const int BorderWidth = 2;

        public static void StyleButton(this MaterialCardView button, Color color, bool isOutline = false)
        {
            button.Radius = DpToPx(CornerRadius);

            if (isOutline)
            {
                button.StrokeWidth = DpToPx(BorderWidth);
                button.StrokeColor = color.ToNative();
                button.SetCardBackgroundColor(Colors.DarkGrey.ToNative());
            }
            else
            {
                button.StrokeWidth = 0;
                button.SetCardBackgroundColor(color.ToNative());
            }

            //fixes a bug where the button height increases when changing StrokeWidth
            button.SetContentPadding(0, 0, 0, 0);
        }
    }
}