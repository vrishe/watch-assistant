using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using watch_assistant.Model.ExternalDataManager;
using watch_assistant.Properties;
using System.IO;
using System;
using System.Collections.Generic;

namespace watch_assistant
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            // load here
            Collection<DataTable> tableKeeper;
            ExternalDataManager.LoadUserTableData(Path.Combine(Settings.Default.DefaultAppFolderPath, Settings.Default.UserAppDataFileName), out tableKeeper, null);

            if (tableKeeper == null)
            {
                // First-time run
                tableKeeper = UserDefaultListsInitialize();
            }

            AppDomain.CurrentDomain.SetData("userListsData", new ObservableCollection<DataTable>(tableKeeper));

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // save here
            var tableKeeper = (Collection<DataTable>)AppDomain.CurrentDomain.GetData("userListsData");
            ExternalDataManager.SaveUserTableData(Path.Combine(Settings.Default.DefaultAppFolderPath, Settings.Default.UserAppDataFileName), tableKeeper, null);

            base.OnExit(e);
        }

        private static void OverrideApplicationDataEnvironmentPath(string appFolderRelativePath)
        {
            if (!Directory.Exists(Settings.Default.DefaultAppFolderPath))
            {
                if (String.IsNullOrEmpty(Settings.Default.DefaultAppFolderPath))
                {
                    Settings.Default.DefaultAppFolderPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appFolderRelativePath);
                    Settings.Default.Save();
                }
                Directory.CreateDirectory(Settings.Default.DefaultAppFolderPath);
            }
        }

        private Collection<DataTable> UserDefaultListsInitialize()
        {
            var tableKeeper = new Collection<DataTable>();

            tableKeeper.Add(new DataTable() { TableName = "Favorites" });
            tableKeeper.Add(new DataTable() { TableName = "Interest" });

            return tableKeeper;
        }

        #endregion (Methods)

        #region Constructors

        static App()
        {
            OverrideApplicationDataEnvironmentPath(@"2AInc\watch_assistant");
        }

        #endregion (Constructors)
    }
}
