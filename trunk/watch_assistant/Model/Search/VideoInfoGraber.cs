using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using watch_assistant.Properties;

namespace watch_assistant.Model.Search
{
    static class VideoInfoGraber
    {
        /// <summary>
        /// Gets all missing data about video
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        public static void GetInfo(DataRow videoItem)
        {
            var tmp = ((Dictionary<string, string>)videoItem["HRefs"]).GetEnumerator();
            tmp.MoveNext();
            switch (GetServerName(tmp.Current.Key))
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
        /// Cache image from Url in videoItem and replace that http Url on local-based
        /// </summary>
        public static void CacheImage(DataRow videoItem)
        {
            if (videoItem["Poster"] != DBNull.Value && Uri.IsWellFormedUriString(videoItem["Poster"].ToString(), UriKind.Absolute))
            {
                string imgUri = videoItem["Poster"].ToString();
                string imgFile = Path.Combine(Settings.Default.DefaultAppFolderPath,
                                            "img", imgUri.Substring(imgUri.LastIndexOf('/') + 1));

                WebClient wc = new WebClient();
                wc.DownloadFile(imgUri, imgFile);
                //BitmapImage img = new BitmapImage(new Uri(imgFile));
                //result.Rows[0]["Poster"] = img;
                videoItem["Poster"] = imgFile;
            }
        }

        /// <summary>
        /// Gets a server name from the Uri
        /// </summary>
        public static string GetServerName(string uri)
        {
            return Regex.Match(uri, @"//([^\.]*)\.").Groups[1].ToString();
        }

        #region Description grabbing
        /// <summary>
        /// Gets an HTML page from server and trim it to the video item
        /// content from begining
        /// </summary>
        /// <param name="href">Uri to server</param>
        /// <param name="videoItemBegining">Regex match pattern</param>
        private static string GetInfoContent(string href, string videoItemBegining)
        {
            WebClient wc = new WebClient();
            string answerContent = wc.DownloadString(href);
            if (String.IsNullOrEmpty(answerContent))
                throw new WebException(String.Format("{0} server doesn't reply", GetServerName(href)));

            Match videoItemRef = Regex.Match(answerContent, videoItemBegining);
            if (!videoItemRef.Success)
                throw new WebException("Wrong URI");
            return answerContent.Substring(videoItemRef.Index + videoItemRef.Length);
        }

        /// <summary>
        /// Gets all missing data about video form animeonline.su
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromAOS(DataRow videoItem)
        {
            bool needYear = String.IsNullOrEmpty(videoItem["Year"].ToString()),
                 needDesc = String.IsNullOrEmpty(videoItem["Description"].ToString());
            if (!needYear && !needDesc)
                return;

            var enumerator = ((Dictionary<string, string>)videoItem["HRefs"]).GetEnumerator();
            enumerator.MoveNext();
            string answerContent = GetInfoContent(
                enumerator.Current.Key,
                "<div class='new_'>\r\n\t<div class='head_'><a href=\"([^\"]*)\"");

            if (needYear)
            {
                Match year = Regex.Match(answerContent, @"\[<a style=\x22color: [^>]*>([0-9]{4})</a>\]");
                try
                {
                    videoItem["Year"] = Int32.Parse(year.Groups[1].ToString());
                }
                catch { }
            }
            if (needDesc)
            {
                videoItem["Description"] =
                        Regex.Match(answerContent, "<p class=\"review\" align=\"justify\">([^<]*)</p>").Groups[1].ToString();
                if (String.IsNullOrEmpty((String)videoItem["Description"]))
                {
                    Match tmp = Regex.Match(
                        answerContent,
                        @"(?:<[^>]*>(?:\s)*)*?Описание:(?:(?:\s)*<[^>]*>(?:\s)*)*(.*?)</",
                        RegexOptions.Singleline);
                    videoItem["Description"] =
                        tmp.Groups[1].ToString();
                }

                videoItem["Description"] = FormTextFromHTML(videoItem["Description"].ToString());
            }
        }

        /// <summary>
        /// Gets all missing data about video from animesee.com
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromASee(DataRow videoItem)
        {
            bool needDesc = String.IsNullOrEmpty(videoItem["Description"].ToString());
            if (!needDesc)
                return;

            var enumerator = ((Dictionary<string, string>)videoItem["HRefs"]).GetEnumerator();
            enumerator.MoveNext();
            string answerContent = GetInfoContent(
                enumerator.Current.Key,
                "<div id='dle-content'><h1>Онлайн: ([^<]*)</h1>");

            if (needDesc)
            {
                videoItem["Description"] =
                    Regex.Match(answerContent, "<b>Описание:</b>(.*)</div>").Groups[1].ToString().Trim();
                videoItem["Description"] = FormTextFromHTML(videoItem["Description"].ToString());
            }
        }

        /// <summary>
        /// Gets all missing data about video from tvbest.com.ua
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromFilmin(DataRow videoItem)
        {
            bool needGenre = String.IsNullOrEmpty(videoItem["Description"].ToString());
            if (!needGenre)
                return;

            var enumerator = ((Dictionary<string, string>)videoItem["HRefs"]).GetEnumerator();
            enumerator.MoveNext();
            string answerContent = GetInfoContent(
                enumerator.Current.Key,
                "<div class=\"filminfo\"");

            if (needGenre)
                videoItem["Genre"] =
                    Regex.Match(answerContent, "<p><b>Жанр:</b>([^<]*)</p>").Groups[1].ToString().Trim();
        }

        /// <summary>
        /// Gets all missing data about video from filmin.ru
        /// </summary>
        /// <param name="videoItem">A video to grab info about</param>
        private static void GetInfoFromTVBest(DataRow videoItem)
        {
            bool needDesc = String.IsNullOrEmpty(videoItem["Description"].ToString());
            if (!needDesc)
                return;

            var enumerator = ((Dictionary<string, string>)videoItem["HRefs"]).GetEnumerator();
            enumerator.MoveNext();
            string answerContent = GetInfoContent(
                enumerator.Current.Key,
                "<div class=\"story_title\">");

            if (needDesc)
            {
                videoItem["Description"] =
                    Regex.Match(answerContent, "<!--TBegin--><a href=\"(?:[^\"]*)\" onclick=\"return hs.expand(this)\" ></a><!--TEnd-->([^<]*)<").Groups[1].ToString();
                videoItem["Description"] = FormTextFromHTML(videoItem["Description"].ToString());
            }
        }

        /// <summary>
        /// Returns a text with all tags and special HTML symbols
        /// replaced on their equivalent or deleted
        /// </summary>
        /// <param name="html">html based text</param>
        private static string FormTextFromHTML(string html)
        {
            html = ((String)html).Replace("<br />", "\n");
            html = ((String)html).Replace("</p>", "");
            html = ((String)html).Replace("&copy;", "");
            html = ((String)html).Replace("&hellip;", "...");
            html = ((String)html).Replace("&laquo;", "\"");
            html = ((String)html).Replace("&raquo;", "\"");
            html = ((String)html).Replace("&ndash;", "-");
            return html;
        }

        #endregion (Description grabbing)
    }
}
