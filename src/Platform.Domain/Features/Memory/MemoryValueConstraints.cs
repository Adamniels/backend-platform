namespace Platform.Domain.Features.Memory;

public static class MemoryValueConstraints
{
    public const double MinUnit = 0d;
    public const double MaxUnit = 1d;

    public static double Clamp01(double value) => value < MinUnit ? MinUnit : value > MaxUnit ? MaxUnit : value;

    public static void ThrowIfOutOf01(string name, double value)
    {
        if (double.IsNaN(value) || value < MinUnit || value > MaxUnit)
        {
            throw new MemoryDomainException($"{name} must be between 0.0 and 1.0 inclusive (got {value}).");
        }
    }
}
