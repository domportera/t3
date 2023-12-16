using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using T3.Core.Resource.ShaderInputs;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace T3.Core.Resource.Generators.DX11;

public partial class DX11ResourceGenerator : ResourceGenerator
{
    internal static DX11.DX11ResourceGenerator Instance { get; private set; }

    public DX11ResourceGenerator(Device device)
    {
        if (Instance != null)
            throw new Exception("DX11ResourceGenerator already created");

        Instance = this;
        _device = device;
    }

    public override Buffer<T> CreateBuffer<T>(in T defaultValue, BufferFlags flags)
    {
        return new BufferDx11<T>(_device, defaultValue, flags);
    }

    public override IStructuredBuffer<T> CreateStructuredBuffer<T>(in StructuredBufferDescriptor description, T[] data)
    {
        throw new System.NotImplementedException();
    }

    private readonly Device _device;
    internal Device Device => _device;
}

class BufferDx11<T> : Buffer<T> where T : unmanaged
{
    public BufferDx11(Device device, in T value, BufferFlags flags)
    {
        _dataStream = new DataStream(Size, true, true);
        _dataStream.Write(value);
        _dataStream.Position = 0;
        _buffer = new Buffer(device, _dataStream, DefaultBufferDescription);
    }

    public override object GetShaderView(bool unorderedReadWrite)
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        _buffer.Dispose();
        _dataStream.Dispose();
    }

    private readonly Buffer _buffer;
    private readonly DataStream _dataStream;

    protected override void SetDataInternal(ReadOnlySpan<byte> _, in T value)
    {
        if(_disposed)
            throw new ObjectDisposedException(nameof(BufferDx11<T>));
        
        _dataStream.Position = 0;
        _dataStream.Write(value);
        _dataStream.Position = 0;
        
        // todo - defer this to a later point in time? (e.g. when the buffer is actually used)?
        var device = DX11ResourceGenerator.Instance.Device;
        var context = device.ImmediateContext;
        var dataBox = new DataBox(_dataStream.DataPointer, 0, 0);
        context.UpdateSubresource(dataBox, _buffer);
    }

    private bool _disposed = false;

    private static readonly int Size = Marshal.SizeOf<T>();
    private static readonly BufferDescription DefaultBufferDescription = new()
                                                                             {
                                                                                 Usage = ResourceUsage.Default,
                                                                                 SizeInBytes = Size,
                                                                                 BindFlags = BindFlags.ConstantBuffer
                                                                             };
}