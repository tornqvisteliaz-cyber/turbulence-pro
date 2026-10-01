using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using TurbulencePro.Audio;
using TurbulencePro.Core.Engine;
using TurbulencePro.Core.Profiles;
using TurbulencePro.Core.Recording;
using TurbulencePro.Core.Sim;
using TurbulencePro.Core.Turbulence;

namespace TurbulencePro.App;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SimulationLoop _loop = new();
    private readonly ProceduralAudioEngine _audio = new();
    private readonly FlightRecorder _recorder = new();
    private readonly ProfileManager _profiles = ProfileManager.CreateWithStarters();
    private readonly DateTime _started = DateTime.UtcNow;
    private int _uiTick;

    public MainViewModel()
    {
        _loop.Profile = _profiles.Resolve("");
        Profiles = new ObservableCollection<AircraftProfile>(_profiles.All);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        timer.Tick += (_, _) => OnTick();
        timer.Start();
    }

    public ObservableCollection<AircraftProfile> Profiles { get; }
    public ObservableCollection<double> MeasuredHistory { get; } = new();
    public ObservableCollection<double> SimulatedHistory { get; } = new();
    public ObservableCollection<double> FinalHistory { get; } = new();
    public ObservableCollection<string> LogLines { get; } = new();

    public string ConnectionStatus { get; private set; } = "TEST MODE — not connected to MSFS";
    public string Aircraft { get; private set; } = "Generic Airliner";
    public string Phase { get; private set; } = "CRUISE";
    public string AltitudeText { get; private set; } = "35000 ft";
    public string IasText { get; private set; } = "280 kt";
    public string WindText { get; private set; } = "280/20";
    public string MeasuredText { get; private set; } = "0%";
    public string SimulatedText { get; private set; } = "0%";
    public string FinalText { get; private set; } = "0%";
    public string ClassText { get; private set; } = "CALM";
    public string ConfidenceText { get; private set; } = "0%";
    public string Reasons { get; private set; } = "";
    public string DebugText { get; private set; } = "";
    public string AudioText { get; private set; } = "";

    public double Altitude { get; set; } = 35000;
    public double Agl { get; set; } = 34000;
    public double Ias { get; set; } = 280;
    public double VerticalSpeedFpm { get; set; }
    public double WindSpeed { get; set; } = 20;
    public double WindDirection { get; set; } = 280;
    public double WindVariation { get; set; } = 2;
    public double WeightPounds { get; set; } = 140000;
    public bool OnGround { get; set; }
    public bool InCloud { get; set; }
    public bool Precipitation { get; set; }
    public double Drive { get; set; } = 0.45;
    public bool Recording { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Force(TurbulenceClass klass)
    {
        _loop.Estimator.ForcedClass = klass;
        AddLog("Manual class " + klass + " — simulated, not simulator turbulence");
    }

    public void ClearForce() => _loop.Estimator.ForcedClass = null;

    public void SelectProfile(AircraftProfile profile)
    {
        _loop.Profile = profile;
        Aircraft = profile.Aircraft + " (perceptual profile)";
        AddLog("Profile " + profile.Id);
    }

    public void ExportLog(string path)
    {
        File.WriteAllLines(path, LogLines);
    }

    public void SaveRecording(string path) => _recorder.Save(path);

    private void OnTick()
    {
        _loop.Estimator.SimulatedDrive = Drive;
        var sample = BuildSample();
        var step = _loop.Step(sample, 0.033);
        var levels = _audio.Update(step.Estimate.FinalIntensity, step.Effects.Vibration, step.ActiveEvent, 0.033);
        if (Recording)
            _recorder.Add(step);
        _uiTick++;
        if (_uiTick % 3 != 0)
            return;
        Phase = step.Phase.ToString().ToUpperInvariant();
        AltitudeText = $"{sample.AltitudeFeet:0} ft";
        IasText = $"{sample.IndicatedAirspeedKnots:0} kt";
        WindText = $"{sample.AmbientWindDirectionDegrees:0}/{sample.AmbientWindSpeedKnots:0}";
        MeasuredText = Pct(step.Estimate.MeasuredIntensity);
        SimulatedText = Pct(step.Estimate.SimulatedIntensity);
        FinalText = Pct(step.Estimate.FinalIntensity);
        ClassText = step.Estimate.Class.ToString().ToUpperInvariant();
        ConfidenceText = Pct(step.Estimate.Confidence);
        Reasons = string.Join(", ", step.Estimate.ReasonCodes);
        Push(MeasuredHistory, step.Estimate.MeasuredIntensity);
        Push(SimulatedHistory, step.Estimate.SimulatedIntensity);
        Push(FinalHistory, step.Estimate.FinalIntensity);
        DebugText =
            $"HP accel X/Y/Z {step.Measured.HighPassAccelX:0.00} {step.Measured.HighPassAccelY:0.00} {step.Measured.HighPassAccelZ:0.00}\n" +
            $"RMS {step.Measured.FilteredAccelRms:0.00} ft/s^2  VS rate {step.Measured.FilteredVerticalSpeedRate:0.00}\n" +
            $"Gust V/L/Lon {step.Gust.Vertical:0.00} {step.Gust.Lateral:0.00} {step.Gust.Longitudinal:0.00}\n" +
            $"Event {step.ActiveEvent}\n" +
            $"Camera: null implementation  Wake: null implementation";
        AudioText = $"rumble {levels.Rumble:0.00}  airframe {levels.Airframe:0.00}  rattle {levels.Rattle:0.00}  creak {levels.Creak:0.00}  impact {levels.Impact:0.00}";
        NotifyAll();
    }

    private FlightSample BuildSample()
    {
        var t = (DateTime.UtcNow - _started).TotalSeconds;
        var variation = Math.Sin(t * 0.3) * WindVariation;
        return new FlightSample
        {
            TimestampUtc = DateTime.UtcNow,
            AircraftTitle = _loop.Profile.Aircraft,
            AltitudeFeet = Altitude,
            AglFeet = Agl,
            IndicatedAirspeedKnots = Ias,
            GroundSpeedKnots = OnGround ? Math.Min(Ias, 30) : Ias + 80,
            VerticalSpeedFeetPerSecond = VerticalSpeedFpm / 60.0,
            AccelerationBodyYFeetPerSec2 = 32.2,
            AmbientWindSpeedKnots = Math.Max(0, WindSpeed + variation),
            AmbientWindDirectionDegrees = WindDirection,
            AmbientWindXMps = (WindSpeed + variation) * 0.5144,
            TotalWeightPounds = WeightPounds,
            OnGround = OnGround,
            InCloud = InCloud,
            PrecipStateMask = Precipitation ? 4 : 2
        };
    }

    private void Push(ObservableCollection<double> series, double value)
    {
        series.Add(value);
        while (series.Count > 180)
            series.RemoveAt(0);
    }

    private void AddLog(string line)
    {
        LogLines.Add(DateTime.Now.ToString("HH:mm:ss ") + line);
        while (LogLines.Count > 200)
            LogLines.RemoveAt(0);
    }

    private static string Pct(double v) => $"{v * 100:0}%";

    private void NotifyAll()
    {
        foreach (var name in new[]
        {
            nameof(ConnectionStatus), nameof(Aircraft), nameof(Phase), nameof(AltitudeText), nameof(IasText),
            nameof(WindText), nameof(MeasuredText), nameof(SimulatedText), nameof(FinalText), nameof(ClassText),
            nameof(ConfidenceText), nameof(Reasons), nameof(DebugText), nameof(AudioText)
        })
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
