﻿using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search
{
    class AOSInterviewer : InterviewerBase
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
                videoItem["Genre"] = Regex.Match(answerContent, "Жанр: ([^<]*)").Groups[1];
                itemLocalMatch = Regex.Match(answerContent, "style=\"color: [^>]*>([0-9]{4})<");
                videoItem["Year"] = Int32.Parse(itemLocalMatch.Groups[1].ToString());

                AddResultRow(videoItem);
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
                byteArr = Encoding.GetEncoding(1251).GetBytes("do=search&subaction=search&story=" + query.Replace(' ', '+') + "&x=1&y=1");

            return byteArr;
        }

        #endregion (Methods)
    }
}
