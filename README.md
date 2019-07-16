# Update `Span<T>::Fill`

Currently `Span<T>::Fill` is poorly optimized except when `sizeof(T)` is `1` and sorely needs optimizing.
This PR replaces the method with a faster implementation. The rough pseudocode idea is

```cs
if (size == 1)
{
    Unsafe.InitBlockUnaligned();
}
else if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
{
    if (Avx.IsSupported && IsPowOf2(size) && size * len >= 32 && size <= 32) { /* Avx */ }
    else if (Sse2.IsSupported && IsPowOf2(size) && size * len >= 16 && size <= 16) { /* Sse2 */ }
    else Software();
}
else
{
    Software();
}
```

## Graphs

In this code, we tested `Fill` for 5 different types.
`byte`
`int`
`object`
`Block32`, a 32-byte unmanaged struct type
`Block768`, a 768-byte unmanaged struct type
`Block768Managed`, a 768-byte managed struct type

Each time it was tested with a 4096 element array, and filling with an arbitrary value

It was tested with the following code

`FastFillVectorized` is a custom optimized method that uses intrinsics where possible
`FastFillVectorizedAligned` is a custom optimized method that uses intrinsics where possible, and will use aligned operations where possible

```cs
[Benchmark]
public void ArrayFill()
{
    Array.Fill(_arr, default);
}

[Benchmark]
public void ArrayAsSpanFill()
{
    _arr.AsSpan().Fill(default);
}

[Benchmark]
public void ForLoopFill()
{
    _arr.AsSpan().ForLoopFill(default);
}

[Benchmark]
public void FastFill()
{
    _arr.AsSpan().FastFillVectorized(default);
}

[Benchmark]
public void FastFillAligned()
{
    _arr.AsSpan().FastFillVectorizedAligned(default);
}
```

Here are the performance numbers from this:

### byte

|          Method |        Mean |      Error |     StdDev | Ratio | Rank |
|---------------- |------------:|-----------:|-----------:|------:|-----:|
|       ArrayFill | 1,711.31 ns |  1.5362 ns |  1.1994 ns |  1.00 |    1 |
|                 |             |            |            |       |      |
| ArrayAsSpanFill |    44.18 ns |  0.0548 ns |  0.0486 ns |  1.00 |    1 |
|                 |             |            |            |       |      |
|     ForLoopFill | 1,696.46 ns | 12.3990 ns | 11.5980 ns |  1.00 |    1 |
|                 |             |            |            |       |      |
|        FastFill |    71.58 ns |  0.1858 ns |  0.1738 ns |  1.00 |    1 |
|                 |             |            |            |       |      |
| FastFillAligned |    44.25 ns |  0.0277 ns |  0.0216 ns |  1.00 |    1 |

### int

|          Method |       Mean |     Error |    StdDev | Ratio | Rank |
|---------------- |-----------:|----------:|----------:|------:|-----:|
|       ArrayFill | 1,710.1 ns | 1.6214 ns | 1.5166 ns |  1.00 |    1 |
|                 |            |           |           |       |      |
| ArrayAsSpanFill | 1,029.9 ns | 2.9844 ns | 2.7916 ns |  1.00 |    1 |
|                 |            |           |           |       |      |
|     ForLoopFill | 1,709.3 ns | 2.8009 ns | 2.6200 ns |  1.00 |    1 |
|                 |            |           |           |       |      |
|        FastFill |   277.0 ns | 0.5010 ns | 0.4441 ns |  1.00 |    1 |
|                 |            |           |           |       |      |
| FastFillAligned |   271.2 ns | 0.2063 ns | 0.1829 ns |  1.00 |    1 |

### object

|          Method |      Mean |     Error |    StdDev | Ratio | Rank |
|---------------- |----------:|----------:|----------:|------:|-----:|
|       ArrayFill |  6.198 us | 0.0190 us | 0.0178 us |  1.00 |    1 |
|                 |           |           |           |       |      |
| ArrayAsSpanFill | 17.044 us | 0.0767 us | 0.0717 us |  1.00 |    1 |
|                 |           |           |           |       |      |
|     ForLoopFill |  9.295 us | 0.0185 us | 0.0164 us |  1.00 |    1 |
|                 |           |           |           |       |      |
|        FastFill |  7.253 us | 0.0170 us | 0.0159 us |  1.00 |    1 |
|                 |           |           |           |       |      |
| FastFillAligned |  7.246 us | 0.0037 us | 0.0031 us |  1.00 |    1 |

### Block32

|          Method |     Mean |     Error |    StdDev | Ratio | Rank |
|---------------- |---------:|----------:|----------:|------:|-----:|
|       ArrayFill | 3.211 us | 0.0080 us | 0.0075 us |  1.00 |    1 |
|                 |          |           |           |       |      |
| ArrayAsSpanFill | 3.122 us | 0.0467 us | 0.0436 us |  1.00 |    1 |
|                 |          |           |           |       |      |
|     ForLoopFill | 3.182 us | 0.0426 us | 0.0399 us |  1.00 |    1 |
|                 |          |           |           |       |      |
|        FastFill | 2.181 us | 0.0064 us | 0.0060 us |  1.00 |    1 |
|                 |          |           |           |       |      |
| FastFillAligned | 2.160 us | 0.0010 us | 0.0010 us |  1.00 |    1 |

### Block768

|          Method |     Mean |     Error |    StdDev | Ratio | Rank |
|---------------- |---------:|----------:|----------:|------:|-----:|
|       ArrayFill | 92.52 us | 1.4776 us | 1.3821 us |  1.00 |    1 |
|                 |          |           |           |       |      |
| ArrayAsSpanFill | 91.13 us | 0.5738 us | 0.5087 us |  1.00 |    1 |
|                 |          |           |           |       |      |
|     ForLoopFill | 91.18 us | 1.2418 us | 1.1616 us |  1.00 |    1 |
|                 |          |           |           |       |      |
|        FastFill | 90.80 us | 0.8238 us | 0.7303 us |  1.00 |    1 |
|                 |          |           |           |       |      |
| FastFillAligned | 91.46 us | 1.2967 us | 1.2129 us |  1.00 |    1 |

### Block768Managed

|          Method |     Mean |     Error |    StdDev | Ratio | Rank |
|---------------- |---------:|----------:|----------:|------:|-----:|
|       ArrayFill | 6.168 us | 0.0035 us | 0.0031 us |  1.00 |    1 |
|                 |          |           |           |       |      |
| ArrayAsSpanFill | 8.382 us | 0.0231 us | 0.0216 us |  1.00 |    1 |
|                 |          |           |           |       |      |
|     ForLoopFill | 8.212 us | 0.0009 us | 0.0008 us |  1.00 |    1 |
|                 |          |           |           |       |      |
|        FastFill | 8.218 us | 0.0054 us | 0.0050 us |  1.00 |    1 |
|                 |          |           |           |       |      |
| FastFillAligned | 8.226 us | 0.0177 us | 0.0166 us |  1.00 |    1 |

`byte` utilises `Unsafe.InitBlock`, as does the current span implementation, so we keep performance on par there.

`int` and similar types, that are sizes 2, 4, 8, 16, or 32 are where the performance massively increased, here with a ~3.7 times increase over the current span fill, and a ~6.3
times increase over the current array fill for `int`, and over a 30% increase in time for `Block32` (the JIT already generates SSE code for for-loop filling with this, so a less significant speedup is gained
from AVX)

`object`, `Block768`, and `Block768Managed` fall back to the for-loop software implementation, which works as well as expected - the small difference between `ArrayFill` and this
is due to the JIT codegen apparently, which cannot be rectified in managed code where you have a `Span<T>` not a `T[]`.

An earlier implementation tested if the `T` was equal to `default` when unmanaged, which allows use of `Unsafe.InitBlock`, however, testing can be expensive, so I dropped this

If a managed API exposing cache size is introduced, this code could probably benefit from using non-temporal stores where possible when `fullSize > CacheSize`

[Here](https://sharplab.io/#v2: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) is a link to a SharpLab to view the disasm for the code, if needed
