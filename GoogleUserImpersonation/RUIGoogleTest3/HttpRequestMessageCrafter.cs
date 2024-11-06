using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace Pet.RUIGoogle
{
    public static class HttpRequestMessageCrafter
    {
        //Update exampleHeaderValue with new targetHeaderValue and set values of targetHeaderValue to exampleHeaderValue
        public static void UpdateHeaderTemplate(HttpRequestMessage targetRequestMessage,
            string[,] templateHeaderValue)
        {
            List<KeyValuePair<string, IEnumerable<string>>> headersTemp = new List<KeyValuePair<string, IEnumerable<string>>>();
            List<KeyValuePair<string, IEnumerable<string>>> contentHeadersTemp = new List<KeyValuePair<string, IEnumerable<string>>>();

            bool hasContent = targetRequestMessage.Content != null;

            IEnumerable<string> targetHeaders = targetRequestMessage.Headers.Select(kvp => kvp.Key);
            IEnumerable<string> targetContentHeaders;
            if (hasContent)
                targetContentHeaders = targetRequestMessage.Content.Headers.Select(kvp => kvp.Key);
            else
                targetContentHeaders = new List<string>();
            

            foreach (string header in targetHeaders)
                System.Diagnostics.Debug.WriteLine(header);

            for (int i = 0; i < templateHeaderValue.GetLength(0); i++)
            {
                string templateHeader = templateHeaderValue[i, 0];

                bool needAdding = true;
                if (targetHeaders.Contains(templateHeader) | targetContentHeaders.Contains(templateHeader))
                    needAdding = false;
                System.Diagnostics.Debug.WriteLine("needAdding: " + needAdding);
                bool isHeadersContent = isRequestHeaderContent(templateHeader);
                if (!needAdding)
                    Console.WriteLine(templateHeader + "-------");

                IEnumerable<string> values;
                if (needAdding)
                {
                    string[] temp = templateHeaderValue[i, 1].Split(',');
                    for (int j = 0; j < temp.Length; j++)
                        temp[j] = temp[j].Trim();

                    values = temp;
                }
                else
                    if(isHeadersContent)
                        values = targetRequestMessage.Content.Headers.First(kvp => kvp.Key == templateHeader).Value;
                    else
                        values = targetRequestMessage.Headers.First(kvp => kvp.Key == templateHeader).Value;

                //Please do NOT use multiple User-Agent values in a single line
                if (templateHeader == "User-Agent")
                {
                    string concat = "";
                    foreach (string val in values)
                        concat += val + ", ";
                    concat = concat.Remove(concat.Length - 2);
                    values = new string[] { concat };
                }

                KeyValuePair<string, IEnumerable<string>> header = new KeyValuePair<string, IEnumerable<string>>(templateHeader, values);
                if(isHeadersContent)
                    contentHeadersTemp.Add(header);
                else
                    headersTemp.Add(header);
            }
            //DefaultHttpClient client;
            targetRequestMessage.Headers.Clear();
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in headersTemp)
                targetRequestMessage.Headers.Add(kvp.Key, kvp.Value);
                Debug.WriteLine("PASSED");
            if (!hasContent)
                return;
            targetRequestMessage.Content.Headers.Clear();
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in contentHeadersTemp)
                targetRequestMessage.Content.Headers.Add(kvp.Key, kvp.Value);
        }

        public static void UpdateHeaderTemplate(HttpResponseMessage targetResponseMessage,
            string[,] templateHeaderValue)
        {
            List<KeyValuePair<string, IEnumerable<string>>> headersTemp = new List<KeyValuePair<string, IEnumerable<string>>>();
            List<KeyValuePair<string, IEnumerable<string>>> contentHeadersTemp = new List<KeyValuePair<string, IEnumerable<string>>>();

            bool hasContent = targetResponseMessage.Content != null;

            IEnumerable<string> targetHeaders = targetResponseMessage.Headers.Select(kvp => kvp.Key);
            IEnumerable<string> targetContentHeaders;
            if (hasContent)
                targetContentHeaders = targetResponseMessage.Content.Headers.Select(kvp => kvp.Key);
            else
                targetContentHeaders = new List<string>();


            foreach (string header in targetHeaders)
                System.Diagnostics.Debug.WriteLine(header);

            for (int i = 0; i < templateHeaderValue.GetLength(0); i++)
            {
                string templateHeader = templateHeaderValue[i, 0];

                bool needAdding = true;
                if (targetHeaders.Contains(templateHeader) | targetContentHeaders.Contains(templateHeader))
                    needAdding = false;
                System.Diagnostics.Debug.WriteLine("needAdding: " + needAdding);
                bool isHeadersContent = isResponseHeaderContent(templateHeader);
                if (!needAdding)
                    Console.WriteLine(templateHeader + "-------");

                IEnumerable<string> values;
                if (needAdding)
                {
                    string[] temp = templateHeaderValue[i, 1].Split(',');
                    for (int j = 0; j < temp.Length; j++)
                        temp[j] = temp[j].Trim();
                    values = temp;
                }
                else
                    if (isHeadersContent)
                    values = targetResponseMessage.Content.Headers.First(kvp => kvp.Key == templateHeader).Value;
                else
                    values = targetResponseMessage.Headers.First(kvp => kvp.Key == templateHeader).Value;

                KeyValuePair<string, IEnumerable<string>> header = new KeyValuePair<string, IEnumerable<string>>(templateHeader, values);
                if (isHeadersContent)
                    contentHeadersTemp.Add(header);
                else
                    headersTemp.Add(header);
            }
            //DefaultHttpClient client;
            targetResponseMessage.Headers.Clear();
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in headersTemp)
                targetResponseMessage.Headers.Add(kvp.Key, kvp.Value);
            Debug.WriteLine("PASSED");
            if (!hasContent)
                return;
            targetResponseMessage.Content.Headers.Clear();
            foreach (KeyValuePair<string, IEnumerable<string>> kvp in contentHeadersTemp)
                targetResponseMessage.Content.Headers.Add(kvp.Key, kvp.Value);
        }
        static HttpRequestMessage req = new HttpRequestMessage();
        static HttpResponseMessage res = new HttpResponseMessage();
        static bool isRequestHeaderContent(string header)
        {
            try
            {
                req.Headers.Contains(header);
            }
            catch (Exception)
            {
                return true;
            }
            return false;
        }
        static bool isResponseHeaderContent(string header)
        {
            try
            {
                res.Headers.Contains(header);
            }
            catch (Exception)
            {
                return true;
            }
            return false;
        }
        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

            // Copy the request's content (via a MemoryStream) into the cloned object
            using (MemoryStream ms = new MemoryStream())
            {
                if (req.Content != null)
                {
                    await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                    ms.Position = 0;
                    clone.Content = new StreamContent(ms);

                    // Copy the content headers
                    if (req.Content.Headers != null)
                        foreach (var h in req.Content.Headers)
                            clone.Content.Headers.Add(h.Key, h.Value);
                }


                clone.Version = req.Version;

                foreach (KeyValuePair<string, object> prop in req.Properties)
                    clone.Properties.Add(prop);

                foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

                return clone;
            }
        }
    }
}
