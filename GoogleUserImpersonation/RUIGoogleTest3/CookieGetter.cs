using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace Pet.RUIGoogle
{
    public static class CookieGetter
    {
        public static async Task<string> GetCookies(ChromiumWebBrowser browser)
        {
            string cookies = "";
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CookieMonster visitor = new CookieMonster((cookieList) =>
            {
                foreach (Tuple<string, string> cookie in cookieList)
                    cookies += $"{cookie.Item1}={cookie.Item2}; ";
                cookies.Remove(cookies.Length - 2);

                tcs.SetResult(true);
            });

            browser.GetCookieManager().VisitAllCookies(visitor);

            await tcs.Task;
            visitor.RemoveAllSubscribers();
            return cookies;
        }
    }
    public class CookieVisitor : CefSharp.ICookieVisitor
    {
        //Action<cookie, isCompleted>
        public event Action<CefSharp.Cookie, bool> SendCookie;
        public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            deleteCookie = false;
            if (SendCookie != null)
            {
                SendCookie(cookie, (count + 1) == total);
            }
            return true;
        }
        public void Dispose()
        {

        }
        public void RemoveAllSubscribers()
        {
            foreach (Delegate d in SendCookie.GetInvocationList())
            {
                SendCookie -= (Action<CefSharp.Cookie, bool>)d;
            }
        }      
    }
        

    class CookieMonster : ICookieVisitor
    {       
        readonly List<Tuple<string, string>> cookies = new List<Tuple<string, string>>();
        public Action<IEnumerable<Tuple<string, string>>> useAllCookies;

        public CookieMonster(Action<IEnumerable<Tuple<string, string>>> useAllCookies)
        {
            this.useAllCookies = useAllCookies;
        }

        public void Dispose()
        {
        }

        public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            cookies.Add(new Tuple<string, string>(cookie.Name, cookie.Value));

            if (count == total - 1)
                useAllCookies(cookies);

            return true;
        }

        public void RemoveAllSubscribers()
        {
            foreach (Delegate d in useAllCookies.GetInvocationList())
            {
                useAllCookies -= (Action<IEnumerable<Tuple<string, string>>>)d;
            }
        }
    }

    public static class CookieGetter_
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
        string url,
        string cookieName,
        StringBuilder cookieData,
        ref int size,
        Int32 dwFlags,
        IntPtr lpReserved);

        private const Int32 InternetCookieHttponly = 0x2000;

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }
        public static List<System.Net.Cookie> List(this CookieContainer container)
        {
            var cookies = new List<System.Net.Cookie>();

            if (container == null || container.Count == 0)
                return cookies;

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                BindingFlags.NonPublic |
                BindingFlags.GetField |
                BindingFlags.Instance,
                null,
                container,
                null);

            foreach (string key in table.Keys)
            {
                var item = table[key];
                var items = (ICollection)item.GetType().GetProperty("Values").GetGetMethod().Invoke(item, null);
                foreach (CookieCollection cc in items)
                {
                    foreach (System.Net.Cookie cookie in cc)
                    {
                        cookies.Add(cookie);
                    }
                }
            }

            return cookies;
        }
    }
}
