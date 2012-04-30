using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using watch_assistant.Model.ExternalDataManager;
using watch_assistant.Properties;
using System.IO;
using System;
using System.Collections.Generic;
using watch_assistant.Model.RatingSystem;

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
            if (!ExternalDataManager.LoadUserTableData(Path.Combine(Settings.Default.DefaultAppFolderPath, Settings.Default.UserAppDataFileName)))
            {
                string appDir = Settings.Default.DefaultAppFolderPath.Remove(Settings.Default.DefaultAppFolderPath.LastIndexOf('\\'));
                if (Directory.Exists(appDir)) Directory.Delete(appDir, true);
                string path = Path.Combine(Settings.Default.DefaultAppFolderPath, "img");
                ExternalDataManager.CreateImageFolder(path);
                ExternalDataManager.OverrideUserTableData(UserDefaultListsInitialize());
            }

            ExternalUserRatingTableData userListsData = ExternalDataManager.GetUserTableData();
            foreach (DataTable table in userListsData.UserListsData)
            {
                table.ColumnChanged += new DataColumnChangeEventHandler((sender, evt) =>
                {
                    if (evt.Column.ColumnName.Equals("Rating")) RatingDBMS.AssignGenresPriority(evt.Column.Table);
                });
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (!Directory.Exists(Settings.Default.DefaultAppFolderPath)) Directory.CreateDirectory(Settings.Default.DefaultAppFolderPath);
            ExternalDataManager.SaveUserTableData(Path.Combine(Settings.Default.DefaultAppFolderPath, Settings.Default.UserAppDataFileName));

            base.OnExit(e);
        }

        private ExternalUserRatingTableData UserDefaultListsInitialize()
        {
            var tableKeeper = new Collection<DataTable>();

            tableKeeper.Add(new DataTable() { TableName = "Favorites" });
            tableKeeper.Add(new DataTable() { TableName = "Interest" });             

            return new ExternalUserRatingTableData(tableKeeper, new Dictionary<string, double>());
        }

        #endregion (Methods)

        #region Constructors

        static App()
        {
            if (String.IsNullOrEmpty(Settings.Default.DefaultAppFolderPath))
            {
                Settings.Default.DefaultAppFolderPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"2AInc\watch_assistant");
                Settings.Default.Save();
            }
        }

        #endregion (Constructors)
    }
}
