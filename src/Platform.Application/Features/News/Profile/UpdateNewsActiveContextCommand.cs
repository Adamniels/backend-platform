namespace Platform.Application.Features.News.Profile;

public sealed record UpdateNewsActiveContextCommand(int UserId);

public enum UpdateNewsActiveContextResult
{
    Updated,
    NoProfile,
    Error,
}
