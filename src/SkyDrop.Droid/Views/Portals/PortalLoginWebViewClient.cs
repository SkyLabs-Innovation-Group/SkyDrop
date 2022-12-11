using System;
using Android.Webkit;

namespace SkyDrop.Droid.Views.Portals
{
	public class PortalLoginWebViewClient : WebViewClient
	{
        public EventHandler<bool> Navigated;

        private const string javascriptFunction = "javascript: function invokeCSharpAction(data){jsBridge.invokeAction(JSON.stringify(data));}";

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);

            view.EvaluateJavascript(javascriptFunction, null);

            Navigated?.Invoke(this, true);
        }
    }
}

