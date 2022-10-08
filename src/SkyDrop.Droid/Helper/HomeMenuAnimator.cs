using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Solver.Widgets;
using AndroidX.ConstraintLayout.Widget;
using Java.Lang;

namespace SkyDrop.Droid.Helper
{
    public class HomeMenuAnimator
    {
        private Context context;
        private ImageView homeMenuIconSkyDrive, homeMenuIconPortals, homeMenuIconContacts, homeMenuIconSettings;
        private ImageView miniMenuIconSkyDrive, miniMenuIconPortals, miniMenuIconContacts, miniMenuIconSettings;
        private FrameLayout animationContainer;
        private readonly int iconSize;

        public HomeMenuAnimator(ImageView homeMenuIconSkyDrive, ImageView homeMenuIconPortals, ImageView homeMenuIconContacts, ImageView homeMenuIconSettings,
                                ImageView miniMenuIconSkyDrive, ImageView miniMenuIconPortals, ImageView miniMenuIconContacts, ImageView miniMenuIconSettings,
                                Context context, FrameLayout animationContainer)
        {
            this.homeMenuIconSkyDrive = homeMenuIconSkyDrive;
            this.homeMenuIconPortals = homeMenuIconPortals;
            this.homeMenuIconContacts = homeMenuIconContacts;
            this.homeMenuIconSettings = homeMenuIconSettings;

            this.miniMenuIconSkyDrive = miniMenuIconSkyDrive;
            this.miniMenuIconPortals = miniMenuIconPortals;
            this.miniMenuIconContacts = miniMenuIconContacts;
            this.miniMenuIconSettings = miniMenuIconSettings;

            this.context = context;
            this.animationContainer = animationContainer;

            this.iconSize = AndroidUtil.DpToPx(32);
        }

        public async Task AnimateShrink()
        {
            var skyDriveIcon = new ImageView(context);
            var portalsIcon = new ImageView(context);
            var contactsIcon = new ImageView(context);
            var settingsIcon = new ImageView(context);

            skyDriveIcon.SetImageResource(Resource.Drawable.ic_cloud);
            portalsIcon.SetImageResource(Resource.Drawable.ic_portal);
            contactsIcon.SetImageResource(Resource.Drawable.ic_key);
            settingsIcon.SetImageResource(Resource.Drawable.ic_cog);

            AddIconsToWindow(skyDriveIcon, portalsIcon, contactsIcon, settingsIcon);

            //wait for views to layout
            await Task.Delay(100);

            var (miniMenuIconSkyDriveX, miniMenuIconSkyDriveY) = GetViewLocation(miniMenuIconSkyDrive);
            var (miniMenuIconPortalsX, miniMenuIconPortalsY) = GetViewLocation(miniMenuIconPortals);
            var (miniMenuIconContactsX, miniMenuIconContactsY) = GetViewLocation(miniMenuIconContacts);
            var (miniMenuIconSettingsX, miniMenuIconSettingsY) = GetViewLocation(miniMenuIconSettings);

            AnimateMoveToLocation(skyDriveIcon, miniMenuIconSkyDriveX, miniMenuIconSkyDriveY);
            AnimateMoveToLocation(portalsIcon, miniMenuIconPortalsX, miniMenuIconPortalsY);
            AnimateMoveToLocation(contactsIcon, miniMenuIconContactsX, miniMenuIconContactsY);
            AnimateMoveToLocation(settingsIcon, miniMenuIconSettingsX, miniMenuIconSettingsY);
        }

        public void AnimateExpand()
        {
        }

        private void AddIconsToWindow(ImageView skyDriveIcon, ImageView portalsIcon, ImageView contactsIcon, ImageView settingsIcon)
        {
            var layoutParamsSkyDrive = new FrameLayout.LayoutParams(iconSize, iconSize);
            (layoutParamsSkyDrive.LeftMargin, layoutParamsSkyDrive.TopMargin) = GetViewLocation(homeMenuIconSkyDrive);
            animationContainer.AddView(skyDriveIcon, layoutParamsSkyDrive);
            var layoutParamsPortals = new FrameLayout.LayoutParams(iconSize, iconSize);
            (layoutParamsPortals.LeftMargin, layoutParamsPortals.TopMargin) = GetViewLocation(homeMenuIconPortals);
            animationContainer.AddView(portalsIcon, layoutParamsPortals);
            var layoutParamsContacts = new FrameLayout.LayoutParams(iconSize, iconSize);
            (layoutParamsContacts.LeftMargin, layoutParamsContacts.TopMargin) = GetViewLocation(homeMenuIconContacts);
            animationContainer.AddView(contactsIcon, layoutParamsContacts);
            var layoutParamsSettings = new FrameLayout.LayoutParams(iconSize, iconSize);
            (layoutParamsSettings.LeftMargin, layoutParamsSettings.TopMargin) = GetViewLocation(homeMenuIconSettings);
            animationContainer.AddView(settingsIcon, layoutParamsSettings);
        }

        private (int X, int Y) GetViewLocation(View view)
        {
            var loc = new int[2];
            view.GetLocationInWindow(loc);

            var containerLoc = new int[2];
            animationContainer.GetLocationInWindow(containerLoc);

            var x = loc[0] - containerLoc[0];
            var y = loc[1] - containerLoc[1];

            return (x, y);
        }

        private void AnimateMoveToLocation(View view, int x, int y)
        {
            var (currentX, currentY) = GetViewLocation(view);
            view.Animate()
                .TranslationX(x - currentX)
                .TranslationY(y - currentY)
                .SetDuration(1000)
                .WithEndAction(new Runnable(() => view.Remove()))
                .Start();
        }
    }
}

