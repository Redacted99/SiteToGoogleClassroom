using SitesToClassroom.Sites;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SitesToClassroom
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static LogWindow logWindow;
        private static readonly string PROPERTY_FILENAME = "migrateProperties";
        public static void CreateLogWindow()
        {
            logWindow = new LogWindow();
            logWindow.Show();
        }

        public static void CloseLogWindow()
        {
            logWindow.Close();
        }

        public static void GoToWindow( Window nextWindow, Window thisWindow)
        {
            thisWindow.Close();
            nextWindow.Show();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Restore application-scope property from isolated storage
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(PROPERTY_FILENAME, FileMode.Open, storage))
                using (StreamReader reader = new StreamReader(stream))
                {
                    // Restore each application-scope property individually
                    while (!reader.EndOfStream)
                    {
                        string[] keyValue = reader.ReadLine().Split(new char[] { ',' });
                        this.Properties[keyValue[0]] = keyValue[1];
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                // Handle when file is not found in isolated storage:
                // * When the first application session
                // * When file has been deleted
            }
        }

        public static List<SiteAssignment> SiteAssignments { get; set; }
        static App()
        {
            SiteAssignments = new List<SiteAssignment>();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            // Persist application-scope property to isolated storage
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(PROPERTY_FILENAME, FileMode.Create, storage))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                // Persist each application-scope property individually
                foreach (string key in this.Properties.Keys)
                {
                    writer.WriteLine("{0},{1}", key, this.Properties[key]);
                }
            }
        }
    }
}
