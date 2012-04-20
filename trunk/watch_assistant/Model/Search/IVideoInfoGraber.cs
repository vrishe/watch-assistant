using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace watch_assistant.Model.Search
{
    interface IVideoInfoGraber
    {
        /// <summary>
        /// Получает недостающие поля описания для объекта видео
        /// </summary>
        /// <param name="videoItem"></param>
        void GetInfo(DataRow videoItem);
    }
}
