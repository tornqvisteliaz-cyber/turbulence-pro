namespace TurbulencePro.Core.Turbulence;

public enum TurbulenceClass
{
    Calm,
    Light,
    Moderate,
    Severe,
    ClearAirStatistical,
    Mechanical,
    MountainWave,
    Convective,
    Thermal,
    Ground
}

public sealed class TurbulenceEstimate
{
    public double MeasuredIntensity { get; init; }
    public double SimulatedIntensity { get; init; }
    public double FinalIntensity { get; init; }
    public TurbulenceClass Class { get; init; }
    public double Confidence { get; init; }
    public string[] ReasonCodes { get; init; } = [];

    public static TurbulenceEstimate Empty { get; } = new();
}
