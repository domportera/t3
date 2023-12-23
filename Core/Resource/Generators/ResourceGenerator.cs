using System;
using T3.Core.Resource.ShaderInputs;

namespace T3.Core.Resource.Generators;

public abstract class ResourceGenerator
{
    private static ResourceGenerator _instance;
    public static ResourceGenerator Instance
    {
        get => _instance;
        set
        {
            if(_instance != null)
                throw new Exception("ResourceGenerator already created");
            
            _instance = value;
        }
    }

    public abstract Texture CreateTexture(TextureDescription description);
    public abstract Texture CreateTexture(string filePath);
    public abstract Buffer<T> CreateBuffer<T>(in T defaultValue, bool indirect) where T : unmanaged;
    public abstract StructuredBuffer<T> CreateStructuredBuffer<T>(in StructuredBufferDescriptor description, T[] data) where T : unmanaged;
}

public readonly struct StructuredBufferDescriptor
{
    public readonly StructuredBufferFlags BufferFlags;

    public StructuredBufferDescriptor(StructuredBufferFlags bufferFlags)
    {
        BufferFlags = bufferFlags;
    }
}

public enum TextureFormat
{
    R4G4B4_UNorm,
    R4G4B4A4_UNorm,
    
    R8_UNorm,
    R8G8_UNorm,
    R8G8B8_UNorm,
    R8G8B8A8_UNorm,
    
    R4G4B4_UNorm_SRGB,
    R4G4B4A4_UNorm_SRGB,
    
    R8_UNorm_SRGB,
    R8G8_UNorm_SRGB,
    R8G8B8_UNorm_SRGB,
    R8G8B8A8_UNorm_SRGB,
    
    B8G8R8A8_UNorm_SRGB,
}

[Flags]
public enum StructuredBufferFlags : byte
{
    None = 0,
    Raw = 1,
    Append = 2,
    Counter = 4,
}

[Flags]
public enum BufferFlags : byte
{
    None = 0,
    AllowWrite = 1,
}