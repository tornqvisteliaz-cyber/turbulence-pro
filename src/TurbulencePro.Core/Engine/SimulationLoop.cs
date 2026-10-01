using TurbulencePro.Core.Effects;
using TurbulencePro.Core.Profiles;
using TurbulencePro.Core.Sim;

namespace TurbulencePro.Core.Engine;

public sealed class EngineStep
{
    public FlightSample Sample { get; init; } = new();
    public FlightPhase Phase { get; init; }
    public Turbulence.TurbulenceEstimate Estimate { get; init; } = Turbulence.TurbulenceEstimate.Empty;
    public Turbulence.GustChannels Gust { get; init; } = new();
    public Turbulence.MeasuredSnapshot Measured { get; init; } = new();
    public EffectMix Effects { get; init; } = new();
    public string ActiveEvent { get; init; } = "";
}

/// <summary>
/// Single pipeline used by SimConnect, test mode, and recording replay.
/// </summary>
public sealed class SimulationLoop
{
    private readonly Turbulence.MeasuredTurbulenceDetector _detector = new();
    private readonly Turbulence.ProceduralGustField _gust = new();
    private readonly Turbulence.TurbulenceEstimator _estimator = new();
    private readonly EffectMixer _mixer = new();
    private readonly ICameraEffect _camera;
    private readonly IWakeEstimator _wake;

    public SimulationLoop(ICameraEffect? camera = null, IWakeEstimator? wake = null)
    {
        _camera = camera ?? new NullCameraEffect();
        _wake = wake ?? new NullWakeEstimator();
    }

    public Turbulence.TurbulenceEstimator Estimator => _estimator;
    public EffectMixer Mixer => _mixer;
    public EngineStep Last { get; private set; } = new();
    public AircraftProfile Profile { get; set; } = new();

    public EngineStep Step(FlightSample sample, double dtSeconds)
    {
        var phase = FlightPhaseClassifier.Classify(sample);
        var measured = _detector.Update(sample, dtSeconds);
        var gate = Turbulence.RealismGate.Evaluate(sample, phase);
        var (klass, reasons, confidence) = _estimator.Select(sample, phase, gate, measured.Intensity);

        var atmosphericDrive = _estimator.SimulatedDrive * gate.AtmosphericScale;
        if (klass == Turbulence.TurbulenceClass.Ground)
            atmosphericDrive = _estimator.SimulatedDrive * gate.GroundScale;
        if (klass == Turbulence.TurbulenceClass.Mechanical)
            atmosphericDrive = _estimator.SimulatedDrive * Math.Max(gate.MechanicalScale, 0.15);

        var gust = _gust.Update(klass, dtSeconds, Math.Clamp(atmosphericDrive, 0, 1));
        var simulated = gust.Energy;
        var wake = _wake.Estimate();
        var blend = 0.55 * measured.Intensity + 0.45 * simulated;
        var final = Math.Clamp(blend * (klass == Turbulence.TurbulenceClass.Ground ? gate.GroundScale : gate.AtmosphericScale) + wake, 0, 1);
        if (phase == FlightPhase.Parked)
            final = 0;

        var estimate = new Turbulence.TurbulenceEstimate
        {
            MeasuredIntensity = measured.Intensity,
            SimulatedIntensity = simulated,
            FinalIntensity = final,
            Class = final < 0.02 && klass != Turbulence.TurbulenceClass.Ground ? Turbulence.TurbulenceClass.Calm : klass,
            Confidence = confidence,
            ReasonCodes = reasons.Concat(gate.ReasonCodes).Append("measured:high-pass-body-accel").ToArray()
        };
        var effects = _mixer.Mix(gust, Profile, final, sample.TotalWeightPounds);
        _camera.Apply(0, 0, 0, 0, 0, 0);

        Last = new EngineStep
        {
            Sample = sample,
            Phase = phase,
            Estimate = estimate,
            Gust = gust,
            Measured = measured,
            Effects = effects,
            ActiveEvent = gust.ActiveEvent
        };
        return Last;
    }
}
