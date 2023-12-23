using System;
using T3.Core.Resource.ShaderInputs;
using Device = SharpDX.Direct3D11.Device;

namespace T3.Core.Resource.Generators.DX11;

public partial class DX11ResourceGenerator : ResourceGenerator
{
    internal new static DX11.DX11ResourceGenerator Instance { get; private set; }

    public DX11ResourceGenerator(Device device)
    {
        if (Instance != null)
            throw new Exception("DX11ResourceGenerator already created");

        Instance = this;
        _device = device;
    }

    public override Buffer<T> CreateBuffer<T>(in T defaultValue, bool indirect)
    {
        return new BufferDx11<T>(_device, defaultValue, indirect);
    }

    public override StructuredBuffer<T> CreateStructuredBuffer<T>(in StructuredBufferDescriptor description, T[] data)
    {
        return new StructuredBufferDx11<T>(_device, data);
    }

    private readonly Device _device;
    internal Device Device => _device;
}