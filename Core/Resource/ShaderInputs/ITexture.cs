using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using T3.Core.Resource.Generators;

namespace T3.Core.Resource.ShaderInputs;

public interface ITexture : IGpuResource
{
    public TextureDescription Description { get; }
}

public interface IConstantBuffer : IGpuResource
{
    public int SizeInBytes { get; }
    public Span<byte> GetSpan();

    public byte this[int index]
    {
        get => GetSpan()[index];
        set => GetSpan()[index] = value;
    }
    
    public unsafe void SetData<T>(in T data) where T : unmanaged
    {
        var size = Marshal.SizeOf<T>();
        if (size != SizeInBytes)
            throw new ArgumentException($"Size of data ({Unsafe.SizeOf<T>()}) does not match size of buffer ({SizeInBytes})");

        var bufferSpan = GetSpan();
        fixed (T* ptr = &data)
        {
            var dataPtr = (byte*)ptr;
            
            // copy bytes from data to this buffer
            for (var i = 0; i < size; i++)
                bufferSpan[i] = dataPtr[i];
        }
    }
}

public interface IStructuredBuffer<T> : IGpuResource where T : unmanaged
{
    // index operator
    public T this[int index]
    {
        get => GetSpan()[index];
        set => GetSpan()[index] = value;
    }
    
    public Span<T> GetSpan();
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

public interface IGpuResource : IDisposable
{
    public object GetShaderView(bool unorderedReadWrite);
}