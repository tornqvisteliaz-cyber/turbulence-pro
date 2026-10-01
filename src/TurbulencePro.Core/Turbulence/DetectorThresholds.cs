namespace TurbulencePro.Core.Turbulence;

/// <summary>
/// Calibration thresholds for the measured detector. Values are starting points, not certified limits.
/// Acceleration thresholds apply to high-pass filtered body acceleration in feet per second squared.
/// </summary>
public sealed class DetectorThresholds
{
    public double AccelLightFeetPerSec2 { get; init; } = 0.8;
    public double AccelModerateFeetPerSec2 { get; init; } = 3.5;
    public double AccelSevereFeetPerSec2 { get; init; } = 8.0;
    public double VerticalSpeedJerkLight { get; init; } = 1.5;
    public double VerticalSpeedJerkSevere { get; init; } = 8.0;
    public double WindRateLightMps { get; init; } = 0.4;
    public double WindRateSevereMps { get; init; } = 3.0;
    public double AttitudeRateLightRad { get; init; } = 0.01;
    public double AttitudeRateSevereRad { get; init; } = 0.08;
    public double HighPassCutoffHz { get; init; } = 0.25;
    public double OutputSmoothingSeconds { get; init; } = 1.2;
}

public sealed class MeasuredSnapshot
{
    public double Intensity { get; init; }
    public double FilteredAccelRms { get; init; }
    public double FilteredVerticalSpeedRate { get; init; }
    public double FilteredWindRate { get; init; }
    public double FilteredAttitudeRate { get; init; }
    public double HighPassAccelX { get; init; }
    public double HighPassAccelY { get; init; }
    public double HighPassAccelZ { get; init; }
}
