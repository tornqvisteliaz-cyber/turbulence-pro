namespace TurbulencePro.Core.Turbulence;

public sealed class GustChannels
{
    public double Vertical { get; init; }
    public double Lateral { get; init; }
    public double Longitudinal { get; init; }
    public double Pitch { get; init; }
    public double Roll { get; init; }
    public double Yaw { get; init; }
    public double Vibration { get; init; }
    public double Energy { get; init; }
    public string ActiveEvent { get; init; } = "";
}

public sealed class ClassSpectrum
{
    public double FrequencyHz { get; init; }
    public double Amplitude { get; init; }
    public double CalmFraction { get; init; }
    public double EventRatePerMinute { get; init; }
    public double VibrationHz { get; init; }
    public double VerticalBias { get; init; }
    public double LateralBias { get; init; }
    public double PeriodSeconds { get; init; }
}
