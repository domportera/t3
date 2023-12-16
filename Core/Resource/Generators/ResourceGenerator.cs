using System;
using SharpDX;
using T3.Core.Resource.ShaderInputs;

namespace T3.Core.Resource.Generators;

public abstract class ResourceGenerator
{
    public abstract ITexture CreateTexture(TextureDescription description);
    public abstract ITexture CreateTexture(string filePath);
    public abstract IConstantBuffer CreateBuffer(string filePath, int sizeInBytes);
    public abstract IStructuredBuffer<T> CreateStructuredBuffer<T>(string filePath, in StructuredBufferDescriptor description) where T : unmanaged;
}

public readonly struct TextureDescription
{
    public readonly string DebugName;
    public readonly int Width;
    public readonly int Height;
    public readonly int Depth;
    public readonly Int3 Size => new(Width, Height, Depth);
    public readonly bool DepthAsArray;
    public readonly TextureFormat Format = TextureFormat.R8G8B8A8_UNorm;
    
    public bool Is3D => Depth > 1 && !DepthAsArray;

    public int Dimensions
    {
        get
        {
            var hasDepth = Depth > 1;
            var hasHeight = Height > 1;
            var hasWidth = Width > 1;
            
            var dimensions = Convert.ToInt32(hasDepth) + Convert.ToInt32(hasHeight) + Convert.ToInt32(hasWidth);
            return Math.Min(dimensions, 1);
        }
    }
    
    public TextureDescription(int width, TextureFormat format = TextureFormat.R8G8B8A8_UNorm, string debugName = null)
    {
        Width = width;
        Height = 1;
        Depth = 1;
        DepthAsArray = true;
        
        DebugName = debugName ?? $"Texture_{width}x1";
    }

    public TextureDescription(int width, int height, TextureFormat format = TextureFormat.R8G8B8A8_UNorm, string debugName = null)
    {
        Width = width;
        Height = height;
        Depth = 1;
        DepthAsArray = true;
        DebugName = debugName ?? $"Texture_{width}x{height}";
    }
    
    public TextureDescription(int width, int height, int depth, bool depthAsArray, TextureFormat format = TextureFormat.R8G8B8A8_UNorm , string debugName = null)
    {
        Width = width;
        Height = height;
        Depth = depth;
        DepthAsArray = depthAsArray;
        DebugName = debugName ?? $"Texture{(depthAsArray ? "2DArray" : "3D")}_{width}x{height}x{depth}";
    }
}

public readonly struct StructuredBufferDescriptor
{
    public readonly int Stride;
    public readonly int Count;
    public readonly StructuredBufferFlags BufferFlags;

    public StructuredBufferDescriptor(int stride, int count, StructuredBufferFlags bufferFlags)
    {
        Stride = stride;
        Count = count;
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