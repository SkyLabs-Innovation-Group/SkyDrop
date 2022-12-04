using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Foundation;
using Newtonsoft.Json;
using SkyDrop.Core.Components;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Common;
using UIKit;
using WebKit;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SkyDrop.iOS.Views.Portals
{
	public partial class PortalLoginViewController : BaseViewController<PortalLoginViewModel>, IWKNavigationDelegate, IWKScriptMessageHandler
	{
		private bool didInitWebView;
        private WKWebView webView;

        private const string javascriptBridgeFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(JSON.stringify(data));}";

        public PortalLoginViewController() : base("PortalLoginViewController", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            AddBackButton(() => ViewModel.BackCommand.Execute());

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

            var userController = webView.Configuration.UserContentController;
            var script = new WKUserScript(new NSString(javascriptBridgeFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
            userController.AddUserScript(script);
            userController.AddScriptMessageHandler(this, "invokeAction");
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
                webView.EvaluateJavaScript(JsSnippets.GetApiKey, null);

                await Task.Delay(5000);
            }
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var messageJson = message.Body.ToString();
            var messageObj = JsonConvert.DeserializeObject<JsArgs>(messageJson);

            ViewModel.SetApiKey(messageObj.ApiKey);
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

        public class JsArgs
        {
            public string ApiKey { get; set; }
        }
    }
}


