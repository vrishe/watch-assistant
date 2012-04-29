using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace watch_assistant.Model.ExternalDataManager
{
    [Serializable]
    public struct ExternalUserRatingTableData
    {
        private ObservableCollection<DataTable> _userListsData;
        private Dictionary<string, double> _userRatingPrecomputedData;

        public ObservableCollection<DataTable> UserListsData { get { return _userListsData; } }
        public Dictionary<string, double> UserRatingPrecomputedData { get { return _userRatingPrecomputedData; } }

        public bool IsReady { get { return _userListsData != null && _userRatingPrecomputedData != null; } }

        public ExternalUserRatingTableData(IEnumerable<DataTable> listsTableSet, Dictionary<string, double> ratingSummary)
        {
            _userListsData = new ObservableCollection<DataTable>(listsTableSet);
            _userRatingPrecomputedData = ratingSummary;
        }
    }

    static class ExternalDataManager
    {
        public static void SaveUserTableData(string filePath)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
                ExternalUserRatingTableData userListsData = GetUserTableData();
                // ABUSIVE CODE
                foreach (DataTable table in userListsData.UserListsData) table.RemotingFormat = SerializationFormat.Binary;
                // ABUSIVE CODE END
                bf.Serialize(fs, userListsData);
            }
            finally
            {
                if ( fs != null ) fs.Close();
            }
        }

        public static bool LoadUserTableData(string filePath)
        {
            bool result = true;
            var userListsDataDeserializationResult = new ExternalUserRatingTableData();

            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));
                userListsDataDeserializationResult = (ExternalUserRatingTableData)bf.Deserialize(fs);

                OverrideUserTableData(userListsDataDeserializationResult);
            }
            catch (Exception) { }
            finally
            {
                if (fs != null) fs.Close();
            }
            return result && userListsDataDeserializationResult.IsReady;
        }

        public static void OverrideUserTableData(ExternalUserRatingTableData userListsData)
        {
            AppDomain.CurrentDomain.SetData("userListsTableData", userListsData);
        }

        public static ExternalUserRatingTableData GetUserTableData()
        {
            return (ExternalUserRatingTableData)AppDomain.CurrentDomain.GetData("userListsTableData");
        }
    }
}
