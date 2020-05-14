using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitesToClassroom.Classroom
{
    public class Courses
    {
        public ObservableCollection<Course> CourseEntries { get; set; } = new ObservableCollection<Course>();
        public void Add( Course course)
        {
            CourseEntries.Add(course);
        }
    }
}
