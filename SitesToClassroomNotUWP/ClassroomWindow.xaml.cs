using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SitesToClassroom.Classroom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SitesToClassroom
{
    /// <summary>
    /// Interaction logic for Classroom.xaml
    /// </summary>
    public partial class ClassroomWindow : Window
    {
        static string[] Scopes = { ClassroomService.Scope.ClassroomCourses };
        static string ApplicationName = "Google Sites to Google Classroom Transfer";
        private Logger logger = Logger.Instance;
        private Classroom.Course selectedCourse;

        public ClassroomWindow()
        {
            InitializeComponent();
        }

        private void CreateAssignments_Click(object sender, RoutedEventArgs e)
        {

            logger.Log($"Starting loading for {selectedCourse.CourseId}");
            ((Button)sender).IsEnabled = false;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UserCredential credential;
            logger.Log("Starting Assignement Create");

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                logger.Log("Credential file saved to: " + credPath);
            }

            // Create Classroom API service.
            var service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            CoursesResource.ListRequest request = service.Courses.List();
            request.PageSize = 10;

            // List courses.
            ListCoursesResponse response = request.Execute();

            // populate the results
            Courses courses = new Courses();
            this.CoursesDisplay.DataContext = courses.CourseEntries;

            logger.Log("Courses:");
            Wait.Visibility = Visibility.Hidden;
            Status.Visibility = Visibility.Visible;

            if (response.Courses != null && response.Courses.Count > 0)
            {
                foreach (var course in response.Courses)
                {
                    logger.Log($"{course.Name} ({course.Id})");
                    courses.Add(new Classroom.Course { CourseId = course.Id, CourseName = course.Name });
                }
                Status.Text = "Select a course and press Create";
            }
            else
            {
                logger.Log("No courses found.");
                Status.Text = "No courses found!";
            }
        }

        private void CoursesDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Status.Visibility = Visibility.Hidden;
            selectedCourse = (Classroom.Course)e.AddedItems[0];
            this.CreateAssignments.Visibility = Visibility.Visible;
        }
    }
}