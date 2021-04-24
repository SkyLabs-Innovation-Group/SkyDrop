using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SkyDrop.iOS.Common
{
    public class TouchInterceptor : UIGestureRecognizer
    {
        public EventHandler<CGPoint> TouchDown;
        public EventHandler<CGPoint> TouchUp;
        public EventHandler<CGPoint> TouchMove;

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var touch = touches.AnyObject as UITouch;
            TouchDown?.Invoke(this, touch.LocationInView(null));
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            var touch = touches.AnyObject as UITouch;
            TouchUp?.Invoke(this, touch.LocationInView(null));
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            var touch = touches.AnyObject as UITouch;
            TouchMove?.Invoke(this, touch.LocationInView(null));
        }
    }
}
