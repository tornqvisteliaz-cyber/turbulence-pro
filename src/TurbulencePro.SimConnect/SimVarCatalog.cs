namespace TurbulencePro.SimConnect;

/// <summary>
/// Units below are the strings passed to SimConnect_AddToDataDefinition.
/// They match the MSFS 2024 retail SimVar pages checked for this milestone.
/// </summary>
public static class SimVarCatalog
{
    public static readonly SimVarDef[] Data =
    [
        new("PLANE ALTITUDE", "feet", "Altitude of aircraft, feet."),
        new("PLANE ALT ABOVE GROUND", "feet", "Altitude above the surface, including obstacles, feet."),
        new("AIRSPEED INDICATED", "knots", "Indicated airspeed, knots."),
        new("GROUND VELOCITY", "knots", "Speed relative to the surface, knots."),
        new("VERTICAL SPEED", "feet per second", "Indicated vertical speed. 2024 page unit is feet per second, not feet per minute."),
        new("PLANE PITCH DEGREES", "radians", "Pitch. The name says degrees; the SDK unit is radians."),
        new("PLANE BANK DEGREES", "radians", "Bank. The name says degrees; the SDK unit is radians."),
        new("PLANE HEADING DEGREES TRUE", "radians", "True heading. The name says degrees; the SDK unit is radians."),
        new("ACCELERATION BODY X", "feet per second squared", "Body X axis. SDK also says east/west; that wording is not used. Not G-load."),
        new("ACCELERATION BODY Y", "feet per second squared", "Body vertical axis. Not total G-load. Steady flight may be near 32 ft/s^2."),
        new("ACCELERATION BODY Z", "feet per second squared", "Body longitudinal axis. SDK also says north/south; that wording is not used."),
        new("AMBIENT WIND X", "meters per second", "East/west ambient wind."),
        new("AMBIENT WIND Y", "meters per second", "Vertical ambient wind."),
        new("AMBIENT WIND Z", "meters per second", "North/south ambient wind."),
        new("AMBIENT WIND VELOCITY", "knots", "Ambient wind speed. Requested in knots."),
        new("AMBIENT WIND DIRECTION", "degrees", "Direction relative to true north."),
        new("AIRCRAFT WIND X", "knots", "Wind on the aircraft lateral axis. Rudder assist can force this to 0 on takeoff."),
        new("AIRCRAFT WIND Y", "knots", "Wind on the aircraft vertical axis."),
        new("AIRCRAFT WIND Z", "knots", "Wind on the aircraft longitudinal axis."),
        new("TOTAL WEIGHT", "pounds", "Total weight. Requested in pounds. Confirm in SimVar Watcher if a title returns an implausible value."),
        new("SIM ON GROUND", "bool", "On-ground flag."),
        new("AMBIENT IN CLOUD", "bool", "True if the aircraft is in cloud. Not a cloud-field sample."),
        new("AMBIENT PRECIP STATE", "mask", "2 none, 4 rain, 8 snow."),
        new("AMBIENT PRECIP RATE", "millimeters", "Precipitation rate."),
        new("TITLE", "string", "aircraft.cfg title, max 128 characters. Requested separately.")
    ];
}

public readonly record struct SimVarDef(string Name, string Unit, string Note);
