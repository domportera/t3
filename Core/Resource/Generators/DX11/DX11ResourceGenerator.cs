using System;
using System.Runtime.CompilerServices;
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
        var description = new BufferDescription()
                              {
                                  Usage = ResourceUsage.Default,
                                  SizeInBytes = Marshal.SizeOf<T>(),
                                  BindFlags = BindFlags.ConstantBuffer
                              };

        return new BufferDx11<T>(_device, defaultValue, description);
    }

    public override StructuredBuffer<T> CreateStructuredBuffer<T>(in StructuredBufferDescriptor description, T[] data)
    {
        var stride = Marshal.SizeOf<T>();
        var bufferDesc = new BufferDescription
                             {
                                 Usage = ResourceUsage.Default,
                                 BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                                 SizeInBytes = stride * description.Count,
                                 OptionFlags = ResourceOptionFlags.BufferStructured,
                                 StructureByteStride = stride
                             };

        return new StructuredBufferDx11<T>(_device, data, bufferDesc);
    }

    private readonly Device _device;
    internal Device Device => _device;
}

class StructuredBufferDx11<T> : StructuredBuffer<T> where T : unmanaged
{
    public StructuredBufferDx11(Device device, T[] data, in BufferDescription description)
    {
        _data = data;
        _dataStream = new DataStream(data.Length * description.StructureByteStride, true, true);
        _dataStream.WriteRange(data);
        _dataStream.Position = 0;
        _buffer = new Buffer(device, _dataStream, description);
    }

    public override object GetShaderView(bool unorderedReadWrite)
    {
        throw new NotImplementedException();
    }

    public override void ApplyToBuffer()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BufferDx11<T>));

        var bytes = Unsafe.As<byte[]>(_data);
        _dataStream.Position = 0;
        _dataStream.Write(bytes, 0, bytes.Length);
        _dataStream.Position = 0;

        // todo - defer this to a later point in time? (e.g. when the buffer is actually used)?
        var device = DX11ResourceGenerator.Instance.Device;
        var context = device.ImmediateContext;
        var dataBox = new DataBox(_dataStream.DataPointer, 0, 0);
        context.UpdateSubresource(dataBox, _buffer);
    }

    public override void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        _buffer.Dispose();
        _dataStream.Dispose();
    }

    protected override Span<T> GetSpan() => _data.AsSpan();

    private readonly T[] _data;
    private readonly DataStream _dataStream;
    private readonly Buffer _buffer;
    private bool _disposed;
}

class BufferDx11<T> : Buffer<T> where T : unmanaged
{
    public BufferDx11(Device device, in T value, in BufferDescription description)
    {
        _dataStream = new DataStream(description.SizeInBytes, true, true);
        _dataStream.Write(value);
        _dataStream.Position = 0;
        _buffer = new Buffer(device, _dataStream, description);
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
        if (_disposed)
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
}