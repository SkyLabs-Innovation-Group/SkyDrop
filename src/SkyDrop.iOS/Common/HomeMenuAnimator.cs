using System;
using System.Threading.Tasks;
using CoreGraphics;
using UIKit;

namespace SkyDrop.iOS.Common
{
    public class HomeMenuAnimator
    {
        private readonly UIView animationContainer;
        private readonly UIImageView homeMenuButtonContacts;
        private readonly UIImageView homeMenuButtonPortals;
        private readonly UIImageView homeMenuButtonSettings;
        private readonly UIImageView homeMenuButtonSkyDrive;
        private readonly nfloat homeMenuIconSize;
        private readonly UIImageView miniMenuButtonContacts;
        private readonly UIImageView miniMenuButtonPortals;
        private readonly UIImageView miniMenuButtonSettings;
        private readonly UIImageView miniMenuButtonSkyDrive;
        private readonly nfloat miniMenuIconSize;

        public HomeMenuAnimator(UIImageView homeMenuButtonSkyDrive, UIImageView homeMenuButtonPortals,
            UIImageView homeMenuButtonContacts, UIImageView homeMenuButtonSettings,
            UIImageView miniMenuButtonSkyDrive, UIImageView miniMenuButtonPortals, UIImageView miniMenuButtonContacts,
            UIImageView miniMenuButtonSettings,
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

            homeMenuIconSize = homeMenuButtonSkyDrive.Frame.Width;
            miniMenuIconSize = miniMenuButtonSkyDrive.Frame.Width;
        }

        public async Task AnimateShrink(float delay, float duration)
        {
            var skyDriveIcon = new UIImageView(UIImage.FromBundle("ic_cloud"))
                { TintColor = miniMenuButtonSkyDrive.TintColor };
            var portalsIcon = new UIImageView(UIImage.FromBundle("ic_portals"))
                { TintColor = miniMenuButtonPortals.TintColor };
            var contactsIcon = new UIImageView(UIImage.FromBundle("ic_key"))
                { TintColor = miniMenuButtonContacts.TintColor };
            var settingsIcon = new UIImageView(UIImage.FromBundle("ic_settings"))
                { TintColor = miniMenuButtonSettings.TintColor };

            AddIconsToWindow(skyDriveIcon, portalsIcon, contactsIcon, settingsIcon);

            await Task.Delay((int)(delay * 1000));

            var miniMenuButtonSkyDrivePosition = GetViewLocation(miniMenuButtonSkyDrive);
            var miniMenuButtonPortalsPosition = GetViewLocation(miniMenuButtonPortals);
            var miniMenuButtonContactsPosition = GetViewLocation(miniMenuButtonContacts);
            var miniMenuButtonSettingsPosition = GetViewLocation(miniMenuButtonSettings);

            AnimateMoveToLocation(skyDriveIcon, miniMenuButtonSkyDrivePosition, duration);
            AnimateMoveToLocation(portalsIcon, miniMenuButtonPortalsPosition, duration);
            AnimateMoveToLocation(contactsIcon, miniMenuButtonContactsPosition, duration);
            AnimateMoveToLocation(settingsIcon, miniMenuButtonSettingsPosition, duration);
        }

        private void AddIconsToWindow(UIImageView skyDriveIcon, UIImageView portalsIcon, UIImageView contactsIcon,
            UIImageView settingsIcon)
        {
            var skyDriveLocation = GetViewLocation(homeMenuButtonSkyDrive);
            var portalsLocation = GetViewLocation(homeMenuButtonPortals);
            var contactsLocation = GetViewLocation(homeMenuButtonContacts);
            var settingsLocation = GetViewLocation(homeMenuButtonSettings);

            animationContainer.AddSubview(skyDriveIcon);
            animationContainer.AddSubview(portalsIcon);
            animationContainer.AddSubview(contactsIcon);
            animationContainer.AddSubview(settingsIcon);

            skyDriveIcon.Frame = GetFrameForLocation(skyDriveLocation);
            portalsIcon.Frame = GetFrameForLocation(portalsLocation);
            contactsIcon.Frame = GetFrameForLocation(contactsLocation);
            settingsIcon.Frame = GetFrameForLocation(settingsLocation);
        }

        private CGPoint GetViewLocation(UIView view)
        {
            var point = view.Superview.ConvertPointToView(view.Frame.Location, null);

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
            var animator = new UIViewPropertyAnimator(duration, UIViewAnimationCurve.EaseInOut, () =>
            {
                view.Transform = CGAffineTransform.MakeTranslation(x - currentX, y - currentY);
                ResizeFrame(view);
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
            return new CGRect(location.X - homeMenuIconSize / 2, location.Y - homeMenuIconSize / 2, homeMenuIconSize,
                homeMenuIconSize);
        }

        private void ResizeFrame(UIView view)
        {
            //compensate for position offset caused by frame size changing
            var dif = (miniMenuIconSize - homeMenuIconSize) * 0.5;
            view.Frame = new CGRect(view.Frame.X - dif, view.Frame.Y - dif, miniMenuIconSize, miniMenuIconSize);
        }
    }
}