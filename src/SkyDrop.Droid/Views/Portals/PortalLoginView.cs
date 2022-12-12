using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Newtonsoft.Json;
using SkyDrop.Core.Components;
using SkyDrop.Core.ViewModels;
using SkyDrop.Droid.Helper;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace SkyDrop.Droid.Views.Portals
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class PortalLoginView : BaseActivity<PortalLoginViewModel>
    {
        private WebView webView;
        protected override int ActivityLayoutId => Resource.Layout.PortalLoginView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            InitWebView();
        }

        private void InitWebView()
        {
            WebView.SetWebContentsDebuggingEnabled(true);

            webView = new WebView(this);
            webView.Settings.JavaScriptEnabled = true;

            var webViewClient = new PortalLoginWebViewClient();
            webViewClient.Navigated += async (s, e) =>
            {
                await Task.Delay(500);
                webView.EvaluateJavascript(JsSnippets.CheckLoggedIn, new ValueCallback(s =>
                {
                    var result = s?.ToString();
                    if (result == "true")
                    {
                        ViewModel.IsLoggedIn = true;
                        GenerateApiKey();
                    }
                }));
            };
            webView.SetWebViewClient(webViewClient);
            webView.AddJavascriptInterface(new JsBridge(HandleJsCalls), "jsBridge");

            var webViewContainer = FindViewById<FrameLayout>(Resource.Id.WebViewContainer);
            webViewContainer.AddView(webView);

            webView.LoadUrl(ViewModel.PortalUrl);
        }

        private async Task GenerateApiKey()
        {
            while (!ViewModel.DidSetApiKey)
            {
                webView.EvaluateJavascript(JsSnippets.GetApiKey, null);

                await Task.Delay(3000);
            }
        }

        //this method gets called from JS
        private void HandleJsCalls(string data)
        {
            try
            {
                var dataObject = JsonConvert.DeserializeObject<JsArgs>(data);
                if (dataObject == null)
                    throw new Exception("dataObject was null");

                ViewModel.SetApiKey(dataObject.ApiKey);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private class ValueCallback : Object, IValueCallback
        {
            private readonly System.Action<Object> action;

            public ValueCallback(System.Action<Object> action)
            {
                this.action = action;
            }

            public void OnReceiveValue(Object value)
            {
                action(value);
            }
        }

        public class JsArgs
        {
            public string ApiKey { get; set; }
        }
    }
}