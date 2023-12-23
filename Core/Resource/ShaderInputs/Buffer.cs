using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace T3.Core.Resource.ShaderInputs;

public abstract class RawBuffer : GpuResource
{
    public abstract object NativeBuffer { get; }
}

public abstract class Buffer<T> : RawBuffer where T : unmanaged
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