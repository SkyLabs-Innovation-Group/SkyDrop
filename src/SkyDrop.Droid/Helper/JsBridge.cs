using System;
using Android.Webkit;
using Java.Interop;
using Object = Java.Lang.Object;

namespace SkyDrop.Droid.Helper
{
    public class JsBridge : Object
    {
        private readonly Action<string> action;

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