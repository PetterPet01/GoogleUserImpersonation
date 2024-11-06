using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace Pet.RUIGoogle
{
    public class ConditionedResponse
    {
        public Func<Request, bool> evaluation;
        public HttpResponseMessage response;
        public bool oneTime;

        public ConditionedResponse(Func<Request, bool> evaluation, HttpResponseMessage response,
            bool oneTime)
        {
            this.evaluation = evaluation;
            this.response = response;
            this.oneTime = oneTime;
        }
    }
    public class Proxy
    {
        ProxyServer proxyServer;
        int port = 8003;
        bool blockAllOtherRequests;

        public List<ConditionedResponse> autoRespond;

        public async static Task<Titanium.Web.Proxy.Http.Response> ToResponse(HttpResponseMessage response)
        {
            bool hasContent = response.Content != null;
            byte[] data = null;
            if (hasContent)
                data = await response.Content.ReadAsByteArrayAsync();

            Titanium.Web.Proxy.Http.Response result = new Titanium.Web.Proxy.Http.Response(data)
            {
                HttpVersion = response.Version,
                StatusCode = (int)response.StatusCode,
            };

            foreach (KeyValuePair<string, IEnumerable<string>> kvp in response.Headers)
                foreach (string value in kvp.Value)
                {
                    
                    result.Headers.AddHeader(kvp.Key, value);
                    //Debug.WriteLine($"{kvp.Key}: {value}");
                }
            if (hasContent)
                foreach (KeyValuePair<string, IEnumerable<string>> kvp in response.Content.Headers)
                    foreach (string value in kvp.Value)
                    {
                        //TODO: FIX THIS - Content-Length repeats itself twice
                        if (kvp.Key == "Content-Length")
                            continue;
                        result.Headers.AddHeader(kvp.Key, value);
                        //Debug.WriteLine($"{kvp.Key}: {value}");
                    }

            Debug.WriteLine(result.HeaderText);

            return result;
        }

        public void RegisterResponse(Func<Request, bool> evaluation, HttpResponseMessage response, bool oneTime = false)
        {
            ConditionedResponse cr = new ConditionedResponse(evaluation, response, oneTime);
            autoRespond.Add(cr);
        }

        private string LocalIPAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }

        public string Initialize(bool blockAllOtherRequests = false)
        {
            proxyServer = new ProxyServer();
            autoRespond = new List<ConditionedResponse>();
            this.blockAllOtherRequests = blockAllOtherRequests;

            // locally trust root certificate used by this proxy 
            //proxyServer.CertificateManager.TrustRootCertificate(true);

            // optionally set the Certificate Engine
            // Under Mono only BouncyCastle will be supported
            //proxyServer.CertificateManager.CertificateEngine = Network.CertificateEngine.BouncyCastle;

            proxyServer.BeforeRequest += OnRequest;
            //proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            //proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, port, true)
            {
                // Use self-issued generic certificate on all https requests
                // Optimizes performance by not creating a certificate for each https-enabled domain
                // Useful when certificate trust is not required by proxy clients
                //GenericCertificate = new X509Certificate2(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "genericcert.pfx"), "password")
            };
            // An explicit endpoint is where the client knows about the existence of a proxy
            // So client sends request in a proxy friendly manner
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();
            return LocalIPAddress() + $":{port}";
        }

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Proxying: " + e.HttpClient.Request.Url);

            foreach (ConditionedResponse cr in autoRespond)
                if (cr.evaluation(e.HttpClient.Request))
                {
                    e.Respond(await ToResponse(cr.response));
                    if (cr.oneTime)
                        autoRespond.Remove(cr);
                }
            if (blockAllOtherRequests)
                e.Ok("Blocked");
        }

        public void Stop()
        {
            proxyServer.Stop();
        }
        public void Dispose()
        {
            proxyServer.Stop();
            proxyServer.Dispose();
        }
    }
}
