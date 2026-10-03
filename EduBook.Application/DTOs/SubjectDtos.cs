namespace EduBook.Application.DTOs
{
    public record SubjectDto(
        int Id, 
        string Name,
        string? Description);
    public record CreateSubjectRequest(
        string Name,
        string? Description);
    public record UpdateSubjectRequest(
        string Name, 
        string? Description);
}