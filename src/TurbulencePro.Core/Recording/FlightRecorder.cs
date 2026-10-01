using System.Globalization;
using System.Text;
using TurbulencePro.Core.Engine;
using TurbulencePro.Core.Sim;

namespace TurbulencePro.Core.Recording;

/// <summary>
/// CSV recording. Header is the schema. Replay feeds the same SimulationLoop as live data.
/// </summary>
public sealed class FlightRecorder
{
    public const string Header =
        "timestampUtc,aircraft,altitudeFeet,aglFeet,iasKnots,groundSpeedKnots,verticalSpeedFps,pitchRad,bankRad,headingRad,accelXFps2,accelYFps2,accelZFps2,windXMps,windYMps,windZMps,windSpeedKnots,windDirDeg,onGround,inCloud,precipMask,weightPounds,measured,simulated,final,class,event";

    private readonly List<string> _rows = [];

    public int Count => _rows.Count;

    public void Add(EngineStep step)
    {
        var s = step.Sample;
        var e = step.Estimate;
        _rows.Add(string.Join(',',
            s.TimestampUtc.ToString("o", CultureInfo.InvariantCulture),
            Csv(s.AircraftTitle),
            F(s.AltitudeFeet), F(s.AglFeet), F(s.IndicatedAirspeedKnots), F(s.GroundSpeedKnots),
            F(s.VerticalSpeedFeetPerSecond), F(s.PitchRadians), F(s.BankRadians), F(s.HeadingRadians),
            F(s.AccelerationBodyXFeetPerSec2), F(s.AccelerationBodyYFeetPerSec2), F(s.AccelerationBodyZFeetPerSec2),
            F(s.AmbientWindXMps), F(s.AmbientWindYMps), F(s.AmbientWindZMps),
            F(s.AmbientWindSpeedKnots), F(s.AmbientWindDirectionDegrees),
            s.OnGround ? 1 : 0, s.InCloud ? 1 : 0, s.PrecipStateMask, F(s.TotalWeightPounds),
            F(e.MeasuredIntensity), F(e.SimulatedIntensity), F(e.FinalIntensity),
            e.Class, Csv(step.ActiveEvent)));
    }

    public void Save(string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header);
        foreach (var row in _rows)
            sb.AppendLine(row);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, sb.ToString());
    }

    public static IReadOnlyList<FlightSample> LoadSamples(string path)
    {
        var lines = File.ReadAllLines(path);
        var list = new List<FlightSample>();
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var c = Split(line);
            list.Add(new FlightSample
            {
                TimestampUtc = DateTime.Parse(c[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                AircraftTitle = c[1],
                AltitudeFeet = D(c[2]),
                AglFeet = D(c[3]),
                IndicatedAirspeedKnots = D(c[4]),
                GroundSpeedKnots = D(c[5]),
                VerticalSpeedFeetPerSecond = D(c[6]),
                PitchRadians = D(c[7]),
                BankRadians = D(c[8]),
                HeadingRadians = D(c[9]),
                AccelerationBodyXFeetPerSec2 = D(c[10]),
                AccelerationBodyYFeetPerSec2 = D(c[11]),
                AccelerationBodyZFeetPerSec2 = D(c[12]),
                AmbientWindXMps = D(c[13]),
                AmbientWindYMps = D(c[14]),
                AmbientWindZMps = D(c[15]),
                AmbientWindSpeedKnots = D(c[16]),
                AmbientWindDirectionDegrees = D(c[17]),
                OnGround = c[18] == "1",
                InCloud = c[19] == "1",
                PrecipStateMask = int.Parse(c[20], CultureInfo.InvariantCulture),
                TotalWeightPounds = D(c[21])
            });
        }
        return list;
    }

    private static string F(double v) => v.ToString("0.####", CultureInfo.InvariantCulture);
    private static double D(string v) => double.Parse(v, CultureInfo.InvariantCulture);
    private static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    private static string[] Split(string line)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var quoted = false;
        foreach (var ch in line)
        {
            if (ch == '"') { quoted = !quoted; continue; }
            if (ch == ',' && !quoted) { parts.Add(current.ToString()); current.Clear(); continue; }
            current.Append(ch);
        }
        parts.Add(current.ToString());
        return parts.ToArray();
    }
}
