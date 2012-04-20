using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Data;

namespace watch_assistant.Model.Search.IVideoInfoGrabers
{
    class FilminVideoInfoGraber : IVideoInfoGraber
    {
        #region IVideoInfoGraber implementation

        public void GetInfo(DataRow videoItem)
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

        #endregion IVideoInfoGraber implementation
    }
}
