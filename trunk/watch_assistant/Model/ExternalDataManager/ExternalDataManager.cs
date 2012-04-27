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
    internal class ExternalUserTableData
    {

    }

    static class ExternalDataManager
    {
        public static void SaveUserTableData(string filePath, Collection<DataTable> userLists, DataTable userChart)
        {
            FileStream fs = null;
            try
            {
                if (userLists == null || userChart == null) throw new ArgumentException("Some of the serialized objects seem to be of 'null or empty' state.");

                fs = new FileStream(filePath, FileMode.Create);

                BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));

                bf.Serialize(fs, userLists);
                bf.Serialize(fs, userChart);
            }
            finally
            {
                if ( fs != null ) fs.Close();
            }
        }

        public static void LoadUserTableData(string filePath, out Collection<DataTable> userLists, DataTable userChart)
        {
            object userListsDeserializationResult = null;
            object userChartDeserializationResult = null;

            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open);

                BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File));

                userListsDeserializationResult = bf.Deserialize(fs);
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

                userLists = (Collection<DataTable>)userListsDeserializationResult;
                userChart = (DataTable)userChartDeserializationResult;
            }
        }
    }
}
