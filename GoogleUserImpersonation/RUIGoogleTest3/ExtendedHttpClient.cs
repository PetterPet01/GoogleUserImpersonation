using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Collections.Specialized;
using System.Reflection;
using System.Net.Http.Headers;

namespace Pet.RUIGoogle
{
    public static class Extender
    {
        public class CustomHttpHeaders : HttpHeaders
        {
            private const BindingFlags allInstance =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            private static readonly FieldInfo innerHeaders =
                typeof(HttpHeaderValueCollection<string>).GetField("store", allInstance);

            private static readonly FieldInfo innerStore =
                typeof(HttpHeaders).GetField("headerStore", allInstance);

            private static readonly PropertyInfo dictKeys =
        typeof(WebHeaderCollection).GetProperty("Keys", allInstance);

            public static void InjectInto(ref HttpRequestMessage message)
            {
                // WebHeaderCollection uses a custom IEqualityComparer for its internal
                // NameValueCollection. Here we get the InnerCollection property so that
                // we can reuse its IEqualityComparer (via our constructor).
                var innerColl = (HttpHeaders)innerHeaders.GetValue(message.Headers.Connection);

                //var assembly = typeof(HttpHeaders).Assembly; //Class B is in the same namespace as A
                //Type type = assembly.GetType("System.Net.Http.Headers.HeaderStoreItemInfo");
                var innerDict = innerStore.GetValue(innerColl);

                //innerHeaders.SetValue(message.Headers.Connection, new CustomHttpHeaders(innerColl));
                // Use reflection to get the Method
                var type = innerDict.GetType();
                var methodInfo = type.GetMethod("Remove");
                var methodInfo2 = type.GetMethod("ContainsKey");
                Debug.WriteLine("CONTAINS: " + ((bool)methodInfo2.Invoke(innerDict, new object[] { "Connection" })).ToString());

                // Invoke the method here
                Debug.WriteLine("REMOVE: " + ((bool)methodInfo.Invoke(innerDict, new object[] { "Connection" })).ToString());
                //innerDict.Remove("Connection");
                innerColl.Add("Connection", "keep-alive");

                Debug.WriteLine("INJECTED" + (message.Headers.Connection == null).ToString());
            }

            private CustomHttpHeaders(HttpHeaders headers) : base()
            {
                var enumerator = headers.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, IEnumerable<string>> kvp = enumerator.Current;
                    // Perform logic on the item
                    base.Add(kvp.Key, kvp.Value);
                }
                Remove("Connection");
                base.Add("Connection", "keep-alive");
            }

            //public override void Add(string name, string value)
            //{
            //    if (name == "Connection") return;
            //    base.Add(name, value);
            //}
        }

        public async static Task<HttpResponseMessage> SendAsyncWithCookie(this HttpClient client, CookieContainer container, HttpRequestMessage message, bool noPath = true)
        {
            //CookieCollection temp = new CookieCollection();
            List<Cookie> cookies = GetCookies(message, noPath);

            CookieCollection currentCookies = container.GetCookies(message.RequestUri);
            string[] names = new string[currentCookies.Count];
            for (int i = 0; i < currentCookies.Count; i++)
            {
                names[i] = currentCookies[i].Name;
            }
            message.Headers.Remove("Cookie");
            foreach (Cookie cookie in cookies)
            {
                if (names.Contains(cookie.Name))
                {
                    container.SetCookies(noPath ?
                        new Uri(message.RequestUri.AbsoluteUri.Replace(message.RequestUri.PathAndQuery,
                        string.Empty)) : message.RequestUri, $"{cookie.Name}={cookie.Value}");
                }
                else
                    container.Add(cookie);
            }

            HttpResponseMessage response = await client.SendAsync(message);

            //string[] names2 = new string[temp.Count];
            //for (int i = 0; i < temp.Count; i++)
            //{
            //    names2[i] = temp[i].Name;
            //}
            currentCookies = container.GetCookies(message.RequestUri);
            string[] names3 = new string[currentCookies.Count];
            for (int i = 0; i < currentCookies.Count; i++)
            {
                names3[i] = currentCookies[i].Name;
            }
            IEnumerable<string> duplicate = names3.GroupBy(x => x)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key);
            foreach (string dup in duplicate)
                Debug.WriteLine("DUPLICATE: " + dup);
            foreach (Cookie cookie in currentCookies)
            {
                Debug.WriteLine($"{cookie.Name}={cookie.Value}");
                if (duplicate.Contains(cookie.Name))
                    foreach (Cookie cookieTemp in cookies)
                        if (cookie.Name == cookieTemp.Name & cookie.Value == cookieTemp.Value)
                        {
                            Debug.WriteLine("EXPIRED: " + cookie.Name + "=" + cookie.Value);
                            cookie.Expired = true;
                        }
            }
                    

            return response;
        }
        public static List<Cookie> GetCookies(HttpRequestMessage message, bool noPath = true)
        {
            List<Cookie> result = new List<Cookie>();
            string[] cookies = message.Headers.GetValues("Cookie").First().Split(';');
            for (int i = 0; i < cookies.Length; i++)
            {
                string trimmed = cookies[i].Trim();
                int index = trimmed.IndexOf("=");
                result.Add(new Cookie(trimmed.Remove(index), trimmed.Substring(index + 1),
                    noPath ? "/" : message.RequestUri.AbsolutePath, message.RequestUri.Host));
            }

            return result;
        }
        public static List<Cookie> GetCookies(HttpResponseMessage message)
        {
            message.Headers.TryGetValues("Set-Cookie", out var cookiesHeader);
            var cookies = cookiesHeader.Select(cookieString =>
            {
                var cookie = CreateCookie(cookieString);
                cookie.Domain = message.RequestMessage.RequestUri.Host;
                return cookie;
            }).ToList();
            return cookies;
        }
        public static Cookie CreateCookie(string cookieString)
        {
            var properties = cookieString.Split(';');
            for (int i = 0; i < properties.Length; i++)
                properties[i] = properties[i].Trim();
            var name = properties[0].Split('=')[0];
            var value = properties[0].Split('=')[1];
            var path = properties[2].Replace("path=", "");
            Debug.WriteLine("COOKIE PATH= " + path);
            var cookie = new Cookie(name, value, path)
            {
                Secure = properties.Contains("secure", StringComparer.OrdinalIgnoreCase),
                HttpOnly = properties.Contains("httponly", StringComparer.OrdinalIgnoreCase),
                Expires = DateTime.Parse(properties[1].Replace("expires=", ""))
            };
            return cookie;
        }
    }
    public delegate void HttpClientIntercept(object sender, HttpClientInterceptArgs e);
    public class HttpClientInterceptArgs : EventArgs
    {
        public object Request;
        public bool isRequest;
        public HttpClientInterceptArgs(object request, bool isRequest)
        {
            this.Request = request;
            this.isRequest = isRequest;
        }
    }
    public class HttpClientInterceptorHandler : DelegatingHandler
    {
        static readonly int[] validRedirectCodes = new int[] { 301, 302, 303 };
        public event HttpClientIntercept HttpClientIntercepted;
        List<Cookie> cookieContainer;

        public HttpClientInterceptorHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        private HttpRequestMessage lowercaseKeepAlive(HttpRequestMessage request)
        {
            HttpRequestMessage result = new HttpRequestMessage();

            Debug.WriteLine(request.Headers.Connection.GetEnumerator().Current);

            //{
            //Content = new StringContent(""),

            //};

            foreach (KeyValuePair<string, IEnumerable<string>> kvp in request.Headers)
                foreach (string value in kvp.Value)
                    result.Headers.Add(kvp.Key, value);
            result.Content = request.Content;
            if (request.Headers.Connection.Contains("keep-alive", StringComparer.OrdinalIgnoreCase))
            {
                result.Headers.Connection.Remove("Keep-Alive");
                Debug.WriteLine("KEEPALIVE");
                foreach (string variant in request.Headers.Connection)
                {
                    Debug.WriteLine("variations: " + variant);
                    result.Headers.Connection.Remove(variant);
                }
                result.Headers.Add("Connection", "keep-alive");
            }
            result.Method = request.Method;
            result.RequestUri = new Uri(request.RequestUri.AbsoluteUri);
            result.Version = request.Version;
            return result;
        }

        private HttpRequestMessage ToRedirectRequest(HttpResponseMessage response)
        {
            if (!validRedirectCodes.Contains((int)response.StatusCode))
                throw new Exception("Redirecting request failed. Status code is invalid.");

            HttpRequestMessage result = new HttpRequestMessage();

            Debug.WriteLine(response.RequestMessage.Headers.Connection.GetEnumerator().Current);
            System.Diagnostics.Debug.WriteLine("REDIRECT: " + response.Headers.Location.AbsoluteUri);

            foreach (KeyValuePair<string, IEnumerable<string>> kvp in response.RequestMessage.Headers)
                foreach (string value in kvp.Value)
                    result.Headers.Add(kvp.Key, value);

            result.Method = HttpMethod.Get;
            result.RequestUri = new Uri(response.Headers.Location.AbsoluteUri);
            result.Version = response.RequestMessage.Version;
            return result;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (HttpClientIntercepted == null)
                throw new Exception("HttpClientIntercepted NULL");
            HttpClientIntercept temp = Volatile.Read(ref HttpClientIntercepted);

            //Console.WriteLine("Request:");
            //Console.WriteLine(request.ToString());
            //if (request.Content != null)
            //{
            //    Console.WriteLine(await request.Content.ReadAsStringAsync());
            //}
            //Console.WriteLine();
            //if (request.Headers.Connection.Contains("keep-alive", StringComparer.OrdinalIgnoreCase))
            //{
            //request.Headers.ConnectionClose = false;
            //Extender.CustomHttpHeaders.InjectInto(ref request);
            //}
            await Task.Run(() => temp.Invoke(this, new HttpClientInterceptArgs(request, true)));
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            //while (validRedirectCodes.Contains((int)response.StatusCode))
            //{
            //    await Task.Run(() => temp.Invoke(this, new HttpClientInterceptArgs(response, false)));
            //    HttpRequestMessage redirect = ToRedirectRequest(response);
            //    System.Diagnostics.Debug.WriteLine(request.Headers.ToString() + request.Content.Headers.ToString());
            //    await Task.Run(() => temp.Invoke(this, new HttpClientInterceptArgs(redirect, true)));
            //    response = await base.SendAsync(redirect, cancellationToken);
            //}
            await Task.Run(() => temp.Invoke(this, new HttpClientInterceptArgs(response, false)));
            
            //Console.WriteLine("Response:");
            //Console.WriteLine(response.ToString());
            //if (response.Content != null)
            //{
            //    Console.WriteLine(await response.Content.ReadAsStringAsync());
            //}
            //Console.WriteLine();

            return response;
        }
    }
}
