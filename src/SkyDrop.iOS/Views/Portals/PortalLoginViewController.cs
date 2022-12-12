using System.Threading.Tasks;
using Acr.UserDialogs;
using Foundation;
using Newtonsoft.Json;
using SkyDrop.Core.Components;
using SkyDrop.Core.Utility;
using SkyDrop.Core.ViewModels;
using SkyDrop.iOS.Common;
using WebKit;

namespace SkyDrop.iOS.Views.Portals
{
    public partial class PortalLoginViewController : BaseViewController<PortalLoginViewModel>, IWKNavigationDelegate,
        IWKScriptMessageHandler
    {
        private const string JavascriptBridgeFunction =
            "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(JSON.stringify(data));}";

        private bool didInitWebView;
        private WKWebView webView;

        public PortalLoginViewController() : base("PortalLoginViewController", null)
        {
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

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            var messageJson = message.Body.ToString();
            var messageObj = JsonConvert.DeserializeObject<JsArgs>(messageJson);

            ViewModel.SetApiKey(messageObj.ApiKey);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddBackButton(() => ViewModel.BackCommand.Execute());

            WebViewContainer.BackgroundColor = Colors.DarkGrey.ToNative();

            var set = CreateBindingSet();
            set.Bind(this).For(w => w.WebViewHidden).To(vm => vm.IsLoggedIn);
            set.Bind(this).For(t => t.Title).To(vm => vm.Title);
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

            webView = new WKWebView(View.Frame,
                new WKWebViewConfiguration { Preferences = new WKPreferences { JavaScriptEnabled = true } });
            webView.LoadRequest(new NSUrlRequest(new NSUrl(ViewModel.PortalUrl)));
            WebViewContainer.LayoutInsideWithFrame(webView);
            webView.NavigationDelegate = this;

            var userController = webView.Configuration.UserContentController;
            var script = new WKUserScript(new NSString(JavascriptBridgeFunction),
                WKUserScriptInjectionTime.AtDocumentEnd, false);
            userController.AddUserScript(script);
            userController.AddScriptMessageHandler(this, "invokeAction");
        }

        [Export("webView:didFinishNavigation:")]
        public async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            await Task.Delay(500);
            webView.EvaluateJavaScript(JsSnippets.CheckLoggedIn, (s, e) =>
            {
                var result = s?.ToString();
                if (result == "1")
                {
                    ViewModel.IsLoggedIn = true;
                    GenerateApiKey();
                }
            });
        }

        private async Task GenerateApiKey()
        {
            while (!ViewModel.DidSetApiKey)
            {
                webView.EvaluateJavaScript(JsSnippets.GetApiKey, null);

                await Task.Delay(3000);
            }
        }

        public class JsArgs
        {
            public string ApiKey { get; set; }
        }
    }
}