using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using watch_assistant.Properties;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace watch_assistant.Model.ExternalDataManager
{
    [Serializable]
    public struct ExternalUserRatingTableData
    {
        private ObservableCollection<DataTable> _userListsData;
        private DataTable _userRatingPrecomputedData;

        public ObservableCollection<DataTable> UserListsData { get { return _userListsData; } }
        public DataTable UserRatingPrecomputedData { get { return _userRatingPrecomputedData; } }

        public bool IsReady { get { return _userListsData != null && _userRatingPrecomputedData != null; } }

        public ExternalUserRatingTableData(IEnumerable<DataTable> listsTableSet, DataTable ratingTable)
        {
            _userListsData = new ObservableCollection<DataTable>(listsTableSet);
            _userRatingPrecomputedData = ratingTable;
        }
    }

    static class ExternalDataManager
    {
        public static void SaveUserTableData(string filePath, ExternalUserRatingTableData userListsData)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create);

                BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));

                bf.Serialize(fs, userListsData);
            }
            finally
            {
                if ( fs != null ) fs.Close();
            }
        }

        public static void LoadUserTableData(string filePath, out ExternalUserRatingTableData userListsData)
        {
            var userListsDataDeserializationResult = new ExternalUserRatingTableData();

            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open);

                BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));

                userListsDataDeserializationResult = (ExternalUserRatingTableData)bf.Deserialize(fs);
            }
            catch (IOException) 
            {
                #if DEBUG
                Debug.WriteLine("No user data file detected. First-time run takes place.");
                #endif
            }
            finally
            {
                if (fs != null) fs.Close();

                userListsData = userListsDataDeserializationResult;
            }
        }
    }
}
