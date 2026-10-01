namespace TurbulencePro.Core.Sim;

/// <summary>
/// One verified sample. Units are the units requested from SimConnect, documented in SIMVAR_UNITS.md.
/// Acceleration is specific acceleration along the body axes in feet per second squared, not G-load.
/// </summary>
public sealed class FlightSample
{
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public string AircraftTitle { get; init; } = "";
    public double AltitudeFeet { get; init; }
    public double AglFeet { get; init; }
    public double IndicatedAirspeedKnots { get; init; }
    public double GroundSpeedKnots { get; init; }
    /// <summary>Indicated vertical speed. SDK unit is feet per second. Display converts to ft/min.</summary>
    public double VerticalSpeedFeetPerSecond { get; init; }
    public double PitchRadians { get; init; }
    public double BankRadians { get; init; }
    public double HeadingRadians { get; init; }
    /// <summary>Body lateral axis. Not world east. Not G-load.</summary>
    public double AccelerationBodyXFeetPerSec2 { get; init; }
    /// <summary>Body vertical axis. Not total G-load. Steady flight may sit near 32 ft/s^2.</summary>
    public double AccelerationBodyYFeetPerSec2 { get; init; }
    /// <summary>Body longitudinal axis. Not world north.</summary>
    public double AccelerationBodyZFeetPerSec2 { get; init; }
    public double AmbientWindXMps { get; init; }
    public double AmbientWindYMps { get; init; }
    public double AmbientWindZMps { get; init; }
    public double AmbientWindSpeedKnots { get; init; }
    public double AmbientWindDirectionDegrees { get; init; }
    public double AircraftWindXKnots { get; init; }
    public double AircraftWindYKnots { get; init; }
    public double AircraftWindZKnots { get; init; }
    public double TotalWeightPounds { get; init; }
    public bool OnGround { get; init; }
    public bool InCloud { get; init; }
    public int PrecipStateMask { get; init; }
    public double PrecipRateMillimetres { get; init; }
    public bool EnginesRunning { get; init; }

    public double VerticalSpeedFeetPerMinute => VerticalSpeedFeetPerSecond * 60.0;
    public double PitchDegrees => PitchRadians * 180.0 / Math.PI;
    public double BankDegrees => BankRadians * 180.0 / Math.PI;
    public double HeadingDegrees => HeadingRadians * 180.0 / Math.PI;
}
