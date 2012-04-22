using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace watch_assistant.Model.Search.IInterviewers
{
    class InterviewerBase : IInterviewer
    {
        #region Fields

        protected DataTable _interviewResult;

        private ResourceDictionary _dictionary = new ResourceDictionary();

        #endregion (Fields)

        #region Properties

        public int MaxAttempts { get; set; }

        #endregion (Properties)

        #region Constructors

        public InterviewerBase()
        {
            _dictionary.Source = new Uri(
                String.Format("..\\Resources\\res{0}.xaml", GetClassType()),
                UriKind.Relative);
            MaxAttempts = 5;
            FormNewResultTable();
        }

        #endregion (Constructors)

        #region IInterviewer implementation

        /// <summary>
        /// Gets DataTable with concern search results 
        /// </summary>
        public DataTable InterviewResult
        {
            get
            {
                if (_interviewResult != null)
                    return _interviewResult;
                _interviewResult = new DataTable(GetClassType());
                return _interviewResult;
            }
        }

        /// <summary>
        /// Fill InterviewResult DataTable with concern search results
        /// </summary>
        /// <param name="query">Strings for server to find</param>
        public virtual void ConductInterview(string[] queries)
        {   
            foreach (string query in queries)
            {
                // Do we need to interview server
                if (String.IsNullOrEmpty(query))
                    continue;

                // Try to get response from AOS server
                string answerContent = GetResponceContent(query, 1);
                // Find out how many results are found
                int resultsPages = GetResultsPages(ref answerContent);
                if (resultsPages == 0)
                    continue;

                // Pick out every concern result
                GetResultsFromContent(query, answerContent);
                for (int page = 2; page <= resultsPages; page++)
                    GetResultsFromContent(query, GetResponceContent(query, page));
            }
        }

        /// <summary>
        /// Clear past interviews results
        /// </summary>
        public void ClearInterviewResults()
        {
            _interviewResult = null;
            FormNewResultTable();
        }

        #endregion (IInterviewer implementation)

        #region Methods

        /// <summary>
        /// Fill InterviewResult DataTable with concern results
        /// </summary>
        /// <param name="query">A string that server used to send results</param>
        /// <param name="answerContent">An HTML based text content as a string from server responce on query</param>
        protected virtual void GetResultsFromContent(string query, string answerContent)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create new InterviewResult DataTable and assign it's schema
        /// </summary>
        protected void FormNewResultTable()
        {
            _interviewResult = new DataTable(GetClassType());

            _interviewResult.Columns.Add("Name", typeof(String));
            //_interviewResult.Columns.Add("HRef", typeof(String[]));
            _interviewResult.Columns.Add("Poster", typeof(String));
            _interviewResult.Columns.Add("Genre", typeof(String));
            _interviewResult.Columns.Add("Year", typeof(Int32));
            _interviewResult.Columns.Add("Description", typeof(String));
            //_interviewResult.Columns.Add("VideoQuality", typeof(String));
            //_interviewResult.Columns.Add("RussianAudio", typeof(Boolean));
            //_interviewResult.Columns.Add("RussianSub", typeof(Boolean));
            //_interviewResult.Columns.Add("Text", typeof(String[]));
            _interviewResult.Columns.Add("HRefs", typeof(List<KeyValuePair<string, string>>));
        }

        /// <summary>
        /// Get the number of pages needed to handle all results
        /// </summary>
        /// <param name="pageContent">HTML page content as a string</param>
        /// <returns></returns>
        protected virtual int GetResultsPages(ref string pageContent)
        {
            Match resNumDefinigMatch = Regex.Match(pageContent,
                @"По\sВашему\sзапросу\sнайдено\s([0-9]*)\sответов\s\(Результаты\sзапроса\s1\s-\s([0-9]*)\)");
            if (String.IsNullOrEmpty(resNumDefinigMatch.Groups[1].ToString()))
                return 0;
            int resultsPages = (Int32)Math.Ceiling(Double.Parse(resNumDefinigMatch.Groups[1].ToString()) / Double.Parse(resNumDefinigMatch.Groups[2].ToString()));
            pageContent = pageContent.Substring(resNumDefinigMatch.Index + resNumDefinigMatch.Length);

            return resultsPages;
        }

        /// <summary>
        /// Get an HTML based text content as a string from server responce 
        /// on user search query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page">Number of needed page of results</param>
        /// <returns></returns>
        protected virtual string GetResponceContent(string query, int page)
        {
            HttpWebResponse serverResponse = PostSearchQuery(query, page);

            // Pick out html page from server response if possible
            if (serverResponse == null)
                throw new WebException(String.Format("{0} server doesn't responce", GetClassType()));
            if (serverResponse.StatusCode != HttpStatusCode.OK)
                throw new WebException(
                    String.Format("{0} server doesn't responce as expected. Recieved StatusCode is {1}.",
                    GetClassType(), serverResponse.StatusCode.ToString()));
            String answerContent;
            using (Stream initStream = serverResponse.GetResponseStream())
            using (StreamReader answerContentStream = new StreamReader(initStream, System.Text.Encoding.GetEncoding(1251)))
                answerContent = answerContentStream.ReadToEnd();
            if (answerContent.Length <= 0)
                throw new WebException(
                    String.Format("{0} server hasn't returned any content.",
                    GetClassType()));

            return answerContent;
        }

        /// <summary>
        /// Send request to a server to get search results responce
        /// </summary>
        /// <param name="query">A string for server to find</param>
        /// <param name="page">Number of needed page of results</param>
        /// <returns></returns>
        protected virtual HttpWebResponse PostSearchQuery(string query, int page)
        {
            HttpWebRequest request = FormPostRequestHeader();

            Byte[] byteArr = FormPostRequestStream(query, page);

            request.ContentLength = byteArr.LongLength;
            request.GetRequestStream().Write(byteArr, 0, byteArr.Length);

            HttpWebResponse response = null;
            for (int i = 0; i < MaxAttempts; i++)
            {
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    break;
                }
                catch { }
            }
            if (request.ServicePoint.CurrentConnections == request.ServicePoint.ConnectionLimit)
            {
                request.ServicePoint.CloseConnectionGroup(request.ConnectionGroupName);
                GC.Collect();
            }

            return response;
        }

        /// <summary>
        /// Create and fill HttpWebRequest from attached dictionary
        /// </summary>
        /// <returns></returns>
        protected virtual HttpWebRequest FormPostRequestHeader()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create((string)_dictionary["Uri"]);
            request.Host = Regex.Match(request.RequestUri.ToString(), @"http://([^/]*)").Groups[1].ToString();
            request.Method = "POST";
            request.ContentType = (string)_dictionary["ContentType"];
            /*request.Accept = (string)_dictionary["Accept"];
            request.UserAgent = (string)_dictionary["UserAgent"];

            request.Headers.Add("Origin", (string)_dictionary["Origin"]);
            request.Headers.Add("Cache-Control", (string)_dictionary["Cache-Control"]);
            request.Headers.Add("Accept-Language", (string)_dictionary["Accept-Language"]);
            request.Headers.Add("Accept-Charset", (string)_dictionary["Accept-Charset"]);
            request.Headers.Add("Accept-Encoding", (string)_dictionary["Accept-Encoding"]);*/

            request.Timeout = (int)_dictionary["Timeout"];
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ServicePoint.Expect100Continue = false;
            request.KeepAlive = true;

            return request;
        }

        /// <summary>
        /// Create a byte array with formated server query
        /// </summary>
        /// <param name="query">String search query as a base for formating</param>
        /// <param name="page">Number of needed page of results</param>
        /// <returns></returns>
        protected virtual Byte[] FormPostRequestStream(string query, int page)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the caller Interviewer class prefix
        /// </summary>
        /// <returns></returns>
        private string GetClassType()
        {
            string childType = this.GetType().ToString();
            int childTypeStart = childType.LastIndexOf(".");
            int childTypeEnd = childType.LastIndexOf("Interviewer");
            return childType.Substring(childTypeStart + 1, childTypeEnd - childTypeStart - 1);
        }

        #endregion (Methods)
    }
}
