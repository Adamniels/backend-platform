namespace Platform.Domain.Features.HumanInput;

public sealed class InputNeededItem
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string Type { get; set; } = "";
    public bool Urgent { get; set; }
    public string Detail { get; set; } = "";
}
