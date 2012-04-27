using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search.IInterviewers
{
    class FilminInterviewer : InterviewerBase
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
                string videoItemBeginingString = "<div class=\"sfr\">";
                int i = answerContent.IndexOf(videoItemBeginingString);
                if (i >= 0) answerContent = answerContent.Substring(i + videoItemBeginingString.Length);
                else break;
                videoItemBeginingString = "<b><a href=\"([^\"]*)[^>]*>([^<]*)</a></b><br />";
                Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
                if (!videoItemRef.Success) break;
                if (!videoItemRef.Groups[2].ToString().ToLower().Contains(query.ToLower())) continue;

                DataRow videoItem = _interviewResult.NewRow();
                //videoItem["HRef"] = new string[] { videoItemRef.Groups[1].ToString() };
                Dictionary<string, string> list = new Dictionary<string, string>();
                list.Add(videoItemRef.Groups[1].ToString(), "DUB");
                videoItem["HRefs"] = list;
                Match tmp = Regex.Match(videoItemRef.Groups[2].ToString(), @"(.*)\((([0-9]{4})(?:\-[0-9]{4})?\))\Z");
                videoItem["Name"] = tmp.Groups[1].ToString().Replace("&amp;", "&").Trim();
                videoItem["Year"] = Int32.Parse(tmp.Groups[3].ToString());
                //videoItem["Text"] = new string[] { "DUB" };
                videoItem["Poster"] = "http://filmin.ru" + Regex.Match(answerContent, "<img src=\"([^\"]*)\"").Groups[1].ToString();
                videoItem["Description"] = Regex.Match(answerContent, "<b>Описание:</b>([^<]*)<").Groups[1].ToString().Trim();

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
                //do=search&subaction=search&search_start=2&full_search=0&result_from=1&result_from=1&story=Love
                byteArr = Encoding.GetEncoding(1251).GetBytes(
                    "do=search&subaction=search&search_start=" +
                    page.ToString() +
                    "&full_search=0&result_from=1&result_from=1&story=" +
                    query.Replace(' ', '+'));
            else
                byteArr = Encoding.GetEncoding(1251).GetBytes("do=search&subaction=search&story=" + query.Replace(' ', '+'));

            return byteArr;
        }

        #endregion (Methods)
    }
}
