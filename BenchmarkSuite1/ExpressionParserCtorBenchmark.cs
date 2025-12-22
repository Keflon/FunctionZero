using BenchmarkDotNet.Attributes;
using FunctionZero.ExpressionParserZero.Parser;
using Microsoft.VSDiagnostics;

namespace FunctionZero.Benchmarks
{
    [CPUUsageDiagnoser]
    public class ExpressionParserCtorBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {
        // No setup required for ctor benchmark
        }

        [Benchmark]
        public ExpressionParser Ctor()
        {
            return new ExpressionParser();
        }
    }
}