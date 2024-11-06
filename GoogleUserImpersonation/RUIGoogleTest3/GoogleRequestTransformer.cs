using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Pet.RUIGoogle.GoogleConstants;
using System.Net.Http;
using System.Web;
using System.Collections.Specialized;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net;

namespace Pet.RUIGoogle
{
    public static class GoogleConstants
    {
        [AttributeUsageAttribute(AttributeTargets.All)]
        internal class DeserializeAsAttribute : Attribute
        {
            // Provides name of the member
            private string name;

            // Constructor

            public DeserializeAsAttribute(string Name)
            {
                name = Name;
            }

            // property to get name
            public string Name
            {
                get { return name; }
            }
        }
        #region Constants
        public static class AuthAdvice
        {
            public static string external_browser => "true";
            public static string report_user_id => "true";
            public static string app_version => "4.2021.22202";
            public static string[] user_id => new string[] { };

            public static string safari_authentication_session => "true";
            public static string[] supported_service => new string[] { "uca" };
            public static string request_trigger => "ADD_ACCOUNT";
            //public static string ESdevice_model = "iPad4%2C4";
            public static string lib_ver => "3.3";
            public static string package_name => "com.google.Drive";
            /* 
             * EVIL :P
             * MODIFIED REDIRECT URL
             */
            public static string redirect_uri => "com.google.sso.640853332981:/authCallback";
            //public static string redirect_uri => "com.google.sso.petterpet.tk:433/authCallback";
            public static string client_id => "640853332981.apps.googleusercontent.com";
            public static string mediator_client_id => "936475272427.apps.googleusercontent.com";
            public static string hl => "en-US";
        }
        public static class EmbeddedSetup
        {
            public static string scope => "https://www.google.com/accounts/OAuthLogin https://www.googleapis.com/auth/userinfo.email";
            public static string hl => "en-US";
            public static string app_version { get; set; }
            public static string kdlc => "1";
            public static string kss => "uca";
            public static string lib_ver => "3.3";
            public static string code_challenge_method => "S256";
            public static string sfas => "1";
            public static string flowName => "GlifSetupSafariVC";
            public static string request_trigger => "ADD_ACCOUNT";
            //public static string ESdevice_model = "iPad4%2C4";
            /* 
             * EVIL :P
             * MODIFIED REDIRECT URL
             */
            public static string redirect_uri => "com.google.sso.640853332981:/authCallback?login=code";
            //public static string redirect_uri => "com.google.sso.petterpet.tk:433/authCallback?login=code";
            public static string sarp => "1";
            public static string client_id { get; set; }
            public static string delegated_client_id { get; set; }
        }
        public static class DeviceInfo
        {
            public static string _0 => null;
            public static string _1 => null;
            public static string _2 => null;
            public static List<string> _3 => new List<string>();
            public static string _4 => null;
            public static string _6 => null;
            public static string _7 => null;
            public static string _8 => null;
            public static string _9 => "GlifSetupSafariVC";
            public static string _10 => null;
            public static string _12 => null;
            public static string _13 => null;
            public static bool _14 = true;
            public static string _15 => null;
            public static int _16 => 0;
            public static string _17 => null;
            public static bool _18 = false;
            public static int _19 => 2;
            public static string _20 => "";
        }
        public static class DeviceInfo11
        {
            public static int _0 => 1;
            //public static string _1;
            public static string[] _2 = new string[] {"https://www.google.com/accounts/OAuthLogin", "https://www.googleapis.com/auth/userinfo.email" };
            public static object _3 => null;
            //public static string _4;
            public static object _5 => null;
            //public static string _6;
            //public static string _7;
            //public static string _8;
            //public static string _9;
            public static object _10 => null;
            public static object _11 => null;
            public static object _12 => null;
            public static object _13 => null;
            //public static string _14;
            public static object _15 => null;
            public static object _16 => null;
            public static object _17 => null;
            public static object _18 => null;
            //public static string _19;
            public static object _20 => null;
            public static object _21 => null;
            public static int _22 => 1;
            public static object _23 => null;
            public static object _24 => null;
            public static string[] _25 => new string[] { };
            public static object _26 => null;
            public static object _27 => null;
            public static object _28 => null;
            public static object _29 => null;
            public static string[] _30 => new string[] { };
        }
        public static class FReq
        {
            //public static string _0 { get; set; }
            //public static string _1 { get; set; }
            public static string[] _2 => new string[] { };
            public static object _3 => null;
            //public static string _4 { get; set; }
            public static string _5 => null;
            public static string _6 => null;
            public static int _7 => 2;
            public static bool _8 => false;
            public static bool _9 => true;
            //public static FReq10 _10 { get; set; }
            //public static string _11 { get; set; }
            public static object _12 => null;
            public static object _13 => null;
            public static object _14 => null;
            public static bool _15 => true;
            public static bool _16 => true;
            public static string[] _17 => new string[] { };
        }
        public static class FReq10
        {
            public static object _0 => null;
            public static object _1 => null;
            //public static FReq10_2 _2 { get; set; }
            public static int _3 => 1;
            //public static FReq10_4 _4;
            public static object _5 => null;
            public static object _6 => null;
            public static object _7 => null;
            public static bool _8 => false;
            public static object _9 => null;
            public static object _10 => null;
            public static object _11 => null;
            public static object _12 => null;
            public static object _13 => null;
            public static object _14 => null;
            public static object _15 => null;
            public static object _16 => null;
            //public static FReq10_17 _17 { get; set; }
        }
        public static class FReq10_2
        {
            public static int _0 => 1;
            public static int _1 => 1;
            public static int _2 => 1;
            public static int _3 => 1;
            //Referer (embedded's request uri)
            //public static string _4 { get; set; }
            public static object _5 => null;
            public static string[] _6 => new string[] { };
            public static int _7 => 4;
            public static string[] _8 => new string[] { };
            public static string _9 => "GlifSetupSafariVC";
            public static object _10 => null;
            public static string[] _11 => new string[] { };
        }
        public static class FReq10_4
        {
            public static int _0 => 1;
            //public static string _1 { get; set; }
            public static string[] _2 => new string[] { "https://www.google.com/accounts/OAuthLogin"
                ,"https://www.googleapis.com/auth/userinfo.email" };
            public static object _3 => null;
            //public static string _4 { get; set; }
            public static object _5 => null;
            //public static string _6 { get; set; }
            //public static string _7 { get; set; }
            //public static string _8 { get; set; }
            //public static string _9 { get; set; }
            public static object _10 => null;
            public static object _11 => null;
            public static object _12 => null;
            public static object _13 => null;
            //public static string _14 { get; set; }
            public static object _15 => null;
            public static object _16 => null;
            public static object _17 => null;
            public static object _18 => null;
            //public static string _19 { get; set; }
            public static object _20 => null;
            public static object _21 => null;
            public static int _22 => 1;
            public static object _23 => null;
            public static object _24 => null;
            public static string[] _25 => new string[] { };
            public static object _26 => null;
            public static object _27 => null;
            public static object _28 => null;
            public static object _29 => null;
            public static string[] _30 => new string[] { };
        }
        public static class FReq10_17
        {
            public static object _0 => null;
            public static bool _1 => true;
            public static object _2 => null;
            public static object _3 => null;
            public static object _4 => null;
            public static object _5 => null;
            public static object _6 => null;
            public static object _7 => null;
            public static object _8 => null;
            //public static string _9 { get; set; }
            public static bool _10 => true;
        }
        public static class BgRequest
        {
            public static string _0 => "identifier";
            //bgRequest
            //public static string _1 { get; set; }
        }
        public static class AccountLookup
        {
            public static string scope => "https://www.google.com/accounts/OAuthLogin https://www.googleapis.com/auth/userinfo.email";
            //public static string client_id { get; set; };
            public static string hl => "en-US";
            public static string sarp => "1";
            //public static string app_version => "4.2021.22202";
            //public static string @continue { get; set; }
            [DeserializeAs("f.req")]
            //public static string fReq { get; set; }
            //public static string bgRequest { get; set; }
            //public static string at { get; set; }
            //public static string azt { get; set; }
            public static bool cookiesDisabled => false;
            public static string igresponse => @"["""",2,null,null,null,null,1]";
            //public static string device_info { get; set; }
            public static string gmscoreversion => "undefined";
            public static string checkConnection => "youtube:526:0";
            public static string checkedDomains => "youtube&";
        }

        public static class ProgrammaticAuth
        {
            //public static string @as { get; set; }
            public static string scope => "https://www.google.com/accounts/OAuthLogin https://www.googleapis.com/auth/userinfo.email";
            public static string client_id { get; set; }
            /* 
             * EVIL :P
             * MODIFIED REDIRECT URL
             */
            public static string redirect_uri => "com.google.sso.640853332981:/authCallback?login=code";
            //public static string redirect_uri => "com.google.sso.petterpet.tk:433/authCallback?login=code";
            public static string sarp => "1";
            public static string delegated_client_id { get; set; }
            public static string hl1 => "en-US";
            public static string hl2 => "en";
            //public static string device_name { get; set; }
            //public static string auth_extension { get; set; }
            //public static string system_version { get; set; }
            //public static string app_version { get; set; }
            public static string kdlc => "1";
            public static string kss => "uca";
            public static string lib_ver => "3.3";
            //public static string device_model { get; set; }
            public static string code_challenge_method => "S256";
            //public static string code_challenge { get; set; }
            public static string sfas => "1";
            public static string flowName => "GlifSetupSafariVC";
            public static string cid => "1";
            public static string navigationDirection => "forward";
            //public static string TL { get; set; }

            //public static string part { get; set; }
            //public static string delegated_client_id { get; set; }
            //public static string hl => "en";
            public static string authuser => "0";

        }
        #endregion

        public static readonly string ifkvXpath = ".//div[@class='XMWAS']//form//input[@name='ifkv']";
        public static readonly string TLXpath = ".//div[@class='XMWAS']//form//input[@name='TL']";
        public static readonly string gxfXpath = ".//div[@class='XMWAS']//form//input[@name='gxf']";

        public static readonly string[,] authAdviceTemplate = new string[,]
        {
            {"Content-Type", "application/json"},
{"Connection", "keep-alive"},
{"Accept", "*/*"},
{"User-Agent", "com.google.Drive/4.2021.22202 iSL/3.3 iPad/12.4.7 hw/iPad4_4 (gzip)"},
{"Content-Length", ""},
{"Accept-Language", "en-us"},
{"Accept-Encoding", "br, gzip, deflate"}
        };

        public static readonly string[,] embeddedSetupTemplate = new string[,]
        {
            {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"},
{"User-Agent", "Mozilla/5.0 (iPad; CPU OS 12_4_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1.2 Mobile/15E148 Safari/604.1"},
{"Accept-Language", "en-us"},
{"Accept-Encoding", "br, gzip, deflate"},
{"Connection", "keep-alive"},
        };

        public static readonly string[,] accountlookupTemplate = new string[,]
        {
            {"Accept", "*/*"},
{"Google-Accounts-XSRF", "1"},
{"Accept-Language", "en-us"},
{"Accept-Encoding", "br, gzip, deflate"},
{"X-Same-Domain", "1"},
{"Content-Type", "application/x-www-form-urlencoded;charset=UTF-8"},
{"Origin", "https://accounts.google.com"},
{"User-Agent", "Mozilla/5.0 (iPad; CPU OS 12_4_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1.2 Mobile/15E148 Safari/604.1"},
{"Referer", ""},
{"Content-Length", ""},
{"Connection", "keep-alive"}
        };

        public static readonly string[,] lookupHeaderTemplate = {
            { "Connection", "keep-alive" },
            {"Content-Length", ""},
{"Cache-Control", "max-age=0"},
{"Upgrade-Insecure-Requests", "1"},
{"Origin", "https://accounts.google.com"},
{"Content-Type", "application/x-www-form-urlencoded"},
{"User-Agent", "Chrome"},
{"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
{"Accept-Language", "en-us"},
{"Sec-Fetch-Site", "same-origin"},
{"Sec-Fetch-Mode", "navigate"},
{"Sec-Fetch-Dest", "document"},
{"Referer", "https://accounts.google.com/ServiceLogin?passive=1209600&continue=https%3A%2F%2Faccounts.google.com%2F&followup=https%3A%2F%2Faccounts.google.com%2F"},
{"Accept-Encoding", "gzip, deflate, br"}
 };
        public static readonly string[,] POSTlogTemplate =
        {
            {"P3P", "CP=\"This is not a P3P policy! See g.co/p3phelp for more info.\""},
{"Access-Control-Allow-Origin", "https://accounts.google.com"},
{"Cross-Origin-Resource-Policy", "cross-origin"},
{"Access-Control-Allow-Credentials", "true"},
{"Access-Control-Allow-Headers", "X-Playlog-Web"},
{"Content-Type", "text/plain; charset=UTF-8"},
{"Date", ""},
{"Server", "Playlog"},
{"Cache-Control", "private"},
{"X-XSS-Protection", "0"},
{"X-Frame-Options", "SAMEORIGIN"},
//{"Set-Cookie", "NID=511=ptdu1ZxKH4tasp1XqajhkBc6-C7PV_TtNvJnhds-LI19An8ISIqsmzMrH5aD_trSLQYp0JRAprMuWJNNr_uKxtDySckk6DkYlnSv0tUTkd43oWSbR58sFRwpUa7qQZMYk3VIdSfPMbJh-h-Z5UVyHgTN8IgUfmwfEGrRmfIjfgdN7-imOB4i2wjaOLZP8jhO6m3qgU6vyljTqPUn1ozFiblQ2sI0FgBXBEgCY9Tpam4ypmmlLSSMdlYqR6cZYsArrGz4nkCCONw5JsPkVjS0_68Xm2RY9F08WhouYsISY8WfwhjYRse6cVodDC2sskKXpk5Mwj9NuGa0rvYdqsiHoIgzpCQfoiASYLWmyGlgLNXTYrejDnNQI46FVUBWCl4irIdIbP--2dMp81bQWwiiJJUhfyGWW_f-5r1IkBxXCUF6eN_ram7GD9O30EHDD4gA-poXg5CAfXQcDkbmTRCME0e0acaH3gdUOD4ck4CTOFBXpEqkIV07VHvQoketo0DE1tIHRf_IdFF5Lmz7N5775A; expires=Mon, 14-Nov-2022 21:36:45 GMT; path=/; domain=.google.com; Secure; HttpOnly"},
//{"Set-Cookie", "SIDCC=AJi4QfEakvOP89psiOO8Ps8EOV9SEOPDCM0KpkMpy7FLhwswqKnx0QSDuTaJlw0OzwSUgg_z6ClZ; expires=Mon, 15-May-2023 21:36:45 GMT; path=/; domain=.google.com; priority=high"},
//{"Set-Cookie", "__Secure-3PSIDCC=AJi4QfFeBNHHK-Lgk4u6IMg9rIX18ISrVy8dNXL9n4lmFXOB7oQPCDv6bG9WrR483pNWJqQSObQx; expires=Mon, 15-May-2023 21:36:45 GMT; path=/; domain=.google.com; Secure; HttpOnly; priority=high"},
{"Alt-Svc", "h3=\":443\"; ma=2592000,h3-29=\":443\"; ma=2592000,h3-Q050=\":443\"; ma=2592000,h3-Q046=\":443\"; ma=2592000,h3-Q043=\":443\"; ma=2592000,quic=\":443\"; ma=2592000; v=\"46,43\""},
{"Expires", ""},
{"Content-Length", ""}
        };

        public static readonly string POSTlogBody = "[\"-1\",null,[[[\"ANDROID_BACKUP\",0],[\"BATTERY_STATS\",0],[\"SMART_SETUP\",0],[\"TRON\",0]],-3334737594024971225],[],{\"175237375\":[10000]}]";

        public static readonly string[,] OPTIONSlogTemplate =
        {
            {"Access-Control-Allow-Origin", "https://accounts.google.com"},
{"Access-Control-Allow-Methods", "GET, POST, OPTIONS"},
{"Access-Control-Max-Age", "86400"},
{"Access-Control-Allow-Credentials", "true"},
{"Access-Control-Allow-Headers", "X-Playlog-Web,authorization,authorization,x-goog-authuser,origin"},
{"Content-Type", "text/plain; charset=UTF-8"},
{"Date", ""},
{"Server", "Playlog"},
{"Content-Length", "0"},
{"X-XSS-Protection", "0"},
{"X-Frame-Options", "SAMEORIGIN"},
{"Alt-Svc", "h3=\":443\"; ma=2592000,h3-29=\":443\"; ma=2592000,h3-Q050=\":443\"; ma=2592000,h3-Q046=\":443\"; ma=2592000,h3-Q043=\":443\"; ma=2592000,quic=\":443\"; ma=2592000; v=\"46,43\""}
        };

        public static readonly string[,] programmaticauthTemplate = new string[,]
        {
            {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"},
{"Connection", "keep-alive"},
{"User-Agent", "Mozilla/5.0 (iPad; CPU OS 12_4_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1.2 Mobile/15E148 Safari/604.1"},
{"Accept-Language", "en-us"},
{"Referer", ""},
{"Accept-Encoding", "br, gzip, deflate"}
        };

        public static readonly string authAdviceContent = @"{""client_state"":""{0}"",""external_browser"":""true"",""report_user_id"":""true"",""system_version"":""{1}"",""app_version"":""{2}"",""user_id"":[""104418674882697158928"",""108998313601114969390"",""107610895037684599465"",""101768705278564302385"",""110603104816895621950"",""103953819570519628763"",""100901841756087865591"",""108565316436590433476""],""safari_authentication_session"":""true"",""supported_service"":[""uca""],""request_trigger"":""ADD_ACCOUNT"",""lib_ver"":""3.3"",""package_name"":""com.google.Drive"",""redirect_uri"":""com.google.sso.640853332981:\/authCallback"",""device_name"":""{3}"",""client_id"":""640853332981.apps.googleusercontent.com"",""mediator_client_id"":""936475272427.apps.googleusercontent.com"",""device_id"":""{4}"",""hl"":""en-US"",""device_challenge_request"":""{5}"",""device_model"":""{6}""}";
        public static readonly string ESreferrer = "https://accounts.google.com/embedded/setup/v2/safarivc/identifier";
    }
    public static class QueryConvert
    {
        public static string ToQueryString(NameValueCollection nvc/*, bool modified = false*/)
        {
            StringBuilder sb = new StringBuilder("?");

            bool first = true;

            foreach (string key in nvc.AllKeys)
            {
                foreach (string value in nvc.GetValues(key))
                {
                    if (!first)
                    {
                        sb.Append("&");
                    }
                    //if (modified)

                    //    sb.AppendFormat("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value));
                    //else
                        sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));

                    first = false;
                }
            }

            return sb.ToString();
        }
        public static string SerializeObject<T>(T obj)
        {
            NameValueCollection nvc = new NameValueCollection();
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string name = prop.Name;
                if (Attribute.IsDefined(prop, typeof(DeserializeAsAttribute)))
                {
                    Debug.WriteLine("ATTRIBUTE");
                    object[] attrs = prop.GetCustomAttributes(true);
                    DeserializeAsAttribute attr = attrs[0] as DeserializeAsAttribute;
                    name = attr.Name;
                }
                Debug.WriteLine(prop == null);
                Debug.WriteLine(obj == null); 
                if (prop.PropertyType == typeof(string[]))
                {
                    string[] queries = (string[])prop.GetValue(obj);
                    foreach (string query in queries)
                        nvc.Add(name, query);
                }
                else if (prop.PropertyType == typeof(bool))
                    nvc.Add(name, prop.GetValue(obj).ToString().ToLower());
                else
                    nvc.Add(name, prop.GetValue(obj).ToString());
                //Console.WriteLine("{0}={1}", prop.Name, prop.GetValue(obj, null));
            }
            return ToQueryString(nvc);
        }
    }
    public static class GoogleRequestTransformer
    {
        public static string[] prepareValues(string html, GoogleAccountInfo gaInfo)
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);

            string ifkv = document.DocumentNode.SelectNodes(ifkvXpath)[0].Attributes["value"].Value;
            string TL = document.DocumentNode.SelectNodes(TLXpath)[0].Attributes["value"].Value;
            string gxf = document.DocumentNode.SelectNodes(gxfXpath)[0].Attributes["value"].Value;
            gxf = gxf.Replace(":", "%3A");

            gaInfo.username = gaInfo.username.Remove(gaInfo.username.IndexOf("@"));
            return new string[5] { ifkv, TL, gxf, gaInfo.username, gaInfo.password };
        }
        public static string CreateAuthAdviceContent(AuthAdviceInfo aa, bool minify = true)
        {
            AuthAdviceJson result = new AuthAdviceJson()
            {
                client_state = aa.client_state,
                device_challenge_request = aa.device_challenge_request,
                device_id = aa.device_id,
                device_model = aa.device_model,
                device_name = aa.device_name,
                system_version = aa.system_version
            };
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            json = Regex.Replace(json, @"\s(?=([^""]*""[^""]*"")*[^""]*$)", string.Empty);
            return json;
        }
        public static string[] PrepareAccountLookupValues(string data)
        {
            Debug.WriteLine("continue Extract");
            string continueSubstring = "</script>";
            string continueStart = "?continue=";
            string continueEnd = "&amp;";
            string @continue = data.Substring(data.IndexOf(continueSubstring));
            @continue = @continue.Substring(@continue.IndexOf(continueStart) + continueStart.Length);
            @continue = @continue.Remove(@continue.IndexOf(continueEnd));
            @continue = Uri.UnescapeDataString(@continue);

            Debug.WriteLine("sessionState Extract");
            string sessionStateStart = "data-initial-setup-data=\"%.@";
            string sessionStateSplit = "&quot;";

            string sessionState = data.Substring(data.IndexOf(sessionStateStart) + sessionStateStart.Length);
            string[] temp1 = sessionState.Split(new string[] { sessionStateSplit }, StringSplitOptions.None);
            foreach (string temp in temp1)
                if (!temp.Contains(",") & temp.Length != 2)
                {
                    sessionState = temp;
                    break;
                }

            Debug.WriteLine("azt Extract");
            string atNaztEnd = "Qzxixc";
            string atNaztSplit = "\\\"";

            string atNazt = data.Remove(data.IndexOf(atNaztEnd) + atNaztEnd.Length);
            string[] temp2 = atNazt.Split(new string[] { atNaztSplit }, StringSplitOptions.None);

            int count = 0;
            string at = "";
            string azt = "";
            foreach (string temp in temp2)
                if (!temp.Contains(",") & temp.Contains(":"))
                {
                    ++count;
                    if (count == 1)
                        at = temp;
                    else
                    {
                        azt = temp;
                        break;
                    }
                }

            Debug.WriteLine("Embedded Setup Request Uri Extract");
            string ESStart = @"https://accounts.google.com/Logout?continue\u003d";
            string ESEnd = @"\u0026timeStmp\u003d";
            string ES = data.Substring(data.IndexOf(ESStart) + ESStart.Length);
            ES = ES.Remove(ES.IndexOf(ESEnd));
            ES = Uri.UnescapeDataString(ES);

            Debug.WriteLine(@continue);
            Debug.WriteLine(sessionState);
            Debug.WriteLine(at);
            Debug.WriteLine(azt);
            Debug.WriteLine(ES);
            return new string[] { @continue, sessionState, at, azt, ES };
        }
        public static string CreateAccountLookupReferer(AuthAdviceInfo aa, string @as, string auth_extension, string code_challenge)
        {
            EmbeddedSetupQuery result = new EmbeddedSetupQuery()
            {
                @as = @as,
                auth_extension = auth_extension,
                app_version = AuthAdvice.app_version,
                code_challenge = code_challenge,
                device_name = aa.device_name,
                system_version = aa.system_version,
                device_model = aa.device_model,
                client_id = AuthAdvice.mediator_client_id,
                delegated_client_id = AuthAdvice.client_id
            };
            string query = QueryConvert.SerializeObject(result);
            Debug.WriteLine(query);
            return ESreferrer + query;
        }
        public static string CreateAccountLookupReferer(string requestUri)
        {
            return requestUri + "&flowName=GlifSetupSafariVC";
        }
        public static string CreateDeviceInfoJson(AuthAdviceInfo aa, string @as, string auth_extension, string code_challenge, string country)
        {
            DeviceInfo11Json df11 = new DeviceInfo11Json()
            {
                _1 = AuthAdvice.mediator_client_id,
                _4 = auth_extension,
                _6 = aa.device_name,
                _7 = EmbeddedSetup.code_challenge_method,
                _8 = code_challenge,
                _9 = EmbeddedSetup.redirect_uri,
                _14 = AuthAdvice.client_id,
                _19 = aa.device_model
            };
            
            DeviceInfoJson df = new DeviceInfoJson()
            {
                _5 = country,
                _11 = df11
            };
            string dfJson = JsonConvert.SerializeObject(df, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });

            Debug.WriteLine(dfJson);
            return dfJson;
        }
        public static string CreateAccountLookupFReq(AuthAdviceInfo aa, string username, string sessionState, string country,
            string ESRequestUri, string auth_extension, string code_challenge)
        {
            FReq10_2Json fReq10_2 = new FReq10_2Json()
            {
                _4 = ESRequestUri
            };
            FReq10_4Json fReq10_4 = new FReq10_4Json()
            {
                _1 = AuthAdvice.mediator_client_id,
                _4 = auth_extension,
                _6 = aa.device_name,
                _7 = EmbeddedSetup.code_challenge_method,
                _8 = code_challenge,
                _9 = EmbeddedSetup.redirect_uri,
                _14 = AuthAdvice.client_id,
                _19 = aa.device_model
            };
            FReq10_17Json fReq10_17 = new FReq10_17Json()
            {
                _9 = AuthAdvice.client_id
            };
            FReq10Json fReq10 = new FReq10Json()
            {
                _2 = fReq10_2,
                _4 = fReq10_4,
                _17 = fReq10_17
            };
            FReqJson fReq = new FReqJson()
            {
                _0 = username,
                _1 = sessionState,
                _4 = country,
                _10 = fReq10,
                _11 = username
            };
            string fReqJson = JsonConvert.SerializeObject(fReq, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });

            Debug.WriteLine(fReqJson);
            return fReqJson;
        }
        public static string CreateAccountLookupBgRequest(string bgRequest)
        {
            BgRequestJson BgRequest = new BgRequestJson()
            {
                _1 = bgRequest
            };
            string bgRequestJson = JsonConvert.SerializeObject(BgRequest, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });
            Debug.WriteLine(bgRequestJson);
            return bgRequestJson;
        }
        public static string CreateAccountLookupContent(AuthAdviceInfo aa, AccountLookupInfo al)
        {
            string device_info = CreateDeviceInfoJson(aa, al.@as, al.auth_extension, al.code_challenge, al.country);
            string fReq = CreateAccountLookupFReq(aa, al.username, al.sessionState, al.country, al.ESrequestUri, al.auth_extension, al.code_challenge);
            string bgRequest = CreateAccountLookupBgRequest(al.bgRequest);

            AccountLookupQuery accountlookup = new AccountLookupQuery()
            {
                client_id = AuthAdvice.mediator_client_id,
                @continue = al.@continue,
                fReq = fReq,
                bgRequest = bgRequest,
                at = al.at,
                azt = al.azt,
                device_info = device_info
            };

            string result = QueryConvert.SerializeObject<AccountLookupQuery>(accountlookup);
            Debug.WriteLine(result);
            return result;
        }
        public static string CreateProgrammaticAuthReferer(AuthAdviceInfo aa, string @as, string code_challenge,
            string auth_extension, string TL)
        {
            ProgrammaticAuthReferer programmaticAuth = new ProgrammaticAuthReferer()
            {
                app_version = AuthAdvice.app_version,
                auth_extension = auth_extension,
                @as = @as,
                code_challenge = code_challenge,
                client_id = AuthAdvice.mediator_client_id,
                delegated_client_id = AuthAdvice.client_id,
                device_model = aa.device_model,
                device_name = aa.device_name,
                system_version = aa.system_version,
                TL = TL
            };
            string result = QueryConvert.SerializeObject<ProgrammaticAuthReferer>(programmaticAuth);
            Debug.WriteLine(result);
            return result;
        }
        public static string CreateProgrammaticAuthQuery(string part)
        {
            ProgrammaticAuthQuery programmaticAuth = new ProgrammaticAuthQuery()
            {
                delegated_client_id = AuthAdvice.client_id,
                part = part
            };
            string result = QueryConvert.SerializeObject<ProgrammaticAuthQuery>(programmaticAuth);
            Debug.WriteLine(result);
            return result;
        }
        public static string SerializeValueOnly(object obj)
        {
            var valuesList = JArray.FromObject(obj).Select(x => x.Values().ToList()).ToList();
            string finalRes = JsonConvert.SerializeObject(valuesList, Formatting.Indented);
            return finalRes;
        }
        public static IEnumerable<object> GetPropertyValues<T>(T input)
        {
            return input.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => 
                {
                    object pI = p.GetValue(input);
                    while (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                    {
                        pI = GetPropertyValues(p.GetValue(input));
                    }
                    return pI;
                });
        }
        public class AuthAdviceJson
        {
            public string client_state { get; set; }
            public string external_browser => AuthAdvice.external_browser;
            public string report_user_id => AuthAdvice.report_user_id;
            public string system_version { get; set; }
            public string app_version => AuthAdvice.app_version;
            public string[] user_id => AuthAdvice.user_id;

            public string safari_authentication_session => AuthAdvice.safari_authentication_session;
            public string[] supported_service => AuthAdvice.supported_service;
            public string request_trigger => AuthAdvice.request_trigger;
            public string lib_ver => AuthAdvice.lib_ver;
            public string package_name => AuthAdvice.package_name;
            public string redirect_uri => AuthAdvice.redirect_uri;
            public string device_name { get; set; }
            public string client_id => AuthAdvice.client_id;
            public string mediator_client_id => AuthAdvice.mediator_client_id;
            public string device_id { get; set; }
            public string hl => AuthAdvice.hl;
            public string device_challenge_request { get; set; }
            public string device_model { get; set; }
            
        }
        public class EmbeddedSetupQuery
        {
            public string @as { get; set; }
            public string scope => EmbeddedSetup.scope;
            public string client_id { get; set; }
            public string redirect_uri { get; set; }
            public string sarp => EmbeddedSetup.sarp; 
            public string delegated_client_id { get; set; }
            public string hl => EmbeddedSetup.hl;
            public string device_name { get; set; }
            public string auth_extension { get; set; }
            public string system_version { get; set; }
            public string app_version { get; set; }
            public string kdlc => EmbeddedSetup.kdlc;
            public string kss => EmbeddedSetup.kss;

            public string lib_ver => EmbeddedSetup.lib_ver;
            public string device_model { get; set; }
            public string code_challenge_method => EmbeddedSetup.code_challenge_method;
            public string code_challenge { get; set; }
            public string sfas => EmbeddedSetup.sfas;
            public string flowName => EmbeddedSetup.flowName;
        }

        [JsonConverter(typeof(DeviceIndexJsonConverter<DeviceInfoJson>))]
        public class DeviceInfoJson
        {
            public object _0 => DeviceInfo._0;
            public object _1 => DeviceInfo._1;
            public object _2 => DeviceInfo._2;
            public List<string> _3 => DeviceInfo._3;
            public object _4 => DeviceInfo._4;
            public string _5 { get; set; }
            public object _6 => DeviceInfo._6;
            public object _7 => DeviceInfo._7;
            public object _8 => DeviceInfo._8;
            public string _9 => DeviceInfo._9;
            public object _10 => DeviceInfo._10;
            public DeviceInfo11Json _11 { get; set; }
            public object _12 => DeviceInfo._12;
            public object _13 => DeviceInfo._13;
            public bool _14 => DeviceInfo._14;
            public object _15 => DeviceInfo._15;
            public int _16 => DeviceInfo._16;
            public object _17 => DeviceInfo._17;
            public bool _18 => DeviceInfo._18;
            public int _19 => DeviceInfo._19;
            public string _20 => DeviceInfo._20;
        }
        [JsonConverter(typeof(DeviceIndexJsonConverter<DeviceInfo11Json>))]
        public class DeviceInfo11Json
        {
            public int _0 => DeviceInfo11._0;
            public string _1 { get; set; }
            public string[] _2 => DeviceInfo11._2;
            public object _3 => DeviceInfo11._3;
            public string _4 { get; set; }
            public object _5 => DeviceInfo11._5;
            public string _6 { get; set; }
            public string _7 { get; set; }
            public string _8 { get; set; }
            public string _9 { get; set; }
            public object _10 => DeviceInfo11._10;
            public object _11 => DeviceInfo11._11;
            public object _12 => DeviceInfo11._12;
            public object _13 => DeviceInfo11._13;
            public string _14 { get; set; }
            public object _15 => DeviceInfo11._15;
            public object _16 => DeviceInfo11._16;
            public object _17 => DeviceInfo11._17;
            public object _18 => DeviceInfo11._18;
            public string _19 { get; set; }
            public object _20 => DeviceInfo11._20;
            public object _21 => DeviceInfo11._21;
            public int _22 => DeviceInfo11._22;
            public object _23 => DeviceInfo11._23;
            public object _24 => DeviceInfo11._24;
            public string[] _25 => DeviceInfo11._25;
            public object _26 => DeviceInfo11._26;
            public object _27 => DeviceInfo11._27;
            public object _28 => DeviceInfo11._28;
            public object _29 => DeviceInfo11._29;
            public string[] _30 => DeviceInfo11._30;
        }
        [JsonConverter(typeof(DeviceIndexJsonConverter<FReqJson>))]
        public class FReqJson
        {
            public string _0 { get; set; }
            public string _1 { get; set; }
            public string[] _2 => new string[] { };
            public object _3 => null;
            public string _4 { get; set; }
            public string _5 => null;
            public string _6 => null;
            public int _7 => 2;
            public bool _8 => false;
            public bool _9 => true;
            public FReq10Json _10 { get; set; }
            public string _11 { get; set; }
            public object _12 => null;
            public object _13 => null;
            public object _14 => null;
            public bool _15 => true;
            public bool _16 => true;
            public string[] _17 => new string[] { };
        }
        [JsonConverter(typeof(DeviceIndexJsonConverter<FReq10Json>))]
        public class FReq10Json
        {
            public object _0 => FReq10._0;
            public object _1 => FReq10._1;
            public FReq10_2Json _2 { get; set; }
            public int _3 => FReq10._3;
            public FReq10_4Json _4 { get; set; }
            public object _5 => FReq10._5;
            public object _6 => FReq10._6;
            public object _7 => FReq10._7;
            public bool _8 => FReq10._8;
            public object _9 => FReq10._9;
            public object _10 => FReq10._10;
            public object _11 => FReq10._11;
            public object _12 => FReq10._12;
            public object _13 => FReq10._13;
            public object _14 => FReq10._14;
            public object _15 => FReq10._15;
            public object _16 => FReq10._16;
            public FReq10_17Json _17 { get; set; }
        }
        [JsonConverter(typeof(DeviceIndexJsonConverter<FReq10_2Json>))]
        public class FReq10_2Json
        {
            public int _0 => FReq10_2._0;
            public int _1 => FReq10_2._1;
            public int _2 => FReq10_2._2;
            public int _3 => FReq10_2._3;
            //Referer (embedded's request uri)
            public string _4 { get; set; }
            public object _5 => FReq10_2._5;
            public string[] _6 => FReq10_2._6;
            public int _7 => FReq10_2._7;
            public string[] _8 => FReq10_2._8;
            public string _9 => FReq10_2._9;
            public object _10 => FReq10_2._10;
            public string[] _11 => FReq10_2._11;
        }
        [JsonConverter(typeof(DeviceIndexJsonConverter<FReq10_4Json>))]
        public class FReq10_4Json
        {
            public int _0 => FReq10_2._0;
            public string _1 { get; set; }
            public string[] _2 => FReq10_4._2;
            public object _3 => FReq10_4._3;
            public string _4 { get; set; }
            public object _5 => FReq10_4._5;
            public string _6 { get; set; }
            public string _7 { get; set; }
            public string _8 { get; set; }
            public string _9 { get; set; }
            public object _10 => FReq10_4._10;
            public object _11 => FReq10_4._11;
            public object _12 => FReq10_4._12;
            public object _13 => FReq10_4._13;
            public string _14 { get; set; }
            public object _15 => FReq10_4._15;
            public object _16 => FReq10_4._16;
            public object _17 => FReq10_4._17;
            public object _18 => FReq10_4._18;
            public string _19 { get; set; }
            public object _20 => FReq10_4._20;
            public object _21 => FReq10_4._21;
            public int _22 => FReq10_4._22;
            public object _23 => FReq10_4._23;
            public object _24 => FReq10_4._24;
            public object _25 => FReq10_4._25;
            public object _26 => FReq10_4._26;
            public object _27 => FReq10_4._27;
            public object _28 => FReq10_4._28;
            public object _29 => FReq10_4._29;
            public string[] _30 => FReq10_4._30;
        }
        [JsonConverter(typeof(DeviceIndexJsonConverter<FReq10_17Json>))]
        public class FReq10_17Json
        {
            public object _0 => FReq10_17._0;
            public bool _1 => FReq10_17._1;
            public object _2 => FReq10_17._2;
            public object _3 => FReq10_17._3;
            public object _4 => FReq10_17._4;
            public object _5 => FReq10_17._5;
            public object _6 => FReq10_17._6;
            public object _7 => FReq10_17._7;
            public object _8 => FReq10_17._8;
            public string _9 { get; set; }
            public bool _10 => FReq10_17._10;
        }
        [JsonConverter(typeof(DeviceIndexJsonConverter<FReq10_17Json>))]
        public class BgRequestJson
        {
            public string _0 => BgRequest._0;
            //bgRequest
            public string _1 { get; set; }
        }
        public class AccountLookupQuery
        {
            public string scope => AccountLookup.scope;
            public string client_id { get; set; }
            public string hl => AccountLookup.hl;
            public string sarp => AccountLookup.sarp;
            //public string app_version => "4.2021.22202";
            public string @continue { get; set; }
            [DeserializeAs("f.req")]
            public string fReq { get; set; }
            public string bgRequest { get; set; }
            public string at { get; set; }
            public string azt { get; set; }
            public bool cookiesDisabled => AccountLookup.cookiesDisabled;
            public string igresponse => AccountLookup.igresponse;
            public string device_info { get; set; }
            public string gmscoreversion => AccountLookup.gmscoreversion;
            public string checkConnection => AccountLookup.checkConnection;
            public string checkedDomains => AccountLookup.checkedDomains;
        }
        public class ProgrammaticAuthQuery
        {
            public string part { get; set; }
            public string delegated_client_id { get; set; }
            public string hl => ProgrammaticAuth.hl2;
            public string authuser => ProgrammaticAuth.authuser;
        }
        public class ProgrammaticAuthReferer
        {
            public string @as { get; set; }
            public string scope => ProgrammaticAuth.scope;
            public string client_id { get; set; }
            public string redirect_uri => ProgrammaticAuth.redirect_uri;
            public string sarp => ProgrammaticAuth.sarp;
            public string delegated_client_id { get; set; }
            public string hl => ProgrammaticAuth.hl1;
            public string device_name { get; set; }
            public string auth_extension { get; set; }
            public string system_version { get; set; }
            public string app_version { get; set; }
            public string kdlc => ProgrammaticAuth.kdlc;
            public string kss => ProgrammaticAuth.kss;
            public string lib_ver => ProgrammaticAuth.lib_ver;
            public string device_model { get; set; }
            public string code_challenge_method => ProgrammaticAuth.code_challenge_method;
            public string code_challenge { get; set; }
            public string sfas => ProgrammaticAuth.sfas;
            public string flowName => ProgrammaticAuth.flowName;
            public string cid => ProgrammaticAuth.cid;
            public string navigationDirection => ProgrammaticAuth.navigationDirection;
            public string TL { get; set; }
        }
    }
}
