using System;
using System.Data;
using System.IO;
using watch_assistant.Properties;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.AccessControl;

namespace watch_assistant.Model.RaitingAnalyzer
{
    static class RaitingAnalyzer
    {
        private static Dictionary<string, double> _genresChart;

        public static void AssignItemsPriority(DataTable table, DataTable chart) 
        {
            table.Columns.Add("Raiting", typeof(Double));

            _genresChart = GetGenresChart();

            foreach (DataRow chartRow in chart.Rows)
                if ((Double)chartRow["Raiting"] > 0)
                    AssignGenreWeight(chartRow);

            SerializeGenresChart();

            foreach (DataRow tableRow in table.Rows)
                AssignItemPriority(tableRow);
        }

        private static void AssignItemPriority(DataRow tableRow)
        {
            if (String.IsNullOrEmpty(tableRow["Genre"].ToString()))
                return;

            foreach (string genre in tableRow["Genre"].ToString().Split(','))
                if (_genresChart.ContainsKey(genre.Trim()))
                {
                    double value;
                    Double.TryParse(tableRow["Raiting"].ToString(),out value);
                    tableRow["Raiting"] = Math.Max(value, _genresChart[genre.Trim()]);
                }
        }

        private static void AssignGenreWeight(DataRow chartRow)
        {
            if (String.IsNullOrEmpty(chartRow["Genre"].ToString()))
                return;

            double raiting = (double)chartRow["Raiting"];

            foreach (string genre in chartRow["Genre"].ToString().Split(','))
                if (_genresChart.ContainsKey(genre.Trim()))
                    _genresChart[genre.Trim()] += raiting;
                else
                    _genresChart.Add(genre.Trim(), raiting);
        }

        private static Dictionary<string, double> GetGenresChart()
        {
            Dictionary<string, double> chart = new Dictionary<string, double>();
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
                chartFileStream.Close();
                return chart;   
            }
            chartFileStream = File.Open(file, FileMode.Open);

            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            chart = (Dictionary<string, double>)bf.Deserialize(chartFileStream);
            chartFileStream.Close();
            return chart;
        }

        private static void SerializeGenresChart()
        {
            FileStream chartFileStream = File.Open(
                Path.Combine(Settings.Default.GenreChartFilePath, Settings.Default.GenreChartFileName), 
                FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
            bf.Serialize(chartFileStream, _genresChart);
            chartFileStream.Close();
        }
    }
}
