using BenchmarkDotNet.Attributes;
using D2SLib;

namespace D2SLib_Benchmark;

[Config(typeof(BenchmarkConfig))]
public class LoadGame
{
    private byte[] _saveData = Array.Empty<byte>();

    [Benchmark]
    public void LoadComplexSave()
    {
        _ = Core.ReadD2S(_saveData);
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _saveData = File.ReadAllBytes(@"Resources\D2S\1.15\DannyIsGreat.d2s");
    }
}
