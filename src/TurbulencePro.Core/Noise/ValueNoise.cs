namespace TurbulencePro.Core.Noise;

/// <summary>
/// Continuous value noise. Samples are a function of time, not independent random draws.
/// </summary>
public sealed class ValueNoise
{
    private readonly int[] _perm = new int[512];

    public ValueNoise(int seed)
    {
        var rnd = new Random(seed);
        var p = Enumerable.Range(0, 256).ToArray();
        for (var i = 255; i > 0; i--)
        {
            var j = rnd.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }
        for (var i = 0; i < 512; i++)
            _perm[i] = p[i & 255];
    }

    public double Sample(double t)
    {
        var x = t;
        var x0 = (int)Math.Floor(x);
        var xf = x - x0;
        var u = xf * xf * (3.0 - 2.0 * xf);
        var a = Hash(x0);
        var b = Hash(x0 + 1);
        return a + (b - a) * u;
    }

    public double Fractal(double t, int octaves, double lacunarity, double gain)
    {
        var sum = 0.0;
        var amp = 1.0;
        var freq = 1.0;
        var norm = 0.0;
        for (var i = 0; i < octaves; i++)
        {
            sum += Sample(t * freq) * amp;
            norm += amp;
            amp *= gain;
            freq *= lacunarity;
        }
        return norm <= 0 ? 0 : sum / norm;
    }

    private double Hash(int i)
    {
        var h = _perm[i & 511];
        return h / 127.5 - 1.0;
    }
}
