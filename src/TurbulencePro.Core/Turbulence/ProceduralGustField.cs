using TurbulencePro.Core.Noise;

namespace TurbulencePro.Core.Turbulence;

/// <summary>
/// Procedural gust field. Independent of measured simulator turbulence.
/// Output is a continuous filtered signal, not a per-frame random draw.
/// </summary>
public sealed class ProceduralGustField
{
    private readonly ValueNoise _vertical;
    private readonly ValueNoise _lateral;
    private readonly ValueNoise _longitudinal;
    private readonly ValueNoise _vibration;
    private readonly ValueNoise _calm;
    private double _time;
    private double _eventEnvelope;
    private double _eventSign = 1;
    private string _eventName = "";
    private double _nextEventAt = 8;
    private readonly Random _eventRng;

    public ProceduralGustField(int seed = 1701)
    {
        _vertical = new ValueNoise(seed);
        _lateral = new ValueNoise(seed + 17);
        _longitudinal = new ValueNoise(seed + 29);
        _vibration = new ValueNoise(seed + 41);
        _calm = new ValueNoise(seed + 53);
        _eventRng = new Random(seed + 71);
    }

    public GustChannels Last { get; private set; } = new();
    public string ActiveEvent => _eventName;

    public static ClassSpectrum SpectrumFor(TurbulenceClass klass) => klass switch
    {
        TurbulenceClass.Light => new ClassSpectrum { FrequencyHz = 0.35, Amplitude = 0.28, CalmFraction = 0.70, EventRatePerMinute = 2, VibrationHz = 6, VerticalBias = 0.7, LateralBias = 0.25, PeriodSeconds = 14 },
        TurbulenceClass.Moderate => new ClassSpectrum { FrequencyHz = 0.75, Amplitude = 0.55, CalmFraction = 0.42, EventRatePerMinute = 6, VibrationHz = 11, VerticalBias = 0.8, LateralBias = 0.45, PeriodSeconds = 7 },
        TurbulenceClass.Severe => new ClassSpectrum { FrequencyHz = 1.5, Amplitude = 0.9, CalmFraction = 0.18, EventRatePerMinute = 14, VibrationHz = 18, VerticalBias = 1.0, LateralBias = 0.7, PeriodSeconds = 3.2 },
        TurbulenceClass.ClearAirStatistical => new ClassSpectrum { FrequencyHz = 0.55, Amplitude = 0.5, CalmFraction = 0.62, EventRatePerMinute = 3, VibrationHz = 8, VerticalBias = 1.1, LateralBias = 0.2, PeriodSeconds = 18 },
        TurbulenceClass.Mechanical => new ClassSpectrum { FrequencyHz = 0.9, Amplitude = 0.45, CalmFraction = 0.35, EventRatePerMinute = 8, VibrationHz = 14, VerticalBias = 0.5, LateralBias = 0.9, PeriodSeconds = 5 },
        TurbulenceClass.MountainWave => new ClassSpectrum { FrequencyHz = 0.05, Amplitude = 0.7, CalmFraction = 0.25, EventRatePerMinute = 1, VibrationHz = 3, VerticalBias = 1.3, LateralBias = 0.15, PeriodSeconds = 40 },
        TurbulenceClass.Convective => new ClassSpectrum { FrequencyHz = 0.4, Amplitude = 0.65, CalmFraction = 0.4, EventRatePerMinute = 5, VibrationHz = 9, VerticalBias = 1.2, LateralBias = 0.35, PeriodSeconds = 9 },
        TurbulenceClass.Thermal => new ClassSpectrum { FrequencyHz = 0.12, Amplitude = 0.4, CalmFraction = 0.5, EventRatePerMinute = 2, VibrationHz = 4, VerticalBias = 1.0, LateralBias = 0.1, PeriodSeconds = 22 },
        TurbulenceClass.Ground => new ClassSpectrum { FrequencyHz = 2.2, Amplitude = 0.35, CalmFraction = 0.2, EventRatePerMinute = 10, VibrationHz = 22, VerticalBias = 0.8, LateralBias = 0.3, PeriodSeconds = 2.5 },
        _ => new ClassSpectrum { FrequencyHz = 0.2, Amplitude = 0.05, CalmFraction = 0.95, EventRatePerMinute = 0, VibrationHz = 2, VerticalBias = 0.2, LateralBias = 0.1, PeriodSeconds = 30 }
    };

    public GustChannels Update(TurbulenceClass klass, double dtSeconds, double drive)
    {
        dtSeconds = Math.Clamp(dtSeconds, 0.005, 0.1);
        _time += dtSeconds;
        var spec = SpectrumFor(klass);
        var calm = _calm.Fractal(_time * 0.07, 3, 2.0, 0.5);
        var gate = calm > (spec.CalmFraction * 2 - 1) ? 1.0 : 0.25;
        if (klass == TurbulenceClass.Severe)
            gate = calm > -0.2 ? 1.0 : 0.55;

        ScheduleEvent(spec);
        _eventEnvelope *= Math.Exp(-dtSeconds * (klass == TurbulenceClass.Severe ? 1.4 : 2.2));

        var amp = spec.Amplitude * drive * gate;
        var vertical = _vertical.Fractal(_time * spec.FrequencyHz, 4, 2.0, 0.5) * amp * spec.VerticalBias;
        var lateral = _lateral.Fractal(_time * spec.FrequencyHz * 0.8, 4, 2.0, 0.5) * amp * spec.LateralBias;
        var longitudinal = _longitudinal.Fractal(_time * spec.FrequencyHz * 0.6, 3, 2.0, 0.5) * amp * 0.45;
        vertical += _eventEnvelope * _eventSign * spec.Amplitude * 0.8;
        var vibration = _vibration.Fractal(_time * spec.VibrationHz, 2, 2.0, 0.45) * amp;

        var energy = Math.Clamp(Math.Abs(vertical) * 0.5 + Math.Abs(lateral) * 0.25 + Math.Abs(vibration) * 0.25, 0, 1);
        Last = new GustChannels
        {
            Vertical = vertical,
            Lateral = lateral,
            Longitudinal = longitudinal,
            Pitch = vertical * 0.35,
            Roll = lateral * 0.4,
            Yaw = lateral * 0.15,
            Vibration = vibration,
            Energy = energy,
            ActiveEvent = _eventEnvelope > 0.05 ? _eventName : ""
        };
        return Last;
    }

    private void ScheduleEvent(ClassSpectrum spec)
    {
        if (spec.EventRatePerMinute <= 0 || _time < _nextEventAt)
            return;
        _eventEnvelope = 1;
        _eventSign = _eventRng.NextDouble() < 0.5 ? -1 : 1;
        _eventName = _eventSign < 0 ? "vertical-drop" : "vertical-lift";
        var spacing = 60.0 / spec.EventRatePerMinute;
        _nextEventAt = _time + spacing * (0.6 + _eventRng.NextDouble());
    }
}
