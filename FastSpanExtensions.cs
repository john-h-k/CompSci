using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CorePlayground
{
    public static class FastSpanExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe void FastFillNoBranch<T>(this Span<T> span, T value)
        {
            if (Avx2.IsSupported)
            {
                int size = Unsafe.SizeOf<T>();
                if ((size & (size - 1)) == 0 && size < 32)
                {
                    // Is pow of 2

                    Vector256<byte> vector;
                    switch (size)
                    {
                        case 1:
                            vector = Vector256.Create(Unsafe.As<T, byte>(ref value));
                            break;
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
                            Vector128<ulong> tmp = Vector128.Create(
                                Unsafe.As<T, ulong>(ref value),
                                Unsafe.As<T, ulong>(ref Unsafe.Add(ref value, 8)));

                            vector = Avx2.BroadcastScalarToVector256(tmp).AsByte();
                            break;
                        case 32:
                        {
                            vector = Vector256.Create(
                                Unsafe.As<T, ulong>(ref value),
                                Unsafe.As<T, ulong>(ref Unsafe.AddByteOffset(ref value, (IntPtr) 8)),
                                Unsafe.As<T, ulong>(ref Unsafe.AddByteOffset(ref value, (IntPtr) 16)),
                                Unsafe.As<T, ulong>(ref Unsafe.AddByteOffset(ref value, (IntPtr) 24))).AsByte();
                            break;
                        }

                        default:
                            vector = Vector256<byte>.Zero;
                            break;
                    }

                    ref byte start = ref Unsafe.As<T, byte>(ref span[0]);
                    fixed (byte* pAliasedVector = &start)
                    {
                        for (var i = 0; i < span.Length; i += 32)
                        {
                            Avx2.Store(pAliasedVector + i, vector);
                        }
                    }
                }
            }
            else
            {
                if (value.Equals(default(T)))
                {
                    Unsafe.InitBlockUnaligned(
                        ref Unsafe.As<T, byte>(ref span[0]),
                        0,
                        (uint) (span.Length * Unsafe.SizeOf<T>()));
                }
                else
                {
                    ThrowHelper.ThrowPlatformNotSupportedException();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe void FastFill<T>(this Span<T> span, T value)
        {
            if (Avx2.IsSupported)
            {
                if (FastMemoryExtensions.UnsafeMemCompareZero(Unsafe.AsPointer(ref value), Unsafe.SizeOf<T>()))
                {
                    Vector256<byte> vector = Vector256<byte>.Zero;

                    ref byte start = ref Unsafe.As<T, byte>(ref span[0]);
                    fixed (byte* pAliasedVector = &start)
                    {
                        for (var i = 0; i < span.Length; i += 32)
                        {
                            Avx2.Store(pAliasedVector, vector);
                        }
                    }
                }
                else
                {

                    int size = Unsafe.SizeOf<T>();
                    if ((size & (size - 1)) == 0 && size < 32)
                    {
                        // Is pow of 2

                        Vector256<byte> vector;
                        switch (size)
                        {
                            case 1:
                                vector = Vector256.Create(Unsafe.As<T, byte>(ref value));
                                break;
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
                            {
                                Vector128<ulong> tmp = Vector128.Create(
                                    Unsafe.As<T, ulong>(ref value),
                                    Unsafe.As<T, ulong>(ref Unsafe.Add(ref value, 8)));

                                vector = Avx2.BroadcastScalarToVector256(tmp).AsByte();
                                break;
                            }
                            case 32:
                            {
                                vector = Vector256.Create(
                                    Unsafe.As<T, ulong>(ref value),
                                    Unsafe.As<T, ulong>(ref Unsafe.Add(ref value, 8)),
                                    Unsafe.As<T, ulong>(ref Unsafe.Add(ref value, 16)),
                                    Unsafe.As<T, ulong>(ref Unsafe.Add(ref value, 24))).AsByte();
                                break;
                            }

                            default:
                                vector = Vector256<byte>.Zero;
                                break;
                        }

                        ref byte start = ref Unsafe.As<T, byte>(ref span[0]);
                        fixed (byte* pAliasedVector = &start)
                        {
                            for (var i = 0; i < span.Length; i += 32)
                            {
                                Avx2.Store(pAliasedVector + i, vector);
                            }
                        }
                    }
                }
            }
            else
            {
                if (value.Equals(default(T)))
                {
                    Unsafe.InitBlockUnaligned(
                        ref Unsafe.As<T, byte>(ref span[0]),
                        0,
                        (uint) (span.Length * Unsafe.SizeOf<T>()));
                }
                else
                {
                    ThrowHelper.ThrowPlatformNotSupportedException();
                }
            }
        }

        private static int PrevPow2(int val)
        {
            val |= (val >> 1);
            val |= (val >> 2);
            val |= (val >> 4);
            val |= (val >> 8);
            val |= (val >> 16);
            return val - (val >> 1);
        }

        public static bool FastSequenceEqual(this Span<byte> span, Span<byte> other)
        {
            throw null;
        }
    }
}

internal static class ThrowHelper
{
    public static void Throw(Exception e) => throw e;

    public static void ThrowPlatformNotSupportedException()
        => throw new PlatformNotSupportedException();
}