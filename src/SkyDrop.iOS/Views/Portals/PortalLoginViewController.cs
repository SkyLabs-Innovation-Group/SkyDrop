using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Foundation;
using SkyDrop.Core.Components;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Common;
using UIKit;
using WebKit;

namespace SkyDrop.iOS.Views.Portals
{
	public partial class PortalLoginViewController : BaseViewController<PortalLoginViewModel>, IWKNavigationDelegate 
	{
		private bool didInitWebView;
        private WKWebView webView;

        public PortalLoginViewController() : base("PortalLoginViewController", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            WebViewContainer.BackgroundColor = Colors.DarkGrey.ToNative();

            var set = this.CreateBindingSet();
            set.Bind(this).For(w => w.WebViewHidden).To(vm => vm.IsLoggedIn);
            set.Apply();
		}

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

			InitWebView();
        }

		private async Task InitWebView()
		{
			if (didInitWebView)
				return;

			didInitWebView = true;

            webView = new WKWebView(View.Frame, new WKWebViewConfiguration { Preferences = new WKPreferences { JavaScriptEnabled = true } });
            webView.LoadRequest(new Foundation.NSUrlRequest(new NSUrl(ViewModel.PortalUrl)));
            WebViewContainer.LayoutInsideWithFrame(webView);

			webView.NavigationDelegate = this;
        }

        [Export("webView:didFinishNavigation:")]
        public async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            await Task.Delay(500);
            webView.EvaluateJavaScript(JsSnippets.CheckLoggedIn, new WKJavascriptEvaluationResult((s, e) =>
            {
                var result = s?.ToString();
                if (result == "1")
                {
                    ViewModel.IsLoggedIn = true;
                    GenerateApiKey();
                }
            }));
        }

		private async Task GenerateApiKey()
		{
            while (!ViewModel.DidSetApiKey)
            {
                await Task.Delay(5000);

                webView.EvaluateJavaScript(JsSnippets.GetApiKey, new WKJavascriptEvaluationResult((apiKey, error) =>
                {
                    if (apiKey != null && error == null)
                    {
                        ViewModel.SetApiKey(apiKey.ToString());
                    }
                    else
                    {
                        Console.WriteLine(error);
                    }
                }));
            }
        }

        public bool WebViewHidden
        {
            get => true;
            set
            {
                if (webView == null)
                    return;

                webView.Hidden = value;
            }
        }
    }
}


