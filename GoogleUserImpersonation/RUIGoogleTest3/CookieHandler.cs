using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Web;
using Pet.RUIGoogle;

namespace Pet.RUIGoogle
{
    public static class CookieHandler
    {
        static readonly string startUri = "https://myaccount.google.com/";
        static readonly string hostgapsUri = "https://accounts.google.com/ServiceLogin?passive=1209600&osid=1&continue=https://myaccount.google.com/&followup=https://myaccount.google.com/";
        static readonly string signup1 = "https://accounts.google.com/_/kids/signup/eligible?hl=vi&_reqid={0}&rt=j";
        static readonly string signup1Referer = "https://accounts.google.com/signin/v2/identifier?passive=1209600&osid=1&continue=https%3A%2F%2Fmyaccount.google.com%2Fgeneral-light&followup=https%3A%2F%2Fmyaccount.google.com%2Fgeneral-light&flowName=GlifWebSignIn&flowEntry=ServiceLogin";


        static readonly string userAgent = UserAgent.Value;

        static Random rand = new Random();

        static string combineCookies(params string[] cookies)
        {
            string res = "";
            foreach (string cookie in cookies)
                res += cookie + "; ";
            return res.Remove(res.Length - 1);
        }

        public static async Task<string> GetFirstNID(HttpClient httpClient)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(startUri),
                Method = HttpMethod.Get,
                Headers = {
                    { "Accept", "text/html, application/xhtml+xml, */*" },
                    { "Accept-Language", "en-US" },
                    { "Accept-Encoding", "gzip, deflate" },
                    { "Connection", "Keep-Alive" },
                    { "User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko" }
                }
            };

            HttpResponseMessage response = await httpClient.SendAsync(request);
            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;

            foreach (string cookie in cookies)
                if (cookie.IndexOf("NID") == 0)
                    return cookie;
            return "";
        }

        static string[] signIn1Prep(string data)
        {
            Debug.WriteLine("ifkv Extract");
            string ifkvStart = "&quot;ifkv&quot;,&quot;";
            string ifkvEnd = "&quot;";

            string ifkv = data.Substring(data.IndexOf(ifkvStart) + ifkvStart.Length);
            ifkv = ifkv.Remove(ifkv.IndexOf(ifkvEnd));

            Debug.WriteLine("fReq Extract");
            string fReqStart = "data-initial-setup-data=\"%.@";
            string fReqSplit = "&quot;";

            string fReq = data.Substring(data.IndexOf(fReqStart) + fReqStart.Length);
            string[] temp1 = fReq.Split(new string[] { fReqSplit }, StringSplitOptions.None);
            foreach (string temp in temp1)
                if (!temp.Contains(",") & temp.Length != 2)
                {
                    fReq = temp;
                    break;
                }

            Debug.WriteLine("azt Extract");
            string aztEnd = "Qzxixc";
            string aztSplit = "\\\"";

            string azt = data.Remove(data.IndexOf(aztEnd) + aztEnd.Length);
            string[] temp2 = azt.Split(new string[] { aztSplit }, StringSplitOptions.None);
            foreach (string temp in temp2)
                if (!temp.Contains(",") & temp.Contains(":"))
                {
                    azt = temp;
                    break;
                }

            return new string[3] { ifkv, fReq, azt };
        }

        public static async Task<string[]> GetHostGaps(HttpClient httpClient/*, string NID*/)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(hostgapsUri),
                Method = HttpMethod.Get,
                Headers = {
                    { "Accept", "text/html, application/xhtml+xml, */*" },
                    { "Accept-Language", "en-US" },
                    { "Accept-Encoding", "gzip, deflate" },
                    { "Connection", "Keep-Alive" },
                    { "User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko" }/*,*/
                    //{ HttpRequestHeader.Cookie.ToString(), NID }
                }
            };

            string[] result = new string[4];

            HttpResponseMessage response = await httpClient.SendAsync(request);
            string data = await response.Content.ReadAsStringAsync();
            IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;

            string hostgapsCookie = "";

            foreach (string cookie in cookies)
                if (cookie.IndexOf("__Host-GAPS") == 0)
                {
                    hostgapsCookie = cookie;
                    break;
                }

            string[] prep = signIn1Prep(data);
            result[0] = hostgapsCookie;
            for (int i = 0; i < 3; i++)
                result[i + 1] = prep[i];

            return result;
        }

        public static async Task<int> SignUp1(HttpClient httpClient, string NID, string[] prep)
        {
            int reqID = rand.Next(30000, 80000);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format(signup1, reqID)),
                Method = HttpMethod.Post,
                Headers = {
                    { "Accept", "*/*" },
                    { "X-Same-Domain", "1" },
                    { "Google-Accounts-XSRF", "1" },
                    { "Referer", signup1Referer },
                    { "Accept-Language", "en-US" },
                    { "Accept-Encoding", "gzip, deflate" },
                    { "User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko" },
                    { "Connection", "Keep-Alive" },
                    { "Cookie", combineCookies(prep[0], NID) },
                    { "Cache-Control", "no-cache" }
                },
                Content = new StringContent("ifkv=" + prep[1] +
                "&continue=https%3A%2F%2Fmyaccount.google.com%2Fgeneral-light&followup=https%3A%2F%2Fmyaccount.google.com%2Fgeneral-light&osid=1&f.req=%5B%22" + prep[2] +
                "%22%2C%22S-503237282%3A1652716372961472%22%5D&azt=" + prep[3].Replace(":", "%3A") + "&cookiesDisabled=false&deviceinfo=%5Bnull%2Cnull%2Cnull%2C%5B%5D%2Cnull%2C%22VN%22%2Cnull%2Cnull%2Cnull%2C%22GlifWebSignIn%22%2Cnull%2C%5Bnull%2Cnull%2C%5B%5D%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2Cnull%2C%5B%5D%2Cnull%2Cnull%2Cnull%2Cnull%2C%5B%5D%5D%2Cnull%2Cnull%2Cnull%2Cnull%2C0%2Cnull%2Cfalse%2C2%2C%22%22%5D&gmscoreversion=undefined&checkConnection=&checkedDomains=youtube&pstMsg=1&", Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            return reqID;
        }
    }
}
