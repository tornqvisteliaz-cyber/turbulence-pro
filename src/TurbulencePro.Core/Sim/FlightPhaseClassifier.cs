namespace TurbulencePro.Core.Sim;

public enum FlightPhase
{
    Unknown,
    Parked,
    Taxi,
    Takeoff,
    InitialClimb,
    Climb,
    Cruise,
    Descent,
    Approach,
    Landing,
    AfterLanding
}

public static class FlightPhaseClassifier
{
    public static FlightPhase Classify(FlightSample sample)
    {
        var ias = sample.IndicatedAirspeedKnots;
        var gs = sample.GroundSpeedKnots;
        var agl = sample.AglFeet;
        var vs = sample.VerticalSpeedFeetPerSecond;

        if (sample.OnGround)
        {
            if (gs < 1.0 && ias < 30.0)
                return FlightPhase.Parked;
            if (gs < 40.0)
                return FlightPhase.Taxi;
            return vs > 2.0 ? FlightPhase.Takeoff : FlightPhase.AfterLanding;
        }

        if (agl < 1500 && vs < -2.0)
            return ias < 160 ? FlightPhase.Approach : FlightPhase.Descent;
        if (agl < 200 && Math.Abs(vs) < 5)
            return FlightPhase.Landing;
        if (agl < 3000 && vs > 2.0)
            return FlightPhase.InitialClimb;
        if (vs > 3.0)
            return FlightPhase.Climb;
        if (vs < -3.0)
            return FlightPhase.Descent;
        return FlightPhase.Cruise;
    }
}
