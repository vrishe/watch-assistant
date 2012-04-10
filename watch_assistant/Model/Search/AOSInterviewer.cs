using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace watch_assistant.Model.Search
{
    class AOSInterviewer : IInterviewer
    {
        #region Fields

        private DataTable _interviewResult;
        private ResourceDictionary _dictionary;

        #endregion // Fields

        #region Constructors

        public AOSInterviewer()
        {
            _dictionary = new ResourceDictionary();
            _dictionary.Source = new Uri("..\\Resources\\resAOS.xaml", UriKind.Relative);
        }

        #endregion // Constructors

        #region IInterviewer implementation

        public void InterviewSite(string query)
        {
            // Create table and it's schema if it hasn't been done yet 
            if (_interviewResult == null)
                FormNewResultTable();

            // Try to get response from AOS server
            string answerContent = GetResponce(query, 1);
            // Find out how many results are found
            Match resNumDefinigMatch = Regex.Match(answerContent, 
                @"По\sВашему\sзапросу\sнайдено\s([0-9]*)\sответов\s\(Результаты\sзапроса\s1\s-\s([0-9]*)\)");
            if (String.IsNullOrEmpty(resNumDefinigMatch.Groups[1].ToString()))
                return;
            int resultsPages = (Int32)Math.Ceiling(Double.Parse(resNumDefinigMatch.Groups[1].ToString()) / Double.Parse(resNumDefinigMatch.Groups[2].ToString()));
            answerContent = answerContent.Substring(resNumDefinigMatch.Index + resNumDefinigMatch.Length);

            // Pick out every concern result
            GetResultsFromContent(query, answerContent);
            for (int page = 2; page <= resultsPages; page++)
                GetResultsFromContent(query, GetResponce(query, page));
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
                string videoItemBeginingString = "<div class='new_'>\r\n\t<div class='head_'><a href=\"([^\"]*)\"";
                Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
                if (!videoItemRef.Success) break;
                answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

                DataRow videoItem = _interviewResult.NewRow();
                videoItem["Name"] = Regex.Match(answerContent, @"<b>(.*)</b>").Groups[1];
                if (!((String)videoItem["Name"]).ToLower().Contains(query.ToLower())) continue;
                // If category is not Video then go to the next search result
                Match itemLocalMatch = Regex.Match(answerContent, @"\sКатегория:\s[^A-ZА-Я]*([^<]*)<");
                if (!itemLocalMatch.Groups[1].ToString().Contains("Аниме")) continue;
                videoItem["HRef"] = videoItemRef.Groups[1];
                videoItem["RussianAudio"] = (((String)videoItem["Name"]).Contains("(RUS)") ? true : false);
                videoItem["RussianSub"] = (((String)videoItem["Name"]).Contains("(SUB)") ? true : false);
                videoItem["Poster"] = Regex.Match(answerContent, "<div class='img_'><a href=\"([^\"]*)\"").Groups[1];
                videoItem["Ganre"] = Regex.Match(answerContent, "Жанр: ([^<]*)").Groups[1];
                itemLocalMatch = Regex.Match(answerContent, "style=\"color: [^>]*>([0-9]{4})<");
                videoItem["Year"] = Int32.Parse(itemLocalMatch.Groups[1].ToString());

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
            _interviewResult.Columns.Add("Genre", typeof(String));
            _interviewResult.Columns.Add("Year", typeof(Int32));
            _interviewResult.Columns.Add("Description", typeof(String));
            _interviewResult.Columns.Add("VideoQuality", typeof(String));
            _interviewResult.Columns.Add("RussianAudio", typeof(Boolean));
            _interviewResult.Columns.Add("RussianSub", typeof(Boolean));
        }

        private HttpWebResponse PostSearchQuery(String query, int requestTimeout, int page)
        {
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            headers.Add(new KeyValuePair<string,string>("Origin", (string)_dictionary["Origin"]));
            headers.Add(new KeyValuePair<string,string>("Cache-Control", (string)_dictionary["Cache-Control"]));
            headers.Add(new KeyValuePair<string,string>("Accept-Language", (string)_dictionary["Accept-Language"]));
            headers.Add(new KeyValuePair<string,string>("Accept-Charset", (string)_dictionary["Accept-Charset"]));
            headers.Add(new KeyValuePair<string,string>("Accept-Encoding", (string)_dictionary["Accept-Encoding"]));

            HttpWebRequest request = HttpQueryCreator.FormPostRequest(
                (string)_dictionary["Uri"],
                (string)_dictionary["ContentType"],
                (string)_dictionary["Accept"],
                (string)_dictionary["UserAgent"],
                headers,
                (int)_dictionary["Timeout"]
                );

            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ServicePoint.Expect100Continue = false;
            request.KeepAlive = true;           

            Byte[] byteArr;
            if (page > 1)
                //do=search&subaction=search&search_start=2&full_search=0&result_from=1&result_from=1&story=Bakuman
                byteArr = Encoding.GetEncoding(1251).GetBytes(
                    "do=search&subaction=search&search_start=" + 
                    page.ToString() + 
                    "&full_search=0&result_from=1&result_from=1&story=" + 
                    query.Replace(' ', '+') + 
                    "&x=1&y=1");
            else
                byteArr = Encoding.GetEncoding(1251).GetBytes("do=search&subaction=search&story=" + query.Replace(' ', '+') + "&x=1&y=1");
            request.GetRequestStream().Write(byteArr, 0, byteArr.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            request.ServicePoint.CloseConnectionGroup(request.ConnectionGroupName);
            GC.Collect();

            return response;
        }

        #endregion // Methods
    }
}
