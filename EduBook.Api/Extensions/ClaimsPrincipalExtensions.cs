using System.Security.Claims;

namespace EduBook.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            // JWT thường map NameIdentifier
            raw ??= user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(raw) || !Guid.TryParse(raw, out var id))
                throw new UnauthorizedAccessException("Invalid user id in token.");

            return id;
        }
    }
}