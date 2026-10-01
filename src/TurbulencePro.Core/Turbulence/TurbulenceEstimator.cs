using TurbulencePro.Core.Sim;

namespace TurbulencePro.Core.Turbulence;

public sealed class TurbulenceEstimator
{
    public TurbulenceClass? ForcedClass { get; set; }
    public double SimulatedDrive { get; set; } = 0.45;

    public (TurbulenceClass Class, string[] Reasons, double Confidence) Select(FlightSample sample, FlightPhase phase, RealismGateResult gate, double measured)
    {
        if (ForcedClass.HasValue)
            return (ForcedClass.Value, ["class:manual-override", "simulated:not-simulator-turbulence"], 1);

        var reasons = new List<string> { "simulated:statistical-model" };
        TurbulenceClass klass;
        var confidence = 0.35 + Math.Min(0.4, measured);

        if (sample.OnGround)
        {
            klass = TurbulenceClass.Ground;
            reasons.Add("class:on-ground");
        }
        else if (sample.InCloud || (sample.PrecipStateMask & 12) != 0)
        {
            klass = TurbulenceClass.Convective;
            reasons.Add("class:convective-probability-from-cloud-or-precip");
            reasons.Add("not-weather-derived-cell");
            confidence = 0.45;
        }
        else if (sample.AglFeet < 1500 && gate.MechanicalScale > 0.2)
        {
            klass = TurbulenceClass.Mechanical;
            reasons.Add("class:mechanical-from-agl-and-wind");
        }
        else if (sample.AltitudeFeet > 22000 && sample.AmbientWindSpeedKnots > 40 && !sample.InCloud)
        {
            klass = TurbulenceClass.ClearAirStatistical;
            reasons.Add("class:statistical-cat");
            reasons.Add("not-a-weather-forecast");
            confidence = 0.3;
        }
        else if (sample.AglFeet < 4000 && sample.AmbientTemperatureWouldSuggestThermal())
        {
            klass = TurbulenceClass.Thermal;
            reasons.Add("class:thermal-heuristic");
        }
        else if (measured >= 0.66)
            klass = TurbulenceClass.Severe;
        else if (measured >= 0.33)
            klass = TurbulenceClass.Moderate;
        else if (measured >= 0.08)
            klass = TurbulenceClass.Light;
        else
            klass = TurbulenceClass.Calm;

        if (phase == FlightPhase.Cruise && klass == TurbulenceClass.Ground)
            klass = TurbulenceClass.Calm;

        return (klass, reasons.ToArray(), Math.Clamp(confidence, 0, 1));
    }
}

internal static class ThermalHeuristic
{
    public static bool AmbientTemperatureWouldSuggestThermal(this FlightSample sample) =>
        sample.AglFeet is > 500 and < 5000 && !sample.InCloud && sample.AmbientWindSpeedKnots < 12;
}
