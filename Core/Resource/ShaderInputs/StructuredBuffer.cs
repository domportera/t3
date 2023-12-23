using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using T3.Core.Resource.Generators;

namespace T3.Core.Resource.ShaderInputs;

public abstract class StructuredBuffer<T> : RawBuffer where T : unmanaged
{
    // index operator
    public T this[int index]
    {
        get => GetSpan()[index];
        set => GetSpan()[index] = value;
    }

    protected abstract Span<T> GetSpan();
    public abstract void ApplyToBuffer();
    public int Count => GetSpan().Length;
    public int Length => GetSpan().Length;
    static readonly int StrideInternal = Marshal.SizeOf<T>();
    public int Stride => StrideInternal;
    
    public void SetData(params T[] data) => SetData((ReadOnlySpan<T>)data);
    public abstract object GetShaderView(bool unorderedReadWrite, StructuredBufferFlags bufferFlags);

    public void SetData(ReadOnlySpan<T> data)
    {
        var span = GetSpan();
        if (span.Length != data.Length)
            throw new ArgumentException($"Length of data ({data.Length}) does not match length of buffer ({span.Length})");
        
        for(int i = 0; i < data.Length; i++)
            span[i] = data[i];
    }

    public void SetData(IReadOnlyList<T> data)
    {
        var span = GetSpan();
        if (span.Length != data.Count)
            throw new ArgumentException($"Length of data ({data.Count}) does not match length of buffer ({span.Length})");
        
        for(int i = 0; i < data.Count; i++)
            span[i] = data[i];
    }

    public static Type DataType => typeof(T);
}