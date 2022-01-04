using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// When running memory profiler, comment out the above and uncomment the following.

//var lg = new D2SLib_Benchmark.LoadGame();
//lg.GlobalSetup();

//for (int i = 0; i < 10_000; i++)
//{
//    lg.SaveOnly();
//}
