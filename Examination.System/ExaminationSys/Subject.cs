using System.Collections.Generic;

namespace ExaminationSystem
{
    public class Subject
    {
        public string Name { get; }
        public List<Student> Students { get; }

        public Subject(string name)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Students = new List<Student>();
        }

        public void AddStudent(Student student)
        {
            if (student == null)
                throw new System.ArgumentNullException(nameof(student));

            Students.Add(student);
        }
    }
}
