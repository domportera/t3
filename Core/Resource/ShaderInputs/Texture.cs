using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using T3.Core.Resource.Generators;

namespace T3.Core.Resource.ShaderInputs;

public abstract class Texture : GpuResource
{
    public TextureDescription Description { get; }
}

public abstract class Buffer<T> : GpuResource where T : unmanaged
{
    public int SizeInBytes => Size;
    public ReadOnlySpan<byte> Bytes => _dataByteArray;

    public T Data
    {
        get => Unsafe.As<byte, T>(ref _dataByteArray[0]);
        set
        {
            Unsafe.As<byte, T>(ref _dataByteArray[0]) = value;
            SetDataInternal(_dataByteArray, in value);
        }
    }
    
    protected abstract void SetDataInternal(ReadOnlySpan<byte> data, in T value);

    private readonly byte[] _dataByteArray = new byte[Size];
    private static readonly int Size = Marshal.SizeOf<T>();
}

public abstract class IStructuredBuffer<T> : GpuResource where T : unmanaged
{
    // index operator
    public T this[int index]
    {
        get => GetSpan()[index];
        set => GetSpan()[index] = value;
    }
    
    public abstract Span<T> GetSpan();
    public int Count { get; }
    public int Stride => Marshal.SizeOf<T>();
    
    public void SetData(params T[] data) => SetData((ReadOnlySpan<T>)data);

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

    public Type DataType => typeof(T);
}

public abstract class GpuResource : IDisposable
{
    public abstract object GetShaderView(bool unorderedReadWrite);
    public abstract void Dispose();
}