using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search.IInterviewers
{
    class TVBestInterviewer : InterviewerBase
    {
        #region IInterviewer implementation

        /// <summary>
        /// Fill InterviewResult DataTable with concern search results
        /// </summary>
        /// <param name="query">A string for server to find</param>
        public override void ConductInterview(string[] queries)
        {
            foreach (string query in queries)
            {
                // Do we need to interview server
                if (String.IsNullOrEmpty(query))
                    return;

                // Try to get response from AOS server
                string answerContent = GetResponceContent(query, 1);
                // Find out how many results are found
                int resultsPages = GetResultsPages(ref answerContent);
                if (resultsPages == 0)
                    return;
                if (resultsPages > 1)
                {
                    GetResponceContent(query, resultsPages * 100);
                    resultsPages = GetResultsPages(ref answerContent);
                }

                // Pick out every concern result
                GetResultsFromContent(query, answerContent);
                for (int page = 2; page <= resultsPages; page++)
                    GetResultsFromContent(query, GetResponceContent(query, page));
            }
        }

        #endregion (IInterviewer implementation)

        #region Methods

        /// <summary>
        /// Fill InterviewResult DataTable with concern results
        /// </summary>
        /// <param name="query">A string that server used to send results</param>
        /// <param name="answerContent">An HTML based text content as a string from server responce on query</param>
        protected override void GetResultsFromContent(string query, string answerContent)
        {
            do
            {
                string videoItemBeginingString = "<div class=\"story_title\">";
                answerContent = answerContent.Substring(answerContent.IndexOf(videoItemBeginingString) + videoItemBeginingString.Length);
                videoItemBeginingString = "<a href=\"([^\"]*)[^>]*>([^<]*)</a></div>";
                Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
                if (!videoItemRef.Success) break;
                if (!videoItemRef.Groups[2].ToString().ToLower().Contains(query.ToLower())) continue;

                DataRow videoItem = _interviewResult.NewRow();
                videoItem["HRef"] = new string[] { videoItemRef.Groups[1].ToString() };
                Match tmp = Regex.Match(videoItemRef.Groups[2].ToString(), @"(.*)\((([0-9]{4})\))\Z");
                videoItem["Name"] = tmp.Groups[1].ToString().Trim();
                videoItem["Year"] = Int32.Parse(tmp.Groups[3].ToString());
                videoItem["Text"] = new string[] { "RUS" };
                tmp = Regex.Match(answerContent, "<!--TBegin--><a href=\"([^\"]*).*<!--TEnd-->(.*)</div>");
                videoItem["Poster"] = tmp.Groups[1].ToString();
                videoItem["Description"] = tmp.Groups[2].ToString();
                answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

                Match current = Regex.Match(answerContent, @"<a href=[^>]*>([^<]*)</a>");
                Match end = Regex.Match(answerContent, @"<a href=[^>]*>([^<]*)</a><br />");
                videoItem["Genre"] = current.Groups[1].ToString();
                current = current.NextMatch();
                while (current.Groups[1].ToString() != end.Groups[1].ToString())
                {
                    current = current.NextMatch();
                    videoItem["Genre"] = videoItem["Genre"].ToString() + ", " + current.Groups[1].ToString();
                } 

                _interviewResult.Rows.Add(videoItem);
            }
            while (true);
        }

        /// <summary>
        /// Create a byte array with formated server query
        /// </summary>
        /// <param name="query">String search query as a base for formating</param>
        /// <param name="page">Number of needed page of results</param>
        /// <returns></returns>
        protected override Byte[] FormPostRequestStream(string query, int resNumber)
        {
            Byte[] byteArr = Encoding.GetEncoding(1251).GetBytes(
                    "do=search&subaction=search&x=0&y=0&story=" 
                    + query.Replace(' ', '+') +
                    "&result_num=" +
                    (resNumber / 100) * 100 + 100);

            return byteArr;
        }

        #endregion (Methods)
    }
}
