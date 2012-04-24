using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using watch_assistant.Properties;

namespace watch_assistant.Model.RaitingSystem
{
    // TODO: Распределить выполнение жанрового анализатора в программе (см. подробнее)
    // 1. В момент загрузки проги синхронизироваться с текущим рейтингом жанров
    // 2. В момент обновления списка фавов выполнить AssignGenresPriority
    // 3. Перед закрытием проги сериализовать текущий рейтинг жанров
    static class RaitingDBMS
    {
        #region RaitingAnalyzer
        internal static class RaitingAnalyzer
        {
            #region User genres manipulation
            /// <summary>
            /// Assign genre values in user raiting system for every genre from chart
            /// </summary>
            public static void AssignGenresPriority(DataTable chart)
            {
                _raitingSummary = new Dictionary<string, double>();
                foreach (DataRow chartRow in chart.Rows)
                    if ((Double)chartRow["Raiting"] > 0)
                        AssignGenrePriority(chartRow);
            }
            /// <summary>
            /// Assign genre values in user raiting system for every genre from chartRow
            /// </summary>
            private static void AssignGenrePriority(DataRow chartRow)
            {
                if (String.IsNullOrEmpty(chartRow["Genre"].ToString()))
                    return;

                double raiting = (chartRow["Raiting"] == DBNull.Value ? 0 : (double)chartRow["Raiting"]);

                foreach (string genre in chartRow["Genre"].ToString().Split(','))
                    if (_raitingSummary.ContainsKey(genre.Trim()))
                        _raitingSummary[genre.Trim()] += raiting;
                    else
                        _raitingSummary.Add(genre.Trim(), raiting);
            }
            #endregion (User genres manipulation)

            #region Input table items manipulation
            public static void AssignItemsPriority(DataTable table)
            {
                table.Columns.Add("Raiting", typeof(Double));
                foreach (DataRow tableRow in table.Rows)
                    AssignItemPriority(tableRow);
            }

            private static void AssignItemPriority(DataRow tableRow)
            {
                if (String.IsNullOrEmpty(tableRow["Genre"].ToString()))
                    return;

                foreach (string genre in tableRow["Genre"].ToString().Split(','))
                    if (_raitingSummary.ContainsKey(genre.Trim()))
                    {
                        double value = (tableRow["Raiting"] == DBNull.Value ? 0 : (double)tableRow["Raiting"]);
                        tableRow["Raiting"] = Math.Max(value, _raitingSummary[genre.Trim()]);
                    }
            }
            #endregion (Input table items manipulation)
        }
        #endregion (RaitingAnalyzer)

        #region Fields
        private static Dictionary<string, double> _raitingSummary;
        #endregion (Fields)

        #region Methods
        /// <summary>
        /// Synchronize with current user genres raiting system
        /// </summary>
        public static void Synchronize()
        {
            FileStream chartFileStream;
            if (!Directory.Exists(Settings.Default.GenreChartFilePath))
            {
                if (String.IsNullOrEmpty(Settings.Default.GenreChartFilePath))
                {
                    Settings.Default.GenreChartFilePath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "2AInc\\Watch assistant");
                    Settings.Default.Save();
                }
                Directory.CreateDirectory(Settings.Default.GenreChartFilePath);
            }
            string file = Path.Combine(Settings.Default.GenreChartFilePath, Settings.Default.GenreChartFileName);
            if (!File.Exists(file))
            {
                chartFileStream = File.Create(file);
                _raitingSummary = new Dictionary<string, double>();
                chartFileStream.Close();
                return;
            }
            chartFileStream = File.Open(file, FileMode.Open);

            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            _raitingSummary = (Dictionary<string, double>)bf.Deserialize(chartFileStream);
            chartFileStream.Close();
        }

        /// <summary>
        /// Provides a file serialization for raiting information dictionary.
        /// </summary>
        private static void SerializeRaitingSummary()
        {
            FileStream chartFileStream = File.Open(
                Path.Combine(Settings.Default.GenreChartFilePath, Settings.Default.GenreChartFileName),
                FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            bf.Serialize(chartFileStream, _raitingSummary);
            chartFileStream.Close();
        }
        #endregion (Methods)
    }
}
