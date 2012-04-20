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
        /// Заполняет InterviewResult ответами сайта на поисковые запросы.
        /// </summary>
        /// <param name="query">Строки для поиска на сайте</param>
        void ConductInterview(string[] query);

        /// <summary>
        /// Очищает результаты предыдущих опросов
        /// </summary>
        void ClearInterviewResults();
    }
}
