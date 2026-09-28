namespace EduBook.Domain.Entities;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();
}