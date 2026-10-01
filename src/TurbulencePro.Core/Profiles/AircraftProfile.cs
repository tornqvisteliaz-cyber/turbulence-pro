namespace TurbulencePro.Core.Profiles;

/// <summary>
/// Perceptual response only. These numbers do not reproduce a flight model.
/// </summary>
public sealed class AircraftProfile
{
    public string Id { get; set; } = "generic-airliner";
    public string Aircraft { get; set; } = "Generic Airliner";
    public string MatchTitleContains { get; set; } = "";
    public double ReferenceMassKg { get; set; } = 70000;
    public double VerticalResponse { get; set; } = 1;
    public double LateralResponse { get; set; } = 0.8;
    public double LongitudinalResponse { get; set; } = 0.7;
    public double PitchResponse { get; set; } = 0.8;
    public double RollResponse { get; set; } = 0.75;
    public double YawResponse { get; set; } = 0.5;
    public double StructuralVibration { get; set; } = 1;
    public double CameraSensitivity { get; set; } = 0.2;
    public double SoundSensitivity { get; set; } = 1;

    public double MassScale(double liveWeightPounds)
    {
        var liveKg = liveWeightPounds > 1000 ? liveWeightPounds * 0.453592 : ReferenceMassKg;
        var scale = ReferenceMassKg / Math.Max(liveKg, 1);
        return Math.Clamp(scale, 0.35, 1.8);
    }
}
