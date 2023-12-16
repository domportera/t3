using System;
using SharpDX.Direct3D11;
using T3.Core.Resource.ShaderInputs;

namespace T3.Core.Resource.Generators.DX11;

class TextureWrapperDX11 : Texture, IDisposable
{
    public TextureDescription Description { get; }
    
    public TextureWrapperDX11(SharpDX.Direct3D11.Texture2D texture, TextureDescription description, ShaderResourceView srv = null)
    {
        _texture = texture;
        Description = description;
        _srv = srv;
    }
    
    public TextureWrapperDX11(SharpDX.Direct3D11.Texture3D texture, TextureDescription description)
    {
        _texture = texture;
        Description = description;
    }
    
    public override void Dispose()
    {
        if (_disposed)
            return;
        
        _disposed = true;
        _texture.Dispose();
    }

    public override object GetShaderView(bool unorderedReadWrite)
    {
        // todo: UAV options, SRV options
         if(unorderedReadWrite)
             return _uav ??= new UnorderedAccessView(DX11.DX11ResourceGenerator.Instance.Device, _texture);
         
         return _srv ??= new ShaderResourceView(DX11.DX11ResourceGenerator.Instance.Device, _texture);
    }

    private readonly SharpDX.Direct3D11.Resource _texture;
    private UnorderedAccessView _uav;
    private ShaderResourceView _srv;
    bool _disposed;
}