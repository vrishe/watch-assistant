﻿using System;
using System.Collections.Generic;
using System.Data;

namespace watch_assistant.Model.RatingSystem
{
    static class RatingDBMS
    {
        #region RatingAnalyzer

        #region User genres manipulation
        /// <summary>
        /// Assign genre values in user Rating system for every genre from chart
        /// </summary>
        public static void AssignGenresPriority(DataTable chart)
        {
            Dictionary<string, double> _RatingSummary = ExternalDataManager.ExternalDataManager.GetUserTableData().UserRatingPrecomputedData;
            _RatingSummary.Clear();
            foreach (DataRow chartRow in chart.Rows)
                if ((Double)chartRow["Rating"] > 0)
                    AssignGenrePriority(chartRow, _RatingSummary);
        }
        /// <summary>
        /// Assign genre values in user Rating system for every genre from chartRow
        /// </summary>
        private static void AssignGenrePriority(DataRow chartRow, Dictionary<string, double> _RatingSummary)
        {
            if (String.IsNullOrEmpty(chartRow["Genre"].ToString()))
                return;

            double Rating = (chartRow["Rating"] == DBNull.Value ? 0 : (double)chartRow["Rating"]);

            foreach (string genre in chartRow["Genre"].ToString().Split(','))
                if (_RatingSummary.ContainsKey(genre.Trim()))
                    _RatingSummary[genre.Trim()] += Rating;
                else
                    _RatingSummary.Add(genre.Trim(), Rating);
        }
        #endregion (User genres manipulation)

        #region Input table items manipulation
        public static void AssignItemsPriority(DataTable table)
        {
            table.Columns.Add("Rating", typeof(Double));
            Dictionary<string, double> _RatingSummary = ExternalDataManager.ExternalDataManager.GetUserTableData().UserRatingPrecomputedData;
            if (_RatingSummary.Count == 0)
            {
                foreach (DataRow tableRow in table.Rows)
                    tableRow["Rating"] = 0;
                return;
            }
            foreach (DataRow tableRow in table.Rows)
                AssignItemPriority(tableRow, _RatingSummary);
        }

        private static void AssignItemPriority(DataRow tableRow, Dictionary<string, double> _RatingSummary)
        {
            if (String.IsNullOrEmpty(tableRow["Genre"].ToString()))
                return;

            foreach (string genre in tableRow["Genre"].ToString().Split(','))
                if (_RatingSummary.ContainsKey(genre.Trim()))
                {
                    double value = (tableRow["Rating"] == DBNull.Value ? 0 : (double)tableRow["Rating"]);
                    tableRow["Rating"] = value + _RatingSummary[genre.Trim()];
                }
        }
        #endregion (Input table items manipulation)
        #endregion (RatingAnalyzer)

        //#region Fields
        //private static Dictionary<string, double> _RatingSummary;
        //#endregion (Fields)

        #region Methods
        /// <summary>
        /// Synchronize with current user genres Rating system
        /// </summary>
        public static void Synchronize()
        { }

        /// <summary>
        /// Provides a file serialization for Rating information dictionary.
        /// </summary>
        //private static void SerializeRatingSummary()
        //{
        //    FileStream chartFileStream = File.Open(
        //        Path.Combine(Settings.Default.DefaultAppFolderPath, Settings.Default.UserAppDataFileName),
        //        FileMode.Create);
        //    BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
        //    bf.Serialize(chartFileStream, _RatingSummary);
        //    chartFileStream.Close();
        //}
        #endregion (Methods)
    }
}
