using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Attributes;

namespace ISO8583NetBenchmark
{
    [MemoryDiagnoser]
    //[EtwProfiler] //Create traces for perfview
    //[SimpleJob(RuntimeMoniker.NetCoreApp21)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [SimpleJob(RunStrategy.Throughput, targetCount: 30, id: "MonitoringJob")]
    //[MinColumn, Q1Column, Q3Column, MaxColumn]
    public class ASCIIBcdtilsTest
    {        
        private byte[] packedBytes;
        private string stringhex = "29001234567890123456193012121959";
        [GlobalSetup]
        public void GlobalSetup()
        {
            
            packedBytes = new byte[256];
            int index = 0;
            ISO8583Net.Utilities.ISOUtils.Ascii2Bcd(stringhex, packedBytes, ref index, ISO8583Net.Types.ISOFieldPadding.LEFT);
        }

        [Benchmark(Baseline = true)]
        public byte[] Ascii2BcdOriginal()
        {
            int index = 0;
            ISO8583Net.Utilities.ISOUtils.Ascii2BcdOld(stringhex, packedBytes, ref index, ISO8583Net.Types.ISOFieldPadding.LEFT);
            return packedBytes;
        }

        [Benchmark]
        public byte[] Ascii2Bcd()
        {
            int index = 0;
            ISO8583Net.Utilities.ISOUtils.Ascii2Bcd(stringhex, packedBytes, ref index, ISO8583Net.Types.ISOFieldPadding.LEFT);
            return packedBytes;
        }

     

    }
    [MemoryDiagnoser]
    //[EtwProfiler] //Create traces for perfview
    //[SimpleJob(RuntimeMoniker.NetCoreApp21)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [SimpleJob(RunStrategy.Throughput, targetCount: 30, id: "MonitoringJob")]
    //[MinColumn, Q1Column, Q3Column, MaxColumn]
    public class HexUtilsTest
    {
        private byte[] bytes;
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            string stringhex = "29001234567890123456193012121959";
            bytes = ISO8583Net.Utilities.ISOUtils.Hex2Bytes(stringhex);
            ISO8583Net.Utilities.ISOUtils.Bytes2HexOld(bytes, bytes.Length);
        }

        [Benchmark(Baseline =true)]
        public string Bytes2HexOriginal()
        {
            return ISO8583Net.Utilities.ISOUtils.Bytes2HexOld(bytes, bytes.Length);            
        }

        [Benchmark]
        public string Bytes2HexSpan()
        {            
            return ISO8583Net.Utilities.ISOUtils.Bytes2HexSpan(bytes, bytes.Length);
        }


        [Benchmark]
        public string Bytes2HexStringCreate()
        {
            return ISO8583Net.Utilities.ISOUtils.Bytes2Hex(bytes, bytes.Length);
        }

    }
}
