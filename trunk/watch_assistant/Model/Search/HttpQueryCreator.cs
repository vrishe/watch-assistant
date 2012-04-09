using System.Collections.Generic;
using System.Net;

namespace watch_assistant.Model.Search
{
    static class HttpQueryCreator
    {
        static public HttpWebRequest FormPostRequest(
            string reqUri,
            string reqContentType,
            string reqAccept,
            string reqUserAgent,
            List<KeyValuePair<string, string>> reqHeaders,
            int reqTimeout
            )
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqUri);
            request.Method = "POST";
            request.ContentType = reqContentType;
            request.Accept = reqAccept;
            request.UserAgent = reqUserAgent;
            foreach (KeyValuePair<string, string> header in reqHeaders)
                request.Headers.Add(header.Key, header.Value);
            request.Timeout = reqTimeout;
            
            return request;
        }
    }
}
