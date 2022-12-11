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
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using static Android.Content.Res.Resources;
using static Google.Android.Material.Tabs.TabLayout;

namespace SkyDrop.Droid.Views.Portals
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class PortalLoginView : BaseActivity<PortalLoginViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.PortalLoginView;

        private WebView webView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            InitWebView();
        }

        private void InitWebView()
        {
            Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);

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
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private class ValueCallback : Java.Lang.Object, IValueCallback
        {
            private Action<Java.Lang.Object> action;

            public ValueCallback(Action<Java.Lang.Object> action)
            {
                this.action = action;
            }

            public void OnReceiveValue(Java.Lang.Object value)
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

