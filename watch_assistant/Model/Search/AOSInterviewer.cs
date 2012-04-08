using System;
using watch_assistant.Properties;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search
{
    class AOSInterviewer : IInterviewer
    {
        private DataTable _interviewResult;

        #region IInterviewer implementation

        public void InterviewSite(string query)
        {
            // Create table and it's schema if it hasn't been done yet 
            if (_interviewResult == null)
                FormNewResultTable();

            // Try to get response from AOS server
            // Pick out every concern result

            GetResultsFromContent(query, GetResponce(query, 1));
        }

        public void ClearInterviewResults()
        {
            _interviewResult = null;
        }

        public DataTable InterviewResult 
        { 
            get 
            {
                if (_interviewResult != null)
                    return _interviewResult;
                _interviewResult = new DataTable("AOS");
                return _interviewResult;
            } 
        }

        #endregion // IInterviewer implementation

        #region Methods

        private void GetResultsFromContent(string query, string answerContent)
        {
            do
            {
                {
                    String videoItemBeginingString = "<div class='new_'>\r\n\t<div class='head_'><a href=\"";
                    int i = answerContent.IndexOf(videoItemBeginingString);
                    if (i < 0) break;
                    answerContent = answerContent.Substring(i + videoItemBeginingString.Length);
                }

                DataRow videoItem = _interviewResult.NewRow();
                videoItem["Name"] = Regex.Match(answerContent, @"<b>(.*)</b>").Groups[1];
                if (!((String)videoItem["Name"]).Contains(query)) continue;
                // If category is not Video then go to the next search result
                var tmp = Regex.Match(answerContent, @"\sКатегория:\s[^A-ZА-Я]*([^<]*)<");
                if (!tmp.Groups[1].ToString().Contains("Аниме")) continue;
                videoItem["HRef"] = answerContent.Substring(0, answerContent.IndexOf("\" >"));
                videoItem["RussianAudio"] = (((String)videoItem["Name"]).Contains("(RUS)") ? true : false);
                videoItem["RussianSub"] = (((String)videoItem["Name"]).Contains("(SUB)") ? true : false);
                videoItem["Poster"] = Regex.Match(answerContent, "<div class='img_'><a href=\"([^\"]*)\"").Groups[1];
                videoItem["Ganre"] = Regex.Match(answerContent, "Жанр: ([^<]*)").Groups[1];
                tmp = Regex.Match(answerContent, "style=\"color: [^>]*>([0-9]{4})<");
                videoItem["Year"] = Int32.Parse(tmp.Groups[1].ToString());

                _interviewResult.Rows.Add(videoItem);
            }
            while (true);
        }

        private String GetResponce(string query, int page)
        {
            HttpWebResponse serverResponse = PostSearchQuery(query, 1000, page);

            // Pick out html page from server response if possible
            if (serverResponse.StatusCode != HttpStatusCode.OK)
                throw new WebException(
                    String.Format("AOS server doesn't responce as expected. Recieved StatusCode is {0}.",
                    serverResponse.StatusCode.ToString()));
            String answerContent;
            using (Stream initStream = serverResponse.GetResponseStream())
            using (StreamReader answerContentStream = new StreamReader(initStream, System.Text.Encoding.GetEncoding(serverResponse.CharacterSet)))
                answerContent = answerContentStream.ReadToEnd();
            if (answerContent.Length <= 0)
                throw new WebException("AOS server hasn't returned any content.");

            return answerContent;
        }

        private void FormNewResultTable()
        {
            _interviewResult = new DataTable("AOS");

            _interviewResult.Columns.Add("Name", typeof(String));
            _interviewResult.Columns.Add("HRef", typeof(String));
            _interviewResult.Columns.Add("Poster", typeof(String));
            _interviewResult.Columns.Add("Ganre", typeof(String));
            _interviewResult.Columns.Add("Year", typeof(Int32));
            _interviewResult.Columns.Add("Description", typeof(String));
            _interviewResult.Columns.Add("VideoQuality", typeof(String));
            _interviewResult.Columns.Add("RussianAudio", typeof(Boolean));
            _interviewResult.Columns.Add("RussianSub", typeof(Boolean));
        }

        private HttpWebResponse PostSearchQuery(String searchLine, int requestTimeout, int page)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://animeonline.su/");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.77 Safari/535.7";
            request.Headers.Add("Origin", request.RequestUri.ToString());
            request.Headers.Add("Cache-Control", "max-age=0");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            request.Headers.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.3");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ServicePoint.Expect100Continue = false;
            request.KeepAlive = true;

            request.Timeout = requestTimeout;

            Byte[] byteArr;
            if (page > 1)
                //do=search&subaction=search&search_start=2&full_search=0&result_from=1&result_from=1&story=Bakuman
                byteArr = Encoding.GetEncoding(1251).GetBytes(
                    "do=search&subaction=search&search_start=" + 
                    page.ToString() + 
                    "&full_search=0&result_from=1&result_from=1&story=" + 
                    searchLine.Replace(' ', '+') + 
                    "&x=1&y=1");
            else
                byteArr = Encoding.GetEncoding(1251).GetBytes("do=search&subaction=search&story=" + searchLine.Replace(' ', '+') + "&x=1&y=1");
            request.GetRequestStream().Write(byteArr, 0, byteArr.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            request.ServicePoint.CloseConnectionGroup(request.ConnectionGroupName);
            GC.Collect();

            return response;
        }

        #endregion // Methods
    }
}
