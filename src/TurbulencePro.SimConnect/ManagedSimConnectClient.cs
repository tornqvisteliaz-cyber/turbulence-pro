#if SIMCONNECT
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using TurbulencePro.Core.Sim;

namespace TurbulencePro.SimConnect;

/// <summary>
/// Official managed client. Data is copied out of the callback. No effect work runs here.
/// </summary>
public sealed class ManagedSimConnectClient : IAircraftDataSource, IDisposable
{
    private const int WmUser = 0x0402;
    private Microsoft.FlightSimulator.SimConnect.SimConnect? _sim;
    private readonly object _gate = new();
    public bool IsConnected { get; private set; }
    public string Status { get; private set; } = "disconnected";
    public FlightSample? Latest { get; private set; }
    public event Action<FlightSample>? SampleReceived;

    public void Start()
    {
        Status = "connecting";
    }

    public void Attach(IntPtr windowHandle)
    {
        _sim = new Microsoft.FlightSimulator.SimConnect.SimConnect("Turbulence Pro", windowHandle, WmUser, null, 0);
        _sim.OnRecvOpen += (_, _) => { IsConnected = true; Status = "connected"; Define(); };
        _sim.OnRecvQuit += (_, _) => { IsConnected = false; Status = "sim-quit"; };
        _sim.OnRecvException += (_, e) => Status = "exception:" + e.eException;
        _sim.OnRecvSimobjectData += OnData;
    }

    public void Receive() => _sim?.ReceiveMessage();

    private void Define()
    {
        if (_sim is null) return;
        foreach (var def in SimVarCatalog.Data.Where(d => d.Unit != "string"))
            _sim.AddToDataDefinition(DefineId.Sample, def.Name, def.Unit, SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
        _sim.RegisterDataDefineStruct<SampleBlock>(DefineId.Sample);
        _sim.RequestDataOnSimObject(RequestId.Sample, DefineId.Sample, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 3, 0);
        _sim.AddToDataDefinition(DefineId.Title, "TITLE", null, SIMCONNECT_DATATYPE.STRING128, 0, SimConnect.SIMCONNECT_UNUSED);
        _sim.RegisterDataDefineStruct<TitleBlock>(DefineId.Title);
        _sim.RequestDataOnSimObject(RequestId.Title, DefineId.Title, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SECOND, 0, 0, 0, 0);
    }

    private void OnData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        if (data.dwRequestID == (uint)RequestId.Sample && data.dwData[0] is SampleBlock block)
        {
            var sample = block.ToSample(Latest?.AircraftTitle ?? "");
            lock (_gate) Latest = sample;
            SampleReceived?.Invoke(sample);
        }
    }

    public void Stop() { Dispose(); }
    public void Dispose() { _sim?.Dispose(); _sim = null; IsConnected = false; }

    private enum DefineId { Sample, Title }
    private enum RequestId { Sample, Title }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SampleBlock
    {
        public double Altitude, Agl, Ias, Gs, Vs, Pitch, Bank, Heading;
        public double Ax, Ay, Az, WindX, WindY, WindZ, WindSpeed, WindDir;
        public double AcWindX, AcWindY, AcWindZ, Weight, OnGround, InCloud, Precip, PrecipRate;
        public FlightSample ToSample(string title) => new()
        {
            AircraftTitle = title,
            AltitudeFeet = Altitude,
            AglFeet = Agl,
            IndicatedAirspeedKnots = Ias,
            GroundSpeedKnots = Gs,
            VerticalSpeedFeetPerSecond = Vs,
            PitchRadians = Pitch,
            BankRadians = Bank,
            HeadingRadians = Heading,
            AccelerationBodyXFeetPerSec2 = Ax,
            AccelerationBodyYFeetPerSec2 = Ay,
            AccelerationBodyZFeetPerSec2 = Az,
            AmbientWindXMps = WindX,
            AmbientWindYMps = WindY,
            AmbientWindZMps = WindZ,
            AmbientWindSpeedKnots = WindSpeed,
            AmbientWindDirectionDegrees = WindDir,
            AircraftWindXKnots = AcWindX,
            AircraftWindYKnots = AcWindY,
            AircraftWindZKnots = AcWindZ,
            TotalWeightPounds = Weight,
            OnGround = OnGround > 0.5,
            InCloud = InCloud > 0.5,
            PrecipStateMask = (int)Precip,
            PrecipRateMillimetres = PrecipRate
        };
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    private struct TitleBlock
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Title;
    }
}
#endif
