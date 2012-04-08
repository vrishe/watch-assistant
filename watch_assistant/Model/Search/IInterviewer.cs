using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace watch_assistant.Model.Search
{
    interface IInterviewer
    {
        DataTable InterviewResult { get; }

        /// <summary>
        /// Получает страницу ответа сайта на поисковый запрос.
        /// </summary>
        /// <param name="query">Строка для поиска на сайте</param>
        /// <returns>Получена ли страница ответа</returns>
        void InterviewSite(string query);

        /// <summary>
        /// Очищает результаты предыдущих опросов
        /// </summary>
        void ClearInterviewResults();
    }
}
