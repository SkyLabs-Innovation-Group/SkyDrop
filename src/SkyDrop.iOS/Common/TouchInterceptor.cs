using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SkyDrop.iOS.Common
{
    public class TouchInterceptor : UIGestureRecognizer
    {
        public EventHandler<CGPoint> TouchDown;
        public EventHandler<CGPoint> TouchMove;
        public EventHandler<CGPoint> TouchUp;

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

        /// <summary>
        /// Invokes TouchesUp, so cancelled gestures are treated the same as normal gestures
        /// </summary>
        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            var touch = touches.AnyObject as UITouch;
            TouchUp?.Invoke(this, touch.LocationInView(null));
        }
    }
}