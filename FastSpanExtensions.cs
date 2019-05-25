using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading;

#if BIT64
using nuint = System.UInt64;
using nint = System.Int64;
#else // BIT64
using nuint = System.UInt32;
using nint = System.Int32;
#endif // BIT64

namespace FastMemory
{
    public static class FastSpanExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void ForLoopFill<T>(this Span<T> span, T value)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = value;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe void FastFillVectorized<T>(this Span<T> span, T value)
        {
            if (span.IsEmpty) return;

            int size = Unsafe.SizeOf<T>();
            int len = span.Length;

            Debug.Assert(size > 0 && len > 0);

            if (size == 1)
                Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), (uint)span.Length);

            if (false && Avx.IsSupported && len >= 32)
            {
                AvxFill(span, value);
            }
            else if (false && Sse2.IsSupported && len >= 16)
            {
                Sse2Fill(span, value);
            }
            else
            {
                // If 'value' is 'default', it is all zeroes, and we can just zero the mem
                // using the well-optimized initblk op
                if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                    && size <= 128 /* too expensive to test more than 128 bytes */
                    && IsValueDefault(value))
                {
                    Unsafe.InitBlockUnaligned(
                        ref Unsafe.As<T, byte>(ref span[0]),
                        0,
                        (uint)(len * size));
                }
                else
                {
                    SoftwareFallback(span, value);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void AvxFill<T>(Span<T> span, T value)
        {
            int size = Unsafe.SizeOf<T>();
            int len = span.Length;

            if ((size & (size - 1)) == 0 && size <= 32)
            {
                // Is pow of 2

                Vector256<byte> vector;
                switch (size)
                {
                    case 2:
                        vector = Vector256.Create(Unsafe.As<T, ushort>(ref value)).AsByte();
                        break;
                    case 4:
                        vector = Vector256.Create(Unsafe.As<T, uint>(ref value)).AsByte();
                        break;
                    case 8:
                        vector = Vector256.Create(Unsafe.As<T, ulong>(ref value)).AsByte();
                        break;
                    case 16:
                        Vector128<byte> tmp = Unsafe.As<T, Vector128<byte>>(ref value);
                        vector = Avx2.BroadcastScalarToVector256(tmp);
                        break;
                    case 32:
                        vector = Unsafe.As<T, Vector256<byte>>(ref value);
                        break;
                    default:
                        return; // unreachable, necessary
                }

                var movSize = unchecked((uint)(size * len));

                ref byte start = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));
                fixed (byte* pAliasedVector = &start)
                {
                    for (var i = 0; i < (movSize & ~31U); i += 32)
                    {
                        Avx.Store(pAliasedVector + i, vector);
                    }

                    Avx.Store((pAliasedVector + movSize) - 32, vector);
                }
            }
            else
            {
                SoftwareFallback(span, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void Sse2Fill<T>(Span<T> span, T value)
        {
            int size = Unsafe.SizeOf<T>();
            int len = span.Length;

            if ((size & (size - 1)) == 0 && size <= 16)
            {
                // Is pow of 2

                Vector128<byte> vector;
                switch (size)
                {
                    case 2:
                        vector = Vector128.Create(Unsafe.As<T, ushort>(ref value)).AsByte();
                        break;
                    case 4:
                        vector = Vector128.Create(Unsafe.As<T, uint>(ref value)).AsByte();
                        break;
                    case 8:
                        vector = Vector128.Create(Unsafe.As<T, ulong>(ref value)).AsByte();
                        break;
                    case 16:
                        vector = Unsafe.As<T, Vector128<byte>>(ref value);
                        break;
                    default:
                        return; // unreachable, necessary
                }

                var movSize = unchecked((uint)(size * len));

                ref byte start = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));
                fixed (byte* pAliasedVector = &start)
                {
                    for (var i = 0; i < (movSize & ~15U); i += 16)
                    {
                        Sse2.Store(pAliasedVector + i, vector);
                    }

                    Sse2.Store((pAliasedVector + movSize) - 16, vector);
                }
            }
            else
            {
                SoftwareFallback(span, value);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void SoftwareFallback<T>(Span<T> span, T value)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe bool IsValueDefault<T>(T value)
        {
            int size = Unsafe.SizeOf<T>();
            T tmp = default;
            var tmpMemSpan = new Span<byte>(Unsafe.AsPointer(ref tmp), size);
            var valueMemSpan = new Span<byte>(Unsafe.AsPointer(ref value), size);

            // If 'value' is 'default', it is all zeroes, and we can just zero the mem
            return tmpMemSpan.SequenceEqual(valueMemSpan);
        }
    }
}