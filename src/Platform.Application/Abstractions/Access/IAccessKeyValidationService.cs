namespace Platform.Application.Abstractions.Access;

public enum UnlockSessionOutcome
{
    NotConfigured,
    InvalidKey,
    Success,
}

public interface IAccessKeyValidationService
{
    /// <summary>Validates the provided key against platform configuration (constant-time when configured).</summary>
    UnlockSessionOutcome ValidateAccessKey(string? accessKey);
}
