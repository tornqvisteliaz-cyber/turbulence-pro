using TurbulencePro.Core.Profiles;
using TurbulencePro.Core.Turbulence;

namespace TurbulencePro.Core.Effects;

public interface ICameraEffect
{
    bool IsEnabled { get; }
    void Apply(double xMetres, double yMetres, double zMetres, double pitchRad, double bankRad, double headingRad);
    void Release();
}

public sealed class NullCameraEffect : ICameraEffect
{
    public bool IsEnabled => false;
    public void Apply(double xMetres, double yMetres, double zMetres, double pitchRad, double bankRad, double headingRad) { }
    public void Release() { }
}

public interface IWakeEstimator
{
    double Estimate();
}

public sealed class NullWakeEstimator : IWakeEstimator
{
    public double Estimate() => 0;
}

public sealed class EffectMix
{
    public double Vertical { get; init; }
    public double Lateral { get; init; }
    public double Longitudinal { get; init; }
    public double Pitch { get; init; }
    public double Roll { get; init; }
    public double Yaw { get; init; }
    public double Vibration { get; init; }
    public double Audio { get; init; }
    public double Camera { get; init; }
}

public sealed class EffectGains
{
    public double Master { get; set; } = 0.5;
    public double Vertical { get; set; } = 0.6;
    public double Lateral { get; set; } = 0.4;
    public double Longitudinal { get; set; } = 0.3;
    public double Pitch { get; set; } = 0.25;
    public double Roll { get; set; } = 0.3;
    public double Yaw { get; set; } = 0.15;
    public double Vibration { get; set; } = 0.7;
    public double Audio { get; set; } = 0.5;
    public double Camera { get; set; } = 0;
    public bool AudioEnabled { get; set; } = true;
    public bool CameraEnabled { get; set; }
}

public sealed class EffectMixer
{
    public EffectGains Gains { get; } = new();

    public EffectMix Mix(GustChannels gust, AircraftProfile profile, double finalIntensity, double liveWeightPounds)
    {
        var mass = profile.MassScale(liveWeightPounds);
        var drive = finalIntensity * Gains.Master;
        return new EffectMix
        {
            Vertical = gust.Vertical * profile.VerticalResponse * Gains.Vertical * drive,
            Lateral = gust.Lateral * profile.LateralResponse * Gains.Lateral * drive,
            Longitudinal = gust.Longitudinal * profile.LongitudinalResponse * Gains.Longitudinal * drive,
            Pitch = gust.Pitch * profile.PitchResponse * Gains.Pitch * drive * mass,
            Roll = gust.Roll * profile.RollResponse * Gains.Roll * drive * mass,
            Yaw = gust.Yaw * profile.YawResponse * Gains.Yaw * drive * mass,
            Vibration = gust.Vibration * profile.StructuralVibration * Gains.Vibration * drive,
            Audio = Gains.AudioEnabled ? Math.Clamp(Math.Abs(gust.Vibration) * Gains.Audio * drive * profile.SoundSensitivity, 0, 1) : 0,
            Camera = Gains.CameraEnabled ? drive * Gains.Camera * profile.CameraSensitivity : 0
        };
    }
}
