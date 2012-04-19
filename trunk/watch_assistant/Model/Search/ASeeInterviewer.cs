using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search
{
    class ASeeInterviewer : InterviewerBase
    {
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
                string videoItemBeginingString = "<h2><a href=\"([^\"]*)\"";
                Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
                if (!videoItemRef.Success) break;
                answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

                DataRow videoItem = _interviewResult.NewRow();
                Match itemLocalMatch = Regex.Match(answerContent, @">(.*)</a></h2>");
                videoItem["Name"] = itemLocalMatch.Groups[1].ToString();
                if (!((String)videoItem["Name"]).ToLower().Contains(query.ToLower())) continue;
                // If category is not Video then go to the next search result
                if (!((String)videoItem["Name"]).Contains("Онлайн: ")) continue;
                else videoItem["Name"] = ((string)videoItem["Name"]).Substring(8);
                videoItem["HRef"] = new string[] { videoItemRef.Groups[1].ToString() };
                //videoItem["RussianAudio"] = (((String)videoItem["Name"]).Contains("(RUS)") ? true : false);
                if (((String)videoItem["Name"]).Contains("(SUB)"))
                    videoItem["Text"] = new string[] { "SUB" };
                else
                    videoItem["Text"] = new string[] { "RUS" };
                videoItem["Poster"] = Regex.Match(answerContent, "<!--TBegin--><a href=\"([^\"]*)\"").Groups[1].ToString();
                if (String.IsNullOrEmpty(videoItem["Poster"].ToString()))
                    videoItem["Poster"] = Regex.Match(answerContent, @"<!--dle_image_begin:([^\|]*)\|").Groups[1].ToString();
                videoItem["Genre"] = Regex.Match(answerContent, @"<b>Жанр:\s*</b>\s*([^<]*)").Groups[1].ToString();
                Match year = Regex.Match(answerContent, "<!--/colorstart-->[^0-9]*([0-9]{4})[^0-9]*<!--colorend-->");
                if (year.Success)
                    videoItem["Year"] = Int32.Parse(year.Groups[1].ToString());

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
        protected override Byte[] FormPostRequestStream(string query, int page)
        {
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
                byteArr = Encoding.GetEncoding(1251).GetBytes("do=search&subaction=search&story=" + query.Replace(' ', '+'));

            return byteArr;
        }

        #endregion (Methods)
    }
}
