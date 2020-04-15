using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace TiffLibrary.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }
}
