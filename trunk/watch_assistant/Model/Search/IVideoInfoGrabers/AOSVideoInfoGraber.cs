using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Data;

namespace watch_assistant.Model.Search.IVideoInfoGrabers
{
    class AOSVideoInfoGraber : IVideoInfoGraber
    {
        #region IVideoInfoGraber implementation

        public void GetInfo(DataRow videoItem)
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
                    Regex.Match(answerContent, "<b>Описание:</b><br />([^<]*)<br />").Groups[1].ToString();
                if (String.IsNullOrEmpty((String)videoItem["Description"]))
                    videoItem["Description"] =
                        Regex.Match(answerContent, "<p class=\"review\" align=\"justify\">([^<]*)</p>").Groups[1].ToString();
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&laquo;", "\"");
                videoItem["Description"] = ((String)videoItem["Description"]).Replace("&raquo;", "\"");
            }
        }

        #endregion IVideoInfoGraber implementation
    }
}
