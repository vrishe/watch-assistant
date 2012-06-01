using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search.IInterviewers
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
                string videoItemBeginingString = "<div class='new_'>\n\t<div class='head_'><a href=\"([^\"]*)\"";
                Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
                if (!videoItemRef.Success) break;
                answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

                DataRow videoItem = _interviewResult.NewRow();
                videoItem["Name"] = Regex.Match(answerContent, @"<b>(.*)</b>").Groups[1].ToString();
                if (!((String)videoItem["Name"]).ToLower().Contains(query.ToLower())) continue;
                // If category is not Video then go to the next search result
                Match category = Regex.Match(answerContent, @"\sКатегория:\s[^A-ZА-Я]*([^\|]*)\|");
                if (!category.Groups[1].ToString().Contains("Сериалы")) continue;
                Dictionary<string, string> list = new Dictionary<string, string>();
                int spare = ((String)videoItem["Name"]).LastIndexOf("(SUB)");
                if (spare >= 0)
                {
                    list.Add(videoItemRef.Groups[1].ToString(), "SUB");
                    videoItem["Name"] = ((String)videoItem["Name"]).Remove(spare, 5);
                }
                else
                {
                    spare = ((String)videoItem["Name"]).LastIndexOf("(RUS)");
                    list.Add(videoItemRef.Groups[1].ToString(), "DUB");
                    if (spare >= 0)
                        videoItem["Name"] = ((String)videoItem["Name"]).Remove(spare, 5).Trim();
                }
                videoItem["HRefs"] = list;
                videoItem["Poster"] = Regex.Match(answerContent, "<div class='img_'><a href=\"([^\"]*)\"").Groups[1].ToString();

                Match genre = Regex.Match(answerContent, @"Жанр:(?:\s?)(?:&nbsp;)?(?:<[^>]*>)?(?:\s?)(?:&nbsp;)?([а-яА-Я]+[^<]*)");
                string tmp = genre.Groups[1].ToString();
                if (String.IsNullOrEmpty(tmp) || genre.Index > category.Index)
                {
                    Match first = Regex.Match(answerContent, @"Жанр:(?:\s?)</strong>[^<]*<a href=[^>]*>([^<]*)</a>");
                    Match end = Regex.Match(answerContent, @"<a href=[^>]*>([^<]*)</a> <br />");
                    videoItem["Genre"] = first.Groups[1].ToString().Trim();

                    Regex currentPattern = new Regex("<a href=[^>]*>([^<]*)</a>");
                    MatchCollection genres = currentPattern.Matches(answerContent, first.Index + first.Length);
                    string last = end.Groups[1].ToString().Trim();
                    for (int i = 0; i < genres.Count; i++)
                    {
                        string current = genres[i].Groups[1].ToString().Trim();
                        if (genres[i].Index < end.Index && current != last)
                            videoItem["Genre"] = videoItem["Genre"].ToString() + ", " + current;
                        else break;
                    }
                    videoItem["Genre"] = videoItem["Genre"].ToString() + ", " + last;
                }
                else
                    videoItem["Genre"] = genre.Groups[1].ToString().Trim();

                Match year = Regex.Match(answerContent, "style=\"color: [^>]*>([0-9]{4})<");
                try
                {
                    videoItem["Year"] = Int32.Parse(year.Groups[1].ToString());
                }
                catch { }

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
