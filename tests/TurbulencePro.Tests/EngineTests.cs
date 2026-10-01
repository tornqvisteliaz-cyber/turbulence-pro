using TurbulencePro.Core.Engine;
using TurbulencePro.Core.Profiles;
using TurbulencePro.Core.Recording;
using TurbulencePro.Core.Sim;
using TurbulencePro.Core.Turbulence;
using TurbulencePro.Audio;
using Xunit;

namespace TurbulencePro.Tests;

public class EngineTests
{
    [Fact]
    public void Steady_body_acceleration_is_not_treated_as_turbulence()
    {
        var detector = new MeasuredTurbulenceDetector();
        var sample = Cruise();
        sample = WithAccel(sample, 0, 32.2, 0);
        MeasuredSnapshot last = new();
        for (var i = 0; i < 80; i++)
            last = detector.Update(sample, 0.05);
        Assert.True(last.Intensity < 0.08, $"steady 1g-like body Y scored {last.Intensity}");
    }

    [Fact]
    public void Irregular_acceleration_scores_higher_than_steady()
    {
        var steady = new MeasuredTurbulenceDetector();
        var rough = new MeasuredTurbulenceDetector();
        MeasuredSnapshot a = new(), b = new();
        for (var i = 0; i < 80; i++)
        {
            a = steady.Update(WithAccel(Cruise(), 0, 32.2, 0), 0.05);
            var wobble = Math.Sin(i * 1.7) * 12 + (i % 7 == 0 ? 8 : 0);
            b = rough.Update(WithAccel(Cruise(), wobble, 32.2 + wobble, wobble * 0.4), 0.05);
        }
        Assert.True(b.Intensity > a.Intensity + 0.2, $"rough {b.Intensity} steady {a.Intensity}");
    }

    [Fact]
    public void Parked_aircraft_has_no_final_effect()
    {
        var loop = new SimulationLoop();
        var sample = new FlightSample { OnGround = true, GroundSpeedKnots = 0, IndicatedAirspeedKnots = 0, AltitudeFeet = 200 };
        EngineStep step = new();
        for (var i = 0; i < 40; i++)
            step = loop.Step(sample, 0.05);
        Assert.Equal(0, step.Estimate.FinalIntensity);
        Assert.Contains("gate:parked-no-atmospheric", step.Estimate.ReasonCodes);
    }

    [Fact]
    public void Cruise_does_not_enable_ground_bumps()
    {
        var gate = RealismGate.Evaluate(Cruise(), FlightPhase.Cruise);
        Assert.Equal(0, gate.GroundScale);
        Assert.Contains("gate:no-ground-bumps-airborne", gate.ReasonCodes);
    }

    [Fact]
    public void Severe_spectrum_is_not_scaled_light()
    {
        var light = ProceduralGustField.SpectrumFor(TurbulenceClass.Light);
        var severe = ProceduralGustField.SpectrumFor(TurbulenceClass.Severe);
        Assert.NotEqual(light.FrequencyHz, severe.FrequencyHz);
        Assert.NotEqual(light.CalmFraction, severe.CalmFraction);
        Assert.True(severe.EventRatePerMinute > light.EventRatePerMinute * 2);
    }

    [Fact]
    public void Gust_field_is_continuous()
    {
        var field = new ProceduralGustField();
        var prev = field.Update(TurbulenceClass.Moderate, 0.033, 0.6).Vertical;
        var maxJump = 0.0;
        for (var i = 0; i < 200; i++)
        {
            var next = field.Update(TurbulenceClass.Moderate, 0.033, 0.6).Vertical;
            maxJump = Math.Max(maxJump, Math.Abs(next - prev));
            prev = next;
        }
        Assert.True(maxJump < 0.35, $"jump {maxJump}");
    }

    [Fact]
    public void Simulated_reason_is_not_labelled_as_simulator_turbulence()
    {
        var loop = new SimulationLoop();
        loop.Estimator.ForcedClass = TurbulenceClass.ClearAirStatistical;
        var step = loop.Step(Cruise(), 0.05);
        Assert.Contains(step.Estimate.ReasonCodes, r => r.Contains("statistical") || r.Contains("simulated"));
    }

    [Fact]
    public void Profile_round_trip_and_heavy_aircraft_scales_down()
    {
        var dir = Path.Combine(Path.GetTempPath(), "tp-profiles-" + Guid.NewGuid().ToString("N"));
        var manager = ProfileManager.CreateWithStarters();
        manager.SaveAll(dir);
        var loaded = new ProfileManager();
        loaded.LoadDirectory(dir);
        var seven = loaded.Resolve("PMDG 777-300ER");
        Assert.Equal("pmdg-777", seven.Id);
        Assert.True(seven.MassScale(500000) < 1);
        Assert.True(loaded.Resolve("Cessna 172").MassScale(2000) > 1);
    }

    [Fact]
    public void Recording_replays_into_the_same_engine()
    {
        var loop = new SimulationLoop();
        var recorder = new FlightRecorder();
        var sample = Cruise();
        for (var i = 0; i < 5; i++)
            recorder.Add(loop.Step(sample, 0.05));
        var path = Path.Combine(Path.GetTempPath(), "tp-" + Guid.NewGuid().ToString("N") + ".csv");
        recorder.Save(path);
        var replay = FlightRecorder.LoadSamples(path);
        Assert.Equal(5, replay.Count);
        var again = new SimulationLoop();
        var step = again.Step(replay[0], 0.05);
        Assert.Equal(sample.AltitudeFeet, step.Sample.AltitudeFeet);
    }

    [Fact]
    public void Audio_follows_final_intensity()
    {
        var audio = new ProceduralAudioEngine();
        audio.Update(0, 0, "", 0.05);
        var quiet = audio.Levels.Rumble;
        for (var i = 0; i < 30; i++)
            audio.Update(0.9, 0.8, "vertical-drop", 0.05);
        Assert.True(audio.Levels.Rumble > quiet);
        Assert.True(audio.Levels.Impact > 0);
    }

    private static FlightSample Cruise() => new()
    {
        AircraftTitle = "Test Airliner",
        AltitudeFeet = 35000,
        AglFeet = 34000,
        IndicatedAirspeedKnots = 280,
        GroundSpeedKnots = 450,
        VerticalSpeedFeetPerSecond = 0,
        OnGround = false,
        TotalWeightPounds = 140000,
        AmbientWindSpeedKnots = 20
    };

    private static FlightSample WithAccel(FlightSample sample, double x, double y, double z) => new()
    {
        AircraftTitle = sample.AircraftTitle,
        AltitudeFeet = sample.AltitudeFeet,
        AglFeet = sample.AglFeet,
        IndicatedAirspeedKnots = sample.IndicatedAirspeedKnots,
        GroundSpeedKnots = sample.GroundSpeedKnots,
        VerticalSpeedFeetPerSecond = sample.VerticalSpeedFeetPerSecond,
        OnGround = sample.OnGround,
        TotalWeightPounds = sample.TotalWeightPounds,
        AmbientWindSpeedKnots = sample.AmbientWindSpeedKnots,
        AccelerationBodyXFeetPerSec2 = x,
        AccelerationBodyYFeetPerSec2 = y,
        AccelerationBodyZFeetPerSec2 = z
    };
}
