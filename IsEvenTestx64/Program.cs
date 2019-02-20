namespace Test
{
  internal static class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<EvenBenchmark>();
            Console.ReadKey();
        }
    }

    [CoreJob(true)]
    [RPlotExporter]
    [RankColumn]
    public class EvenBenchmark
    {
        [Benchmark]
        [Arguments(12312312312)]
        [DllImport(@"C:\Users\johnk\source\repos\CppPlayground\x64\Release\CppPlayground.exe",
            CallingConvention = CallingConvention.StdCall)]
        public static extern long UsesBitwise(ulong num = 12312312313);

        [Benchmark]
        [Arguments(12312312312)]
        [DllImport(@"C:\Users\johnk\source\repos\CppPlayground\x64\Release\CppPlayground.exe",
        CallingConvention = CallingConvention.StdCall)]
        public static extern long UsesTest(ulong num = 12312312313);

        [Benchmark]
        [Arguments(12312312312)]
        [DllImport(@"C:\Users\johnk\source\repos\CppPlayground\x64\Release\CppPlayground.exe",
        CallingConvention = CallingConvention.StdCall)]
        public static extern ulong UsesDiv(ulong num = 12312312313);
    }
}
