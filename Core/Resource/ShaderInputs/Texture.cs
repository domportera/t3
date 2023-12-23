using T3.Core.Resource.Generators;

namespace T3.Core.Resource.ShaderInputs;

public abstract class Texture : GpuResource
{
    public TextureDescription Description { get; }
}