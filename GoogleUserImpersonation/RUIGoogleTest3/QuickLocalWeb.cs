using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pet.RUIGoogle
{
    public class QuickLocalWeb
    {
        // To enable this so that it can be run in a non-administrator account:
        // Open an Administrator command prompt.
        // netsh http add urlacl http://+:8008/ user=Everyone listen=true

        const string Prefix = "http://+:{0}/";
        string content = "<HTML><BODY> Hello world!</BODY></HTML>";
        public HttpListener Listener = null;
        public int port;

        public QuickLocalWeb()
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("HttpListener is not supported on this platform.");
                return;
            }
            Listener = new HttpListener();
            Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            if (!TryBindListenerOnFreePort(out Listener, out port))
                throw new Exception("No free port available!");

            StartWeb();
        }
        
        async void StartWeb()
        {
            var context = await Listener.GetContextAsync();
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = content;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();

            Listener.Stop();
            Listener.Close();
        }

        public static bool TryBindListenerOnFreePort(out HttpListener httpListener, out int port)
        {
            // IANA suggested range for dynamic or private ports
            const int MinPort = 49215;
            const int MaxPort = 65535;

            for (port = MinPort; port < MaxPort; port++)
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(string.Format(Prefix, port));
                try
                {
                    httpListener.Start();
                    return true;
                }
                catch
                {
                    // nothing to do here -- the listener disposes itself when Start throws
                }
            }

            port = 0;
            httpListener = null;
            return false;
        }
    }
}
