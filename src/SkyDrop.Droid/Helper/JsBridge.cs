using System;
using Android.Webkit;
using Java.Interop;

namespace SkyDrop.Droid.Helper
{
    public class JsBridge : Java.Lang.Object
    {
        private Action<string> action;

        public JsBridge(Action<string> action)
        {
            this.action = action;
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            action?.Invoke(data);
        }
    }
}

