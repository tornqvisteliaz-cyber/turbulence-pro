using TurbulencePro.Core.Sim;

namespace TurbulencePro.SimConnect;

public interface IAircraftDataSource
{
    bool IsConnected { get; }
    string Status { get; }
    FlightSample? Latest { get; }
    event Action<FlightSample>? SampleReceived;
    void Start();
    void Stop();
}

public sealed class ReplayDataSource : IAircraftDataSource
{
    private readonly IReadOnlyList<FlightSample> _samples;
    private int _index;
    private Timer? _timer;
    public bool IsConnected { get; private set; }
    public string Status { get; private set; } = "replay-idle";
    public FlightSample? Latest { get; private set; }
    public event Action<FlightSample>? SampleReceived;

    public ReplayDataSource(IReadOnlyList<FlightSample> samples) => _samples = samples;

    public void Start()
    {
        IsConnected = true;
        Status = "replay";
        _timer = new Timer(_ => Pump(), null, 0, 50);
    }

    public void Stop()
    {
        _timer?.Dispose();
        IsConnected = false;
        Status = "replay-stopped";
    }

    private void Pump()
    {
        if (_index >= _samples.Count)
        {
            Stop();
            return;
        }
        Latest = _samples[_index++];
        SampleReceived?.Invoke(Latest);
    }
}
