using EduBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduBook.Domain.Entities
{
    public class StudentProfile
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }

        public ICollection<StudentSubject> PreferredSubjects { get; set; }
            = new List<StudentSubject>();
    }

    public class StudentSubject
    {
        public Guid StudentProfileId { get; set; }
        public StudentProfile StudentProfile { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
    }
}
