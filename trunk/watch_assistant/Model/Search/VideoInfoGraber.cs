using System;
using System.Net;
using System.Text.RegularExpressions;

namespace watch_assistant.Model.Search
{
    static class VideoInfoGraber
    {
        /// <summary>
        /// Gets all missing data about video
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        public static void GetInfo(System.Data.DataRow videoItem)
        {
            string site = GetServerName(((String[])videoItem["HRef"])[0]);
            switch (site)
            {
                case "animeonline":
                    GetInfoFromAOS(videoItem);
                    break;
                case "animesee":
                    GetInfoFromASee(videoItem);
                    break;
                case "tvbest":
                    GetInfoFromTVBest(videoItem);
                    break;
                case "filmin":
                    GetInfoFromFilmin(videoItem);
                    break;
            }
        }

        /// <summary>
        /// Gets a server name from the Uri
        /// </summary>
        public static string GetServerName(string uri)
        {
            return Regex.Match(uri, @"//([^\.]*)\.").Groups[1].ToString();
        }

        /// <summary>
        /// Gets all missing data about video form animeonline.su
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromAOS(System.Data.DataRow videoItem)
        {
            WebClient wc = new WebClient();
            string answerContent = wc.DownloadString(((String[])videoItem["HRef"])[0]);
            if (String.IsNullOrEmpty(answerContent))
                throw new WebException("AOS server doesn't reply");

            string videoItemBeginingString = "<div class='new_'>\r\n\t<div class='head_'><a href=\"([^\"]*)\"";
            Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
            if (!videoItemRef.Success)
                throw new WebException("Wrong URI");
            answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

            if (String.IsNullOrEmpty(videoItem["Year"].ToString()))
            {
                Match year = Regex.Match(answerContent, @"\[<a style=\x22color: [^>]*>([0-9]{4})</a>\]");
                try
                {
                    videoItem["Year"] = Int32.Parse(year.Groups[1].ToString());
                }
                catch { }
            }
            if (String.IsNullOrEmpty(videoItem["Description"].ToString()))
            {
                videoItem["Description"] =
                    Regex.Match(answerContent, "(?:<[^>]*>)*Описание:(?:<[^>]*>)*<br />([^<]*)<br />").Groups[1].ToString();
                if (String.IsNullOrEmpty((String)videoItem["Description"]))
                    videoItem["Description"] =
                        Regex.Match(answerContent, "<p class=\"review\" align=\"justify\">([^<]*)</p>").Groups[1].ToString();
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&hellip;", "...");
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&laquo;", "\"");
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&raquo;", "\"");
            }
        }

        /// <summary>
        /// Gets all missing data about video from animesee.com
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromASee(System.Data.DataRow videoItem)
        {
            WebClient wc = new WebClient();
            string answerContent = wc.DownloadString(((String[])videoItem["HRef"])[0]);
            if (String.IsNullOrEmpty(answerContent))
                throw new WebException("ASee server doesn't reply");

            string videoItemBeginingString = "<div id='dle-content'><h1>Онлайн: ([^<]*)</h1>";
            Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
            if (!videoItemRef.Success)
                throw new WebException("Wrong URI");
            answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

            if (String.IsNullOrEmpty(videoItem["Description"].ToString()))
            {
                videoItem["Description"] =
                    Regex.Match(answerContent, "<b>Описание:</b>(.*)</div>").Groups[1].ToString().Trim();
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("<br />", "\n");
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&laquo;", "\"");
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&raquo;", "\"");
            }
        }

        /// <summary>
        /// Gets all missing data about video from tvbest.com.ua
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromTVBest(System.Data.DataRow videoItem)
        {
            WebClient wc = new WebClient();
            string answerContent = wc.DownloadString(((String[])videoItem["HRef"])[0]);
            if (String.IsNullOrEmpty(answerContent))
                throw new WebException("Filmin server doesn't reply");

            string videoItemBeginingString = "<div class=\"filminfo\">";
            Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
            if (!videoItemRef.Success)
                throw new WebException("Wrong URI");
            answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

            if (String.IsNullOrEmpty(videoItem["Genre"].ToString()))
                videoItem["Genre"] =
                    Regex.Match(answerContent, "<p><b>Жанр:</b>([^<]*)</p>").Groups[1].ToString().Trim();
        }

        /// <summary>
        /// Gets all missing data about video from filmin.ru
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromFilmin(System.Data.DataRow videoItem)
        {
            WebClient wc = new WebClient();
            string answerContent = wc.DownloadString(((String[])videoItem["HRef"])[0]);
            if (String.IsNullOrEmpty(answerContent))
                throw new WebException("TVBest server doesn't reply");

            string videoItemBeginingString = "<div class=\"story_title\">";
            Match videoItemRef = Regex.Match(answerContent, videoItemBeginingString);
            if (!videoItemRef.Success)
                throw new WebException("Wrong URI");
            answerContent = answerContent.Substring(videoItemRef.Index + videoItemRef.Length);

            if (String.IsNullOrEmpty(videoItem["Description"].ToString()))
            {
                videoItem["Description"] =
                    Regex.Match(answerContent, "<!--TBegin--><a href=\"(?:[^\"]*)\" onclick=\"return hs.expand(this)\" ></a><!--TEnd-->([^<]*)<").Groups[1].ToString();

                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&laquo;", "\"");
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&raquo;", "\"");
            }
        }
    }
}
