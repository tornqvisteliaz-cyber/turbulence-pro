using TurbulencePro.Core.Sim;

namespace TurbulencePro.SimConnect;

public enum LinkState
{
    Disconnected,
    Connecting,
    Connected,
    Failed
}

/// <summary>
/// Live source. The managed SDK client is compiled only when SIMCONNECT is defined
/// and Microsoft.FlightSimulator.SimConnect.dll is referenced. See BUILD.md.
/// This type is the always-available shell: it reports disconnected until that build.
/// </summary>
public sealed class SimConnectDataSource : IAircraftDataSource
{
    public bool IsConnected => false;
    public string Status { get; } = "SimConnect DLL not referenced. Set SimConnectManagedDll and define SIMCONNECT. Test mode does not need it.";
    public FlightSample? Latest => null;
    public event Action<FlightSample>? SampleReceived { add { } remove { } }
    public void Start() { }
    public void Stop() { }
}
