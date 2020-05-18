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

        private Logger logger = Logger.Instance;
        private Classroom.Course selectedCourse;

        public ClassroomWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create assignments within the selected course
        /// </summary>
        private void CreateAssignments_Click(object sender, RoutedEventArgs e)
        {
            logger.Log($"Starting load of assignments for {selectedCourse.CourseId}");
            ((Button)sender).Visibility = Visibility.Collapsed;
            CoursesDisplay.Visibility = Visibility.Collapsed;
            Status.Text = "Please wait (this may take a while)";
            Status.Visibility = Visibility.Visible;

            if (App.SiteAssignments == null || App.SiteAssignments.Count == 0)
            {
                Status.Text = logger.Log("Unexpected no assignements in site extraction");
                return;
            }

            GoogleClassroomManager googleClassroomManager = new GoogleClassroomManager();
            googleClassroomManager.CreateEnded += GoogleClassroomManager_CreateEnded;
            googleClassroomManager.CreateAssignments(App.SiteAssignments, selectedCourse);
        }

        /// <summary>
        /// Raised when all create operations are completed
        /// </summary>
        private void GoogleClassroomManager_CreateEnded(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (System.Threading.SendOrPostCallback)delegate { CompleteCreate(); }, null);
        }

        /// <summary>
        /// reset UI and issue messages when complete
        /// </summary>
        private void CompleteCreate()
        {
            Status.Text = logger.Log("Completed assignment load");
            CloseButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Window loaded -- fetch the list of classes associcated with this user
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Log("Starting Assignment Create");

            // Create Classroom API service.
            var service = GoogleClassroomManager.CreateApiService();

            // Define request parameters.
            CoursesResource.ListRequest request = service.Courses.List();
            request.PageSize = 100;

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
                    logger.Log($"{course.Name} ({course.Id}, {course.CourseState}");
                    courses.Add(new Classroom.Course { CourseId = course.Id, CourseName = course.Name });
                }
                Status.Text = "Select a course and press Create";
            }
            else
            {
                Status.Text = logger.Log("No courses found.");
            }
        }

        private void CoursesDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Status.Visibility = Visibility.Hidden;
            selectedCourse = (Classroom.Course)e.AddedItems[0];
            this.CreateAssignments.Visibility = Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.CloseLogWindow();
            this.Close();
        }
    }
}