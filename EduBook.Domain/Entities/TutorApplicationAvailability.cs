namespace EduBook.Domain.Entities;

public class TutorApplicationAvailability
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid TutorApplicationId { get; set; }

    public TutorApplication TutorApplication { get; set; }
        = null!;

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}