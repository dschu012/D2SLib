using BenchmarkDotNet.Attributes;
using D2SLib;

namespace D2SLib_Benchmark;

[Config(typeof(BenchmarkConfig))]
public class LoadGame
{
    private byte[] _saveData = Array.Empty<byte>();

    [Benchmark]
    public void LoadOnly()
    {
        _ = Core.ReadD2S(_saveData);
    }

    [Benchmark]
    public void LoadAndSave()
    {
        var saveGame = Core.ReadD2S(_saveData);
        _ = Core.WriteD2S(saveGame);
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _saveData = File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s");
    }
}
