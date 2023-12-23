using System;
using System.Runtime.CompilerServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using T3.Core.Logging;
using T3.Core.Resource.ShaderInputs;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace T3.Core.Resource.Generators.DX11;

class StructuredBufferDx11<T> : StructuredBuffer<T> where T : unmanaged
{
    public StructuredBufferDx11(Device device, T[] data)
    {
        _data = data;
        var stride = Stride;
        int count = data.Length;
        var bufferDesc = new BufferDescription
                             {
                                 Usage = ResourceUsage.Default,
                                 BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                                 SizeInBytes = stride * count,
                                 OptionFlags = ResourceOptionFlags.BufferStructured,
                                 StructureByteStride = stride
                             };
        _dataStream = new DataStream(data.Length * stride, true, true);
        _dataStream.WriteRange(data);
        _dataStream.Position = 0;
        _buffer = new Buffer(device, _dataStream, bufferDesc);
    }

    public override object GetShaderView(bool unorderedReadWrite) => GetShaderView(unorderedReadWrite, StructuredBufferFlags.None);

    public override object GetShaderView(bool unorderedReadWrite, StructuredBufferFlags flags)
    {
        if (unorderedReadWrite)
        {
            _uav?.Dispose();
            var uavDesc = new UnorderedAccessViewDescription()
                              {
                                  Dimension = UnorderedAccessViewDimension.Buffer,
                                  Format = Format.Unknown,
                                  Buffer = new UnorderedAccessViewDescription.BufferResource
                                               {
                                                   FirstElement = 0,
                                                   ElementCount = _data.Length,
                                                   Flags = flags.ToDx11()
                                               }
                              };

            try
            {
                _uav = new UnorderedAccessView(DX11ResourceGenerator.Instance.Device, _buffer, uavDesc);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to create UAV for structured buffer: {e.Message}");
            }

            return _uav;
        }

        _srv?.Dispose();
        var srvDesc = new ShaderResourceViewDescription()
                          {
                              Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                              Format = Format.Unknown,
                              BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource
                                             {
                                                 FirstElement = 0,
                                                 ElementCount = _data.Length,
                                             }
                          };
        _srv = new ShaderResourceView(DX11ResourceGenerator.Instance.Device, _buffer, srvDesc);
        return _srv;
    }

    public override void ApplyToBuffer()
    {
        if (IsDisposed)
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

    public override void DisposeObjects()
    {
        _buffer.Dispose();
        _dataStream.Dispose();
    }

    protected override Span<T> GetSpan() => _data.AsSpan();

    private readonly T[] _data;
    private readonly DataStream _dataStream;
    private readonly Buffer _buffer;
    private UnorderedAccessView _uav;
    private ShaderResourceView _srv;
    
    public override object NativeBuffer => _buffer;
}

static class FlagConverter
{
    public static UnorderedAccessViewBufferFlags ToDx11(this StructuredBufferFlags flags)
    {
        var result = UnorderedAccessViewBufferFlags.None;
        
        if (flags.HasFlag(StructuredBufferFlags.Append))
            result |= UnorderedAccessViewBufferFlags.Append;
        
        if (flags.HasFlag(StructuredBufferFlags.Counter))
            result |= UnorderedAccessViewBufferFlags.Counter;
        
        if (flags.HasFlag(StructuredBufferFlags.Raw))
            result |= UnorderedAccessViewBufferFlags.Raw;
        
        return result;
    }
}