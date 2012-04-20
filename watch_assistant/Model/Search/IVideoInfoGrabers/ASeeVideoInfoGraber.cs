using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Data;

namespace watch_assistant.Model.Search.IVideoInfoGrabers
{
    class ASeeVideoInfoGraber : IVideoInfoGraber
    {
        #region IVideoInfoGraber implementation

        public void GetInfo(DataRow videoItem)
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

        #endregion IVideoInfoGraber implementation
    }
}
