using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;


// apologies; have used this for a bunch of stuff might have some bloat code

namespace CorePlayground
{
    internal static class Program
    {
        private static unsafe void Main(string[] args)
        {
            BenchmarkRunner.Run<MemCopyBenchmark>();
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

    public struct SbWrapper
    {
        public unsafe fixed byte Leading32[32];
        public StreamBlock StreamBlock;
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

        public SbWrapper UnalignedBlockSrc;
        public SbWrapper UnalignedBlockDest;

        [GlobalSetup]
        public unsafe void Setup()
        {
            _src = new byte[1024 * 64];
            _dest = new byte[_src.Length];

            AlignedSrc = default;
            AlignedDest = default;

            SbWrapper alignmentSrc = default;
            SbWrapper* srcPtr = &alignmentSrc;
            SbWrapper alignmentDest = default;
            SbWrapper* destPtr = &alignmentDest;

            var counter = 0;

            for (var i = 0; i < _src.Length; i++)
            {
                _src[i] = (byte)counter++;
            }

            counter = 0;

            fixed (KbBlock* avPtr = &AlignedSrc)
            {
                for (var i = 0; i < 1024; i++)
                {
                    avPtr->Buffer[i] = (byte)counter++;
                }
            }
        }

        [Benchmark]
        public unsafe void Avx2BlockCopy()
        {
            fixed (SbWrapper* _srcPtr = &UnalignedBlockSrc)
            fixed (SbWrapper* _destPtr = &UnalignedBlockDest)
            {
                // TODO add unaligned first / last write instead of aligning
                ulong sPtr = ((UIntPtr)_srcPtr).ToUInt64();
                byte* srcPtr = (byte*)((sPtr + 32 - 1) / 32 * 32);

                ulong dPtr = ((UIntPtr)_destPtr).ToUInt64();
                byte* destPtr = (byte*)((dPtr + 32 - 1) / 32 * 32);

                long requiredMoves = sizeof(StreamBlock) / 32;

                while (requiredMoves != 0)
                {
                    // unrolled by factor of 8

                    Vector256<long> vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);
                    
                    srcPtr += 32;
                    destPtr += 32;

                    vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    vector = Avx2.LoadAlignedVector256NonTemporal((long*)srcPtr);
                    Avx.StoreAlignedNonTemporal((long*)destPtr, vector);

                    srcPtr += 32;
                    destPtr += 32;

                    requiredMoves -= 8;
                }
            }
        }

        //[Benchmark]
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

        // [Benchmark]
        public void BufferBlockCopy()
            => Buffer.BlockCopy(_src, 0, _dest, 0, _src.Length);

        [Benchmark]
        public unsafe void UnsafeBlockCopy()
            => Unsafe.CopyBlock(ref AlignedDest.Buffer[0], ref AlignedSrc.Buffer[0], 1024 * 64);

        //[Benchmark]
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
        public void BlockForLoopCopy() => AlignedBlockSrc = AlignedBlockDest;
    }
}
