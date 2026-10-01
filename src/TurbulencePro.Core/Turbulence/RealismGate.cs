using TurbulencePro.Core.Sim;

namespace TurbulencePro.Core.Turbulence;

public sealed class RealismGateResult
{
    public double AtmosphericScale { get; init; }
    public double GroundScale { get; init; }
    public double MechanicalScale { get; init; }
    public string[] ReasonCodes { get; init; } = [];
}

public static class RealismGate
{
    public static RealismGateResult Evaluate(FlightSample sample, FlightPhase phase)
    {
        var reasons = new List<string>();
        var moving = sample.GroundSpeedKnots > 2 || sample.IndicatedAirspeedKnots > 40;
        var atmospheric = 1.0;
        var ground = 0.0;
        var mechanical = 0.0;

        if (phase == FlightPhase.Parked || (sample.OnGround && sample.GroundSpeedKnots < 1 && sample.IndicatedAirspeedKnots < 5))
        {
            atmospheric = 0;
            ground = 0;
            reasons.Add("gate:parked-no-atmospheric");
        }
        else if (!moving && sample.OnGround)
        {
            ground = 0;
            atmospheric = 0;
            reasons.Add("gate:stationary-no-ground-vibration");
        }

        if (sample.OnGround && sample.GroundSpeedKnots >= 1)
        {
            ground = Math.Clamp(sample.GroundSpeedKnots / 40.0, 0, 1);
            atmospheric *= 0.15;
            reasons.Add("gate:ground-effects");
        }
        else
        {
            reasons.Add("gate:no-ground-bumps-airborne");
        }

        if (sample.AglFeet > 2000)
        {
            mechanical = 0;
            reasons.Add("gate:mechanical-reduced-altitude");
        }
        else if (!sample.OnGround && sample.AglFeet < 1500 && sample.AmbientWindSpeedKnots > 8)
        {
            mechanical = Math.Clamp(1.0 - sample.AglFeet / 1500.0, 0, 1) * Math.Clamp(sample.AmbientWindSpeedKnots / 25.0, 0, 1);
            reasons.Add("gate:mechanical-possible");
        }

        if (sample.AltitudeFeet > 18000)
            reasons.Add("gate:high-altitude");

        if (!moving)
            atmospheric = 0;

        return new RealismGateResult
        {
            AtmosphericScale = atmospheric,
            GroundScale = ground,
            MechanicalScale = mechanical,
            ReasonCodes = reasons.ToArray()
        };
    }
}
