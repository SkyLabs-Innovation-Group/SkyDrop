using System;
using Android.Webkit;

namespace SkyDrop.Droid.Views.Portals
{
    public class PortalLoginWebViewClient : WebViewClient
    {
        private const string JavascriptFunction =
            "javascript: function invokeCSharpAction(data){jsBridge.invokeAction(JSON.stringify(data));}";

        public EventHandler<bool> Navigated;

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);

            view.EvaluateJavascript(JavascriptFunction, null);

            Navigated?.Invoke(this, true);
        }
    }
}