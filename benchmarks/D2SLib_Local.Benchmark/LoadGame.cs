using BenchmarkDotNet.Attributes;
using D2SLib;
using D2SLib.Model.Save;

namespace D2SLib_Benchmark;

[Config(typeof(BenchmarkConfig))]
public class LoadGame
{
    private byte[] _saveData = Array.Empty<byte>();
    private D2S? _saveGame;

    [Benchmark]
    public void LoadOnly()
    {
        _ = Core.ReadD2S(_saveData);
    }

    [Benchmark]
    public void SaveOnly()
    {
        _ = Core.WriteD2S(_saveGame!);
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _saveData = File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s");
        _saveGame = Core.ReadD2S(_saveData);
    }
}
