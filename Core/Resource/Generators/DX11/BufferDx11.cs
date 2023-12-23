using System;
using SharpDX;
using SharpDX.Direct3D11;
using T3.Core.Resource.ShaderInputs;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace T3.Core.Resource.Generators.DX11;

class BufferDx11<T> : Buffer<T> where T : unmanaged
{
    public BufferDx11(Device device, in T value, bool indirect)
    {
        BufferDescription description;
        if (!indirect)
            description = new BufferDescription()
                              {
                                  Usage = ResourceUsage.Default,
                                  SizeInBytes = SizeInBytes,
                                  BindFlags = BindFlags.ConstantBuffer
                              };
        else
        {
            description = new BufferDescription
                              {
                                  Usage = ResourceUsage.Default,
                                  BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                                  SizeInBytes = SizeInBytes,
                                  OptionFlags = ResourceOptionFlags.DrawIndirectArguments,
                                  StructureByteStride = SizeInBytes
                              };
        }

        _dataStream = new DataStream(description.SizeInBytes, true, true);
        _dataStream.Write(value);
        _dataStream.Position = 0;
        _buffer = new Buffer(device, _dataStream, description);
    }

    public override object GetShaderView(bool unorderedReadWrite)
    {
        throw new NotImplementedException();
    }

    public override void DisposeObjects()
    {
        _buffer.Dispose();
        _dataStream.Dispose();
    }

    private readonly Buffer _buffer;
    private readonly DataStream _dataStream;

    protected override void SetDataInternal(ReadOnlySpan<byte> _, in T value)
    {
        if (IsDisposed)
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

    public override object NativeBuffer => _buffer;
}