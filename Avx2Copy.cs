using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using ObjPointer;


// apologies; have used this for a bunch of stuff might have some bloat code

namespace CorePlayground
{
    internal static class Program
    {

        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<MemCopyBenchmark>();
            //var bench = new MemCopyBenchmark();
            //bench.Avx2BlockCopy();
            //Console.ReadKey();
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 1024, Pack = 32)]
    public struct KbBlock
    {
        public unsafe fixed byte Buffer[1024];
    }

    [StructLayout(LayoutKind.Sequential, Size = 1024 * 64, Pack = 32)]
    public struct StreamBlock
    {
        public unsafe fixed byte Buffer[1024 * 64];
    }

    [CoreJob(true)]
    [RPlotExporter, RankColumn]
    public class MemCopyBenchmark
    {

        private byte[] _src;
        private byte[] _dest;

        public KbBlock AlignedSrc;
        public KbBlock AlignedDest;

        public StreamBlock AlignedBlockSrc;
        public StreamBlock AlignedBlockDest;

        [GlobalSetup]
        public unsafe void Setup()
        {
            _dest = new byte[sizeof(StreamBlock)];
            _src = new byte[sizeof(StreamBlock)];
        }

        [Benchmark] // TODO slow :(
        public unsafe void Avx2BlockCopy()
        {
            fixed (StreamBlock* _srcPtr = &AlignedBlockSrc)
            fixed (StreamBlock* _destPtr = &AlignedBlockDest)
            {
                byte* srcPtr = (byte*)_srcPtr;
                byte* destPtr = (byte*)_destPtr;

                long requiredMoves = sizeof(StreamBlock) / 32;

                Vector256<byte> fVector = Avx.LoadVector256(srcPtr);
                Avx.Store(destPtr, fVector);
                requiredMoves--;

                destPtr = (byte*)(((ulong)destPtr + 31) & ~31UL); // Up to next 32 byte boundary
                srcPtr += destPtr - (byte*)_destPtr;

                while (requiredMoves != 0)
                {
                    Vector256<byte> vector = Avx.LoadVector256(srcPtr);
                    Avx.StoreAlignedNonTemporal(destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    requiredMoves--;
                }
            }
        }

        [Benchmark]
        public unsafe void BufferMemoryCopy()
        {
            fixed (StreamBlock* srcPtr = &AlignedBlockSrc)
            fixed (StreamBlock* destPtr = &AlignedBlockDest)
            {
                Buffer.MemoryCopy(
                    srcPtr,
                    destPtr,
                    1024 * 64,
                    1024 * 64);
            }
        }
        
        [Benchmark]
        public void BufferBlockCopy()
            => Buffer.BlockCopy(_src, 0, _dest, 0, _src.Length);

        [Benchmark]
        public unsafe void UnsafeBlockCopy()
            => Unsafe.CopyBlock(ref AlignedDest.Buffer[0], ref AlignedSrc.Buffer[0], 1024 * 64);

        [Benchmark]
        public unsafe void ByteForLoopCopy()
        {
            fixed (KbBlock* srcPtr = &AlignedSrc)
            fixed (KbBlock* destPtr = &AlignedDest)
            {
                var srcBPtr = (byte*)srcPtr;
                var destBPtr = (byte*)destPtr;
                for (var i = 0; i < 1024 * 64; i++)
                {
                    destBPtr[i] = srcBPtr[i];
                }
            }
        }

        [Benchmark]
        public unsafe StreamBlock ByteArrayToStructure()
        {

            int structLen = Marshal.SizeOf<StreamBlock>();
            if (_src.Length < structLen)
            {
                throw new ArgumentOutOfRangeException();
            }


            fixed (byte* pBytes = _src)
            {
                return Marshal.PtrToStructure<StreamBlock>((IntPtr)(pBytes));
            }

        }

        private static void Throws() => throw new ArgumentOutOfRangeException();

        [Benchmark]
        public unsafe StreamBlock ByteArrayToStructure2()
        {
            if (_src.Length < sizeof(StreamBlock))
            {
                Throws();
            }

            fixed (byte* pBytes = _src)
            {
                return *(StreamBlock*)pBytes;
            }
        }

        [Benchmark]
        public void BlockForLoopCopy() => AlignedBlockSrc = AlignedBlockDest;
    }
}
