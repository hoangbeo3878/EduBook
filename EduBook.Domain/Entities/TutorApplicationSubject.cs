namespace EduBook.Domain.Entities;

public class TutorApplicationSubject
{
    public Guid TutorApplicationId { get; set; }

    public TutorApplication TutorApplication { get; set; }
        = null!;

    public int SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;
}