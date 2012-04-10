﻿using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search
{
    class FilminInterviewer : InterviewerBase
    {
        #region IInterviewer implementation

        /// <summary>
        /// Fill InterviewResult DataTable with concern search results
        /// </summary>
        /// <param name="query">A string for server to find</param>
        public override void InterviewSite(string query)
        {
            // Create table and it's schema if it hasn't been done yet 
            if (_interviewResult == null)
                FormNewResultTable();

            // Do we need to interview server
            if (String.IsNullOrEmpty(query))
                return;

            // Try to get response from AOS server
            string answerContent = GetResponceContent(query, 1);
            // Find out how many results are found
            int resultsPages = GetResultsPages(ref answerContent);
            if (resultsPages == 0)
                return;

            // Pick out every concern result
            GetResultsFromContent(query, answerContent);
            for (int page = 2; page <= resultsPages; page++)
                GetResultsFromContent(query, GetResponceContent(query, page));
        }

        #endregion (IInterviewer implementation)

        #region Methods

        /// <summary>
        /// Fill InterviewResult DataTable with concern results
        /// </summary>
        /// <param name="query">A string that server used to send results</param>
        /// <param name="answerContent">An HTML based text content as a string from server responce on query</param>
        private void GetResultsFromContent(string query, string answerContent)
        {
            do
            {
                string videoItemBeginingString = "<div class=\"sfr\">";
                answerContent = answerContent.Substring(answerContent.IndexOf(videoItemBeginingString) + videoItemBeginingString.Length);
                videoItemBeginingString = "<b><a href=\"([^\"]*)[^>]*>([^<]*)</a></b><br />";
                Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
                if (!videoItemRef.Success) break;
                if (!videoItemRef.Groups[2].ToString().ToLower().Contains(query.ToLower())) continue;

                DataRow videoItem = _interviewResult.NewRow();
                videoItem["HRef"] = videoItemRef.Groups[1];
                Match tmp = Regex.Match(videoItemRef.Groups[2].ToString(), @"(.*)\((([0-9]{4})\))\Z");
                videoItem["Name"] = tmp.Groups[1].ToString().Trim();
                videoItem["Year"] = Int32.Parse(tmp.Groups[3].ToString());
                videoItem["RussianAudio"] = true;
                videoItem["RussianSub"] = false;
                videoItem["Poster"] = "http://filmin.ru" + Regex.Match(answerContent, "<img src=\"([^\"]*)\"").Groups[1].ToString();
                videoItem["Description"] = Regex.Match(answerContent, "<b>Описание:</b>([^<]*)<").Groups[1].ToString().Trim();
                videoItem["Genre"] = "Unknown";

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
