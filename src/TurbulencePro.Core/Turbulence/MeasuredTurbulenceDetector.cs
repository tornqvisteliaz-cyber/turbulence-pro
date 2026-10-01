using TurbulencePro.Core.Sim;

namespace TurbulencePro.Core.Turbulence;

/// <summary>
/// Infers turbulence from changes in simulator data. Absolute acceleration is not used:
/// ACCELERATION BODY Y is body-axis feet per second squared and can sit near 1 g in steady flight.
/// </summary>
public sealed class MeasuredTurbulenceDetector
{
    private readonly DetectorThresholds _thresholds;
    private bool _hasPrevious;
    private double _hpX, _hpY, _hpZ, _hpVs, _hpWind, _hpAtt;
    private double _prevAx, _prevAy, _prevAz, _prevVs, _prevWind, _prevPitch, _prevBank;
    private double _smoothed;
    private readonly Queue<double> _accelWindow = new();
    private const int Window = 40;

    public MeasuredTurbulenceDetector(DetectorThresholds? thresholds = null)
    {
        _thresholds = thresholds ?? new DetectorThresholds();
    }

    public MeasuredSnapshot Last { get; private set; } = new();

    public void Reset()
    {
        _hasPrevious = false;
        _hpX = _hpY = _hpZ = _hpVs = _hpWind = _hpAtt = 0;
        _smoothed = 0;
        _accelWindow.Clear();
        Last = new MeasuredSnapshot();
    }

    public MeasuredSnapshot Update(FlightSample sample, double dtSeconds)
    {
        dtSeconds = Math.Clamp(dtSeconds, 0.005, 0.25);
        var wind = Math.Sqrt(
            sample.AmbientWindXMps * sample.AmbientWindXMps +
            sample.AmbientWindYMps * sample.AmbientWindYMps +
            sample.AmbientWindZMps * sample.AmbientWindZMps);

        if (!_hasPrevious)
        {
            _prevAx = sample.AccelerationBodyXFeetPerSec2;
            _prevAy = sample.AccelerationBodyYFeetPerSec2;
            _prevAz = sample.AccelerationBodyZFeetPerSec2;
            _prevVs = sample.VerticalSpeedFeetPerSecond;
            _prevWind = wind;
            _prevPitch = sample.PitchRadians;
            _prevBank = sample.BankRadians;
            _hasPrevious = true;
            Last = new MeasuredSnapshot();
            return Last;
        }

        var alpha = Math.Exp(-2.0 * Math.PI * _thresholds.HighPassCutoffHz * dtSeconds);
        _hpX = HighPass(_hpX, sample.AccelerationBodyXFeetPerSec2, _prevAx, alpha);
        _hpY = HighPass(_hpY, sample.AccelerationBodyYFeetPerSec2, _prevAy, alpha);
        _hpZ = HighPass(_hpZ, sample.AccelerationBodyZFeetPerSec2, _prevAz, alpha);
        _hpVs = HighPass(_hpVs, sample.VerticalSpeedFeetPerSecond, _prevVs, alpha);
        _hpWind = HighPass(_hpWind, wind, _prevWind, alpha);
        var att = Math.Abs(sample.PitchRadians - _prevPitch) + Math.Abs(sample.BankRadians - _prevBank);
        _hpAtt = HighPass(_hpAtt, att, 0, alpha);

        _prevAx = sample.AccelerationBodyXFeetPerSec2;
        _prevAy = sample.AccelerationBodyYFeetPerSec2;
        _prevAz = sample.AccelerationBodyZFeetPerSec2;
        _prevVs = sample.VerticalSpeedFeetPerSecond;
        _prevWind = wind;
        _prevPitch = sample.PitchRadians;
        _prevBank = sample.BankRadians;

        var accelMag = Math.Sqrt(_hpX * _hpX + _hpY * _hpY + _hpZ * _hpZ);
        _accelWindow.Enqueue(accelMag);
        while (_accelWindow.Count > Window)
            _accelWindow.Dequeue();
        var rms = Math.Sqrt(_accelWindow.Average(v => v * v));

        var accelScore = Score(rms, _thresholds.AccelLightFeetPerSec2, _thresholds.AccelSevereFeetPerSec2);
        var vsScore = Score(Math.Abs(_hpVs) / dtSeconds, _thresholds.VerticalSpeedJerkLight, _thresholds.VerticalSpeedJerkSevere);
        var windScore = Score(Math.Abs(_hpWind) / dtSeconds, _thresholds.WindRateLightMps, _thresholds.WindRateSevereMps);
        var attScore = Score(_hpAtt / dtSeconds, _thresholds.AttitudeRateLightRad, _thresholds.AttitudeRateSevereRad);

        var raw = 0.50 * accelScore + 0.22 * vsScore + 0.18 * windScore + 0.10 * attScore;
        var smooth = 1.0 - Math.Exp(-dtSeconds / _thresholds.OutputSmoothingSeconds);
        _smoothed += (raw - _smoothed) * smooth;

        Last = new MeasuredSnapshot
        {
            Intensity = Clamp01(_smoothed),
            FilteredAccelRms = rms,
            FilteredVerticalSpeedRate = Math.Abs(_hpVs) / dtSeconds,
            FilteredWindRate = Math.Abs(_hpWind) / dtSeconds,
            FilteredAttitudeRate = _hpAtt / dtSeconds,
            HighPassAccelX = _hpX,
            HighPassAccelY = _hpY,
            HighPassAccelZ = _hpZ
        };
        return Last;
    }

    private static double HighPass(double state, double current, double previous, double alpha) =>
        alpha * (state + current - previous);

    private static double Score(double value, double light, double severe)
    {
        if (severe <= light || value <= 0)
            return 0;
        return Clamp01((value - light) / (severe - light));
    }

    private static double Clamp01(double v) => Math.Clamp(v, 0, 1);
}
