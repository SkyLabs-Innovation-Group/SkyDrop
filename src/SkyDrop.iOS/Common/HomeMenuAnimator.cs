using System;
using System.Threading.Tasks;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using UIKit;

namespace SkyDrop.iOS.Common
{
    public class HomeMenuAnimator
    {
        private UIView homeMenuButtonSkyDrive, homeMenuButtonPortals, homeMenuButtonContacts, homeMenuButtonSettings;
        private UIView miniMenuButtonSkyDrive, miniMenuButtonPortals, miniMenuButtonContacts, miniMenuButtonSettings;
        private UIView animationContainer;
        private readonly int iconSize;

        public HomeMenuAnimator(UIView homeMenuButtonSkyDrive, UIView homeMenuButtonPortals, UIView homeMenuButtonContacts, UIView homeMenuButtonSettings,
                                UIView miniMenuButtonSkyDrive, UIView miniMenuButtonPortals, UIView miniMenuButtonContacts, UIView miniMenuButtonSettings,
                                UIView animationContainer)
        {
            this.homeMenuButtonSkyDrive = homeMenuButtonSkyDrive;
            this.homeMenuButtonPortals = homeMenuButtonPortals;
            this.homeMenuButtonContacts = homeMenuButtonContacts;
            this.homeMenuButtonSettings = homeMenuButtonSettings;

            this.miniMenuButtonSkyDrive = miniMenuButtonSkyDrive;
            this.miniMenuButtonPortals = miniMenuButtonPortals;
            this.miniMenuButtonContacts = miniMenuButtonContacts;
            this.miniMenuButtonSettings = miniMenuButtonSettings;

            this.animationContainer = animationContainer;

            this.iconSize = 36;
        }

        public async Task AnimateShrink(float delay, float duration)
        {
            var skyDriveIcon = new UIImageView(UIImage.FromBundle("ic_cloud"));
            var portalsIcon = new UIImageView(UIImage.FromBundle("ic_portal"));
            var contactsIcon = new UIImageView(UIImage.FromBundle("ic_key"));
            var settingsIcon = new UIImageView(UIImage.FromBundle("ic_cog"));

            await Task.Delay((int)delay);

            AddIconsToWindow(skyDriveIcon, portalsIcon, contactsIcon, settingsIcon);

            //wait for views to layout
            await Task.Delay(10);

            var miniMenuButtonSkyDrivePosition = GetViewLocation(miniMenuButtonSkyDrive);
            var miniMenuButtonPortalsPosition = GetViewLocation(miniMenuButtonPortals);
            var miniMenuButtonContactsPosition = GetViewLocation(miniMenuButtonContacts);
            var miniMenuButtonSettingsPosition = GetViewLocation(miniMenuButtonSettings);

            AnimateMoveToLocation(skyDriveIcon, miniMenuButtonSkyDrivePosition, duration);
            AnimateMoveToLocation(portalsIcon, miniMenuButtonPortalsPosition, duration);
            AnimateMoveToLocation(contactsIcon, miniMenuButtonContactsPosition, duration);
            AnimateMoveToLocation(settingsIcon, miniMenuButtonSettingsPosition, duration);
        }

        private void AddIconsToWindow(UIImageView skyDriveIcon, UIImageView portalsIcon, UIImageView contactsIcon, UIImageView settingsIcon)
        {
            var skyDriveLocation = GetViewLocation(homeMenuButtonSkyDrive, false);
            var portalsLocation = GetViewLocation(homeMenuButtonPortals, false);
            var contactsLocation = GetViewLocation(homeMenuButtonContacts, false);
            var settingsLocation = GetViewLocation(homeMenuButtonSettings, false);

            animationContainer.AddSubview(skyDriveIcon);
            animationContainer.AddSubview(portalsIcon);
            animationContainer.AddSubview(contactsIcon);
            animationContainer.AddSubview(settingsIcon);

            skyDriveIcon.Frame = GetFrameForLocation(skyDriveLocation);
            portalsIcon.Frame = GetFrameForLocation(portalsLocation);
            contactsIcon.Frame = GetFrameForLocation(contactsLocation);
            settingsIcon.Frame = GetFrameForLocation(settingsLocation);
        }

        private CGPoint GetViewLocation(UIView view, bool scale = true)
        {
            var screenDensity = UIScreen.MainScreen.Scale;
            var point = view.ConvertPointToView(view.Frame.Location, animationContainer);

            if (scale)
            {
                point.X /= screenDensity;
                point.Y /= screenDensity;
            }

            //measure from the center of the view
            point.X += view.Frame.Width / 2;
            point.Y += view.Frame.Height / 2;
            return point;
        }

        private void AnimateMoveToLocation(UIView view, CGPoint location, float duration)
        {
            var x = location.X;
            var y = location.Y;
            var (currentX, currentY) = GetViewLocation(view);
            var animator = new UIViewPropertyAnimator(2f, UIViewAnimationCurve.EaseInOut, () =>
            {
                view.Transform = CGAffineTransform.MakeTranslation(x - currentX, y - currentY);
            });
            animator.AddCompletion(async p =>
            {
                if (p == UIViewAnimatingPosition.End)
                {
                    //wait for the icon behind to finish fading in 
                    await Task.Delay(500);
                    view.RemoveFromSuperview();
                }
            });
            animator.StartAnimation();
        }

        private CGRect GetFrameForLocation(CGPoint location)
        {
            return new CGRect(location, new CGSize(iconSize, iconSize));
        }
    }
}

