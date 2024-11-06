using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Handler;

namespace Pet.RUIGoogle
{
    //Long term Request
    [Serializable]
    public class PostData_
    {
        public IList<PostDataElement_> Elements { get; set; }
        public bool HasExcludedElements { get; set; }
        public bool IsDisposed { get; set; }
        public bool IsReadOnly { get; set; }
    }

    [Serializable]
    public class PostDataElement_
    {
        public byte[] Bytes { get; set; }
        public string File { get; set; }
        public bool IsReadOnly { get; set; }
        public PostDataElementType Type { get; set; }
    }

    [Serializable]
    public class LTRequest
    {
        public UrlRequestFlags Flags { get; set; }
        public NameValueCollection Headers { get; set; }
        public ulong Identifier;
        public bool IsDisposed;
        public bool IsReadOnly;
        public string Method;
        public PostData_ PostData;
        public ReferrerPolicy ReferrerPolicy;
        public string ReferrerUrl;
        public ResourceType ResourceType;
        public TransitionType TransitionType;
        public string Url;

        public LTRequest()
        {
            Headers = new NameValueCollection();
        }

        public static LTRequest DeepCloneRequest(IRequest request)
        {
            //Debug.WriteLine(request.Url);
            //Misc
            LTRequest res = new LTRequest()
            {
                Flags = request.Flags,
                Identifier = request.Identifier,
                IsDisposed = request.IsDisposed,
                IsReadOnly = request.IsReadOnly,
                Method = request.Method,
                ReferrerPolicy = request.ReferrerPolicy,
                ReferrerUrl = request.ReferrerUrl,
                ResourceType = request.ResourceType,
                TransitionType = request.TransitionType,
                Url = request.Url
            };
            //Copy headers
            //res.Headers = new NameValueCollection();
            foreach (string key in request.Headers.AllKeys)
                foreach (string value in request.Headers.GetValues(key))
                {
                    res.Headers.Add(key, value);
                    //Debug.WriteLine(key + ": " + value);
                }

            //Copy PostData
            //Debug.WriteLine(res.Method);
            if (request.PostData == null)
                return res;

            res.PostData = new PostData_();
            IList<PostDataElement_> pdeIList = new List<PostDataElement_>();
            foreach (IPostDataElement pde in request.PostData.Elements)
            {
                PostDataElement_ ltpde = new PostDataElement_()
                {
                    File = pde.File,
                    Type = pde.Type,
                    IsReadOnly = pde.IsReadOnly
                };
                ltpde.Bytes = new byte[pde.Bytes.Length];
                Buffer.BlockCopy(pde.Bytes, 0, ltpde.Bytes, 0, pde.Bytes.Length);

                pdeIList.Add(ltpde);
            }
            res.PostData = new PostData_()
            {
                HasExcludedElements = request.PostData.HasExcludedElements,
                Elements = pdeIList,
                IsReadOnly = request.PostData.IsReadOnly,
                IsDisposed = request.PostData.IsDisposed
            };

            return res;
        }
        public static LTRequest DeepClone(LTRequest request)
        {
            //Misc
            LTRequest res = new LTRequest()
            {
                Flags = request.Flags,
                Identifier = request.Identifier,
                IsDisposed = request.IsDisposed,
                IsReadOnly = request.IsReadOnly,
                Method = request.Method,
                ReferrerPolicy = request.ReferrerPolicy,
                ReferrerUrl = request.ReferrerUrl,
                ResourceType = request.ResourceType,
                TransitionType = request.TransitionType,
                Url = request.Url
            };
            //Copy headers
            //res.Headers = new NameValueCollection();
            foreach (string key in request.Headers.AllKeys)
                foreach (string value in request.Headers.GetValues(key))
                {
                    res.Headers.Add(key, value);
                    Debug.WriteLine(key + ": " + value);
                }

            //Copy PostData
            Debug.WriteLine(res.Method);
            if (request.PostData == null)
                return res;

            res.PostData = new PostData_();
            IList<PostDataElement_> pdeIList = new List<PostDataElement_>();
            foreach (IPostDataElement pde in request.PostData.Elements)
            {
                PostDataElement_ ltpde = new PostDataElement_()
                {
                    File = pde.File,
                    Type = pde.Type,
                    IsReadOnly = pde.IsReadOnly
                };
                ltpde.Bytes = new byte[pde.Bytes.Length];
                Buffer.BlockCopy(pde.Bytes, 0, ltpde.Bytes, 0, pde.Bytes.Length);

                pdeIList.Add(ltpde);
            }
            res.PostData = new PostData_()
            {
                HasExcludedElements = request.PostData.HasExcludedElements,
                Elements = pdeIList,
                IsReadOnly = request.PostData.IsReadOnly,
                IsDisposed = request.PostData.IsDisposed
            };

            return res;
        }
        public static HttpRequestMessage ReplicateRequest(LTRequest request)
        {
            Debug.WriteLine(request.Url);
            
            bool hasData = request.PostData != null;
            string data = "";
            if (hasData)
                data = request.PostData.Elements[0].GetBody(request.GetCharSet());

            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(request.Url),
                Method = new HttpMethod(request.Method),
                Content = hasData ? new StringContent(data) : null
            };
            foreach (string header in request.Headers.AllKeys)
            {
                string val = request.Headers.GetValues(header)[0];
                if (header.Contains("Content"))
                {
                    httpRequest.Content.Headers.Remove(header);
                    httpRequest.Content.Headers.Add(header, val);
                    Debug.WriteLine(val);
                    continue;
                }
                httpRequest.Headers.Remove(header);
                httpRequest.Headers.Add(header, val);
            }
            if (request.ReferrerPolicy != ReferrerPolicy.NoReferrer)
                httpRequest.Headers.Add("Referer", request.ReferrerUrl);
            if (hasData && !request.Headers.AllKeys.Contains("Content-Length"))
                httpRequest.Content.Headers.Add("Content-Length", data.Length.ToString());
            Debug.WriteLine(hasData.ToString() + (!request.Headers.AllKeys.Contains("Content-Length")).ToString());
            return httpRequest;
        }

        public static void Serialize(object obj, string filepath)
        {
            BinaryFormatter f = new BinaryFormatter();
            f.Serialize(new FileStream(filepath, FileMode.Create), obj);
        }
        public static LTRequest Deserialize(string filepath)
        {
            BinaryFormatter f = new BinaryFormatter();
            return f.Deserialize(new FileStream(filepath, FileMode.Open)) as LTRequest;
        }
    }

    public class RequestInterceptArgs : EventArgs
    {
        public LTRequest Request;
        public bool isRequest;
        public RequestInterceptArgs(LTRequest request, bool isRequest)
        {
            this.Request = request;
            this.isRequest = isRequest;
        }
    }
    //public struct CustomCefResponse
    //{
    //    public bool @continue;
    //    public string url;
    //    public string mimeType;
    //    public string charset;
    //    public byte[] data;
    //    public bool exact;
    //}
    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        //Change response
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Cancel;
        }
    }

    public delegate void RequestIntercept(object sender, RequestInterceptArgs e);
    public class InterceptRequestHandler : CefSharp.Handler.RequestHandler
    {
        public List<string> uriBlock;

        public event RequestIntercept RequestIntercepted;
        //public List<CustomCefResponse> injectingResponses;
        public InterceptRequestHandler(params string[] uriBlock)
        {
            //Blocking uris starting with the given addresses
            this.uriBlock = uriBlock.ToList();
            //injectingResponses = new List<CustomCefResponse>();
        }

        //protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        //{
        //    Debug.WriteLine("OnBeforeBrowse");
        //    //RequestIntercept temp = Volatile.Read(ref RequestIntercepted);
        //    //if (temp != null)
        //    //    temp.BeginInvoke(this, new RequestInterceptArgs(LTRequest.deepCloneRequest(request), true), EndAsyncEvent, null);
        //    //Only intercept specific Url's
        //    foreach (string uri in uriBlock)
        //    {
        //        if (request.Url.Contains(uri))
        //            return true;
        //    }
        //    return false;
        //}
        //int GetInject(string url)
        //{
        //    if (injectingResponses.Count > 0)
        //        return -1;
        //    for (int i = 0; i < injectingResponses.Count; i++)
        //        if ((injectingResponses[i].exact & injectingResponses[i].url == url) |
        //                (!injectingResponses[i].exact & injectingResponses[i].url.Contains(url)))
        //            return i;
        //    return -1;
        //}
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            Debug.WriteLine($"GetResourceRequestHandler {request.Url}");

            RequestIntercept temp = Volatile.Read(ref RequestIntercepted);
            RequestInterceptArgs transformed = new RequestInterceptArgs(LTRequest.DeepCloneRequest(request), true);
            if (temp != null)
                Task.Run(() => temp.Invoke(this, transformed));

            //Only intercept specific Url's
            foreach (string uri in uriBlock)
            {
                if (request.Url.Contains(uri))
                {
                    return new CustomResourceRequestHandler();
                }
            }
            //int responseIndex = GetInject(request.Url);
            //if (responseIndex == -1)
            //    //Default behaviour, url will be loaded normally.
            //    return new CustomResourceRequestHandler();
            //var handler = new CustomResourceRequestHandler(injectingResponses[responseIndex]);
            //injectingResponses.RemoveAt(responseIndex);
            //return handler;
            return null;
        }

        void RemoveAllSubscribers()
        {
            foreach (Delegate d in RequestIntercepted.GetInvocationList())
            {
                RequestIntercepted -= (RequestIntercept)d;
            }
        }
    }
}
