namespace TurbulencePro.Audio;

public sealed class AudioMix
{
    public double Master { get; set; } = 0.6;
    public double Rumble { get; set; } = 0.7;
    public double Airframe { get; set; } = 0.5;
    public double Rattle { get; set; } = 0.4;
    public double Creak { get; set; } = 0.3;
    public double Impact { get; set; } = 0.5;
    public bool Enabled { get; set; } = true;
}

public sealed class AudioLevels
{
    public double Rumble { get; init; }
    public double Airframe { get; init; }
    public double Rattle { get; init; }
    public double Creak { get; init; }
    public double Impact { get; init; }
    public double Master { get; init; }
}

/// <summary>
/// Procedural layers. No samples. Levels follow the final effect with attack and decay.
/// </summary>
public sealed class ProceduralAudioEngine
{
    public AudioMix Mix { get; } = new();
    public AudioLevels Levels { get; private set; } = new();
    private double _rumble, _airframe, _rattle, _creak, _impact;
    private double _phase;

    public AudioLevels Update(double finalIntensity, double vibration, string activeEvent, double dt)
    {
        if (!Mix.Enabled)
        {
            Levels = new AudioLevels();
            return Levels;
        }
        dt = Math.Clamp(dt, 0.005, 0.1);
        var targetRumble = finalIntensity * Mix.Rumble * Mix.Master;
        var targetAir = Math.Abs(vibration) * Mix.Airframe * Mix.Master;
        _rumble = Approach(_rumble, targetRumble, dt, 0.4, 0.8);
        _airframe = Approach(_airframe, targetAir, dt, 0.15, 0.4);
        var impactHit = activeEvent.Length > 0 ? finalIntensity : 0;
        _impact = Math.Max(_impact * Math.Exp(-dt * 4), impactHit * Mix.Impact * Mix.Master);
        _rattle = Math.Max(_rattle * Math.Exp(-dt * 6), (finalIntensity > 0.45 ? finalIntensity : 0) * Mix.Rattle * Mix.Master * 0.5);
        _creak = Math.Max(_creak * Math.Exp(-dt * 1.5), (Math.Abs(vibration) > 0.3 ? Math.Abs(vibration) : 0) * Mix.Creak * Mix.Master);
        _phase += dt;
        Levels = new AudioLevels
        {
            Rumble = _rumble,
            Airframe = _airframe,
            Rattle = _rattle,
            Creak = _creak,
            Impact = _impact,
            Master = Mix.Master
        };
        return Levels;
    }

    public float[] Render(int sampleRate, int frames)
    {
        var buffer = new float[frames];
        if (!Mix.Enabled)
            return buffer;
        for (var i = 0; i < frames; i++)
        {
            var t = _phase + i / (double)sampleRate;
            var rumble = (float)(Math.Sin(2 * Math.PI * 40 * t) * _rumble * 0.25);
            var air = (float)(Math.Sin(2 * Math.PI * 120 * t + Math.Sin(t * 3)) * _airframe * 0.15);
            var rattle = (float)(((t * 17 % 1) - 0.5) * _rattle * 0.1);
            var creak = (float)(Math.Sin(2 * Math.PI * 180 * t) * Math.Exp(-((t % 1.4) * 3)) * _creak * 0.08);
            var impact = (float)(Math.Sin(2 * Math.PI * 70 * t) * _impact * 0.2);
            buffer[i] = Math.Clamp(rumble + air + rattle + creak + impact, -1f, 1f);
        }
        return buffer;
    }

    private static double Approach(double current, double target, double dt, double attack, double decay)
    {
        var tau = target > current ? attack : decay;
        return current + (target - current) * (1 - Math.Exp(-dt / tau));
    }
}
