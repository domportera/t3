using System;
using T3.Core.Resource.ShaderInputs;
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

    public override IConstantBuffer CreateBuffer(string filePath, int sizeInBytes)
    {
        throw new System.NotImplementedException();
    }

    public override IStructuredBuffer<T> CreateStructuredBuffer<T>(string filePath, in StructuredBufferDescriptor description)
    {
        throw new System.NotImplementedException();
    }

    private readonly Device _device;
    internal Device Device => _device;
}