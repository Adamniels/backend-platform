using FluentValidation;
using Platform.Application.Abstractions.News;
using Platform.Application.Features.News.RecordInteraction;
using Platform.Domain.Features.News;

namespace Platform.UnitTests;

public sealed class RecordNewsInteractionCommandHandlerTests
{
    // ---------------------------------------------------------------------------
    // Test doubles
    // ---------------------------------------------------------------------------

    private sealed class CapturingInteractionRepository : INewsInteractionRepository
    {
        public NewsInteraction? Inserted { get; private set; }

        public Task InsertAsync(NewsInteraction interaction, CancellationToken cancellationToken = default)
        {
            Inserted = interaction;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<NewsInteraction>> GetRecentAsync(
            int userId,
            DateTimeOffset since,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<NewsInteraction>>([]);

        public Task<double?> GetAverageDwellSecondsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<double?>(null);
    }

    private static readonly string ValidId = "ni-" + new string('b', 32);

    private static RecordNewsInteractionCommandHandler MakeHandler(
        CapturingInteractionRepository? repo = null) =>
        new(new RecordNewsInteractionCommandValidator(), repo ?? new CapturingInteractionRepository());

    // ---------------------------------------------------------------------------
    // Happy-path tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Inserts_read_interaction_with_correct_fields()
    {
        var repo = new CapturingInteractionRepository();
        var handler = MakeHandler(repo);

        await handler.HandleAsync(
            new RecordNewsInteractionCommand(1, ValidId, "read", 45));

        Assert.NotNull(repo.Inserted);
        Assert.Equal(1, repo.Inserted!.UserId);
        Assert.Equal(ValidId, repo.Inserted.NewsItemId);
        Assert.Equal(NewsInteractionType.Read, repo.Inserted.Type);
        Assert.Equal(45, repo.Inserted.DwellSeconds);
    }

    [Fact]
    public async Task Inserts_save_interaction_with_null_dwell()
    {
        var repo = new CapturingInteractionRepository();
        var handler = MakeHandler(repo);

        await handler.HandleAsync(
            new RecordNewsInteractionCommand(1, ValidId, "save", null));

        Assert.NotNull(repo.Inserted);
        Assert.Equal(NewsInteractionType.Save, repo.Inserted!.Type);
        Assert.Null(repo.Inserted.DwellSeconds);
    }

    [Fact]
    public async Task Inserts_dismiss_interaction_with_null_dwell()
    {
        var repo = new CapturingInteractionRepository();
        var handler = MakeHandler(repo);

        await handler.HandleAsync(
            new RecordNewsInteractionCommand(1, ValidId, "dismiss", null));

        Assert.NotNull(repo.Inserted);
        Assert.Equal(NewsInteractionType.Dismiss, repo.Inserted!.Type);
        Assert.Null(repo.Inserted.DwellSeconds);
    }

    [Fact]
    public async Task Type_matching_is_case_insensitive()
    {
        var repo = new CapturingInteractionRepository();
        var handler = MakeHandler(repo);

        await handler.HandleAsync(
            new RecordNewsInteractionCommand(1, ValidId, "READ", 10));

        Assert.Equal(NewsInteractionType.Read, repo.Inserted!.Type);
    }

    [Fact]
    public async Task RecordedAt_is_populated()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var repo = new CapturingInteractionRepository();
        var handler = MakeHandler(repo);

        await handler.HandleAsync(new RecordNewsInteractionCommand(1, ValidId, "save", null));

        Assert.True(repo.Inserted!.RecordedAt >= before);
    }

    // ---------------------------------------------------------------------------
    // Validation tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Throws_ValidationException_for_zero_user_id()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new RecordNewsInteractionCommand(0, ValidId, "save", null)));
    }

    [Fact]
    public async Task Throws_ValidationException_for_invalid_news_item_id()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new RecordNewsInteractionCommand(1, "bad-id", "save", null)));
    }

    [Fact]
    public async Task Throws_ValidationException_for_unknown_type()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new RecordNewsInteractionCommand(1, ValidId, "like", null)));
    }

    [Fact]
    public async Task Throws_ValidationException_when_read_has_no_dwell_seconds()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new RecordNewsInteractionCommand(1, ValidId, "read", null)));
    }

    [Fact]
    public async Task Throws_ValidationException_when_read_dwell_is_zero()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new RecordNewsInteractionCommand(1, ValidId, "read", 0)));
    }

    [Fact]
    public async Task Throws_ValidationException_when_save_has_dwell_seconds()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new RecordNewsInteractionCommand(1, ValidId, "save", 30)));
    }

    [Fact]
    public async Task Throws_ValidationException_when_dismiss_has_dwell_seconds()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new RecordNewsInteractionCommand(1, ValidId, "dismiss", 5)));
    }
}
