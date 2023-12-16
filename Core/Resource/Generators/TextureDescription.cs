using System;
using SharpDX;

namespace T3.Core.Resource.Generators;

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