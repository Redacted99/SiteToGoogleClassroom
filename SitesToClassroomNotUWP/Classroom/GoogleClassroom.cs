using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SitesToClassroom.Classroom
{
    public static class GoogleClassroom
    {
        static string[] Scopes = { ClassroomService.Scope.ClassroomCourses, ClassroomService.Scope.ClassroomCourseworkStudents };
        static string ApplicationName = "Google Sites to Google Classroom Transfer";

        /// <summary>
        /// create the Google Classroom API service, login if necessary
        /// </summary>
        /// <returns>ClassroomService</returns>
        public static ClassroomService CreateApiService()
        {
            Logger logger = Logger.Instance; ;
            UserCredential credential;
            logger.Log("Starting Assignement Create");

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
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
            return service;
        }


        public static bool CreateAssignments(List<Sites.SiteAssignment> sitePages, Course selectedCourse)
        {
            var service = CreateApiService();
            foreach (var siteAssignment in sitePages)
            {
                CourseWork work = new CourseWork
                {
                    WorkType = "ASSIGNMENT",
                    CourseId = selectedCourse.CourseId,
                    Description = siteAssignment.Description,
                    Title = siteAssignment.Title,
                    Materials = new Material[] { new Material { Link = new Link { Url = siteAssignment.Url } } }.ToList(),
                    State = "DRAFT"
                };



                work = service.Courses.CourseWork.Create(work, selectedCourse.CourseId).Execute();
                Logger.Instance.Log($"Assignment {work.Title}[{work.Id}] created");
            }


            return false;
        }
    }
}
