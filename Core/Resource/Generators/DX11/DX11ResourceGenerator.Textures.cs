using System;
using System.IO;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using T3.Core.Logging;
using T3.Core.Resource.Dds;
using T3.Core.Resource.ShaderInputs;
using Device = SharpDX.Direct3D11.Device;

namespace T3.Core.Resource.Generators.DX11;

public partial class DX11ResourceGenerator
{
    public override ITexture CreateTexture(TextureDescription description)
    {
        TextureWrapperDX11 textureWrapperDx11;
        if (description is { Dimensions: 3, DepthAsArray: false })
        {
            var texture3DDesc = DefaultTexture3DDescription;
            texture3DDesc.Width = description.Width;
            texture3DDesc.Height = description.Height;
            texture3DDesc.Depth = description.Depth;
            var texture = new Texture3D(_device, texture3DDesc);
            textureWrapperDx11 = new TextureWrapperDX11(texture, description);
        }
        else
        {
            var tex2dDesc = DefaultTextureDescription;
            tex2dDesc.Width = description.Width;
            tex2dDesc.Height = description.Height;
            tex2dDesc.ArraySize = description.Depth;

            var texture = new Texture2D(_device, tex2dDesc);
            textureWrapperDx11 = new TextureWrapperDX11(texture, description);
        }

        return textureWrapperDx11;
    }

    public override ITexture CreateTexture(string filePath)
    {
        SharpDX.Direct3D11.Texture2D texture = null;
        ShaderResourceView srv = null;
        if (filePath.ToLower().EndsWith(".dds"))
        {
            var ddsFile = JeremyAnsel.Media.Dds.DdsFile.FromFile(filePath);
            DdsDirectX.CreateTexture(ddsFile, _device, _device.ImmediateContext, out var resource, out srv);
            texture = resource as SharpDX.Direct3D11.Texture2D;
        }
        else
        {
            try
            {
                ImagingFactory factory = new ImagingFactory();
                var bitmapDecoder = new BitmapDecoder(factory, filePath, DecodeOptions.CacheOnDemand);
                var formatConverter = new FormatConverter(factory);
                var bitmapFrameDecode = bitmapDecoder.GetFrame(0);
                formatConverter.Initialize(bitmapFrameDecode, PixelFormat.Format32bppRGBA, BitmapDitherType.None, null, 0.0, BitmapPaletteType.Custom);

                texture = CreateTexture2DFromBitmap(_device, formatConverter);
                string name = Path.GetFileName(filePath);
                texture.DebugName = name;
                bitmapFrameDecode.Dispose();
                bitmapDecoder.Dispose();
                formatConverter.Dispose();
                factory.Dispose();
                Log.Info($"Created texture '{name}' from '{filePath}'");
            }
            catch (Exception e)
            {
                Log.Info($"Info: couldn't access file '{filePath}': {e.Message}.");
            }
        }

        if (texture == null)
        {
            Log.Error($"Failed to create texture from file '{filePath}'.");
            return null;
        }

        string textureName = Path.GetFileName(filePath);
        var description = new TextureDescription(texture.Description.Width, texture.Description.Height, MapTextureFormat(texture.Description.Format),
                                                 textureName);
        var textureWrapper = new TextureWrapperDX11(texture, description, srv);
        return textureWrapper;
    }

    private static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmap(Device device, BitmapSource bitmapSource)
    {
        // Allocate DataStream to receive the WIC image pixels
        var stride = bitmapSource.Size.Width * 4;
        using var buffer = new SharpDX.DataStream(bitmapSource.Size.Height * stride, true, true);

        // Copy the content of the WIC to the buffer
        bitmapSource.CopyPixels(stride, buffer);
        int mipLevels = (int)Math.Log(bitmapSource.Size.Width, 2.0) + 1;
        var texDesc = new Texture2DDescription()
                          {
                              Width = bitmapSource.Size.Width,
                              Height = bitmapSource.Size.Height,
                              ArraySize = 1,
                              BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                              Usage = ResourceUsage.Default,
                              CpuAccessFlags = CpuAccessFlags.None,
                              Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                              MipLevels = mipLevels,
                              OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                              SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                          };
        var dataRectangles = new DataRectangle[mipLevels];
        for (int i = 0; i < mipLevels; i++)
        {
            dataRectangles[i] = new DataRectangle(buffer.DataPointer, stride);
            stride /= 2;
        }

        return new SharpDX.Direct3D11.Texture2D(device, texDesc, dataRectangles);
    }

    private static readonly Texture2DDescription DefaultTextureDescription = new()
                                                                                 {
                                                                                     Width = 1024,
                                                                                     Height = 1024,
                                                                                     ArraySize = 1,
                                                                                     BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                                                                                     Usage = ResourceUsage.Default,
                                                                                     CpuAccessFlags = CpuAccessFlags.None,
                                                                                     Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                                                                                     MipLevels = 1,
                                                                                     OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                                                                                     SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                                                                                 };

    private static readonly Texture3DDescription DefaultTexture3DDescription = new()
                                                                                   {
                                                                                       Width = 1024,
                                                                                       Height = 1024,
                                                                                       Depth = 1024,
                                                                                       BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                                                                                       Usage = ResourceUsage.Default,
                                                                                       CpuAccessFlags = CpuAccessFlags.None,
                                                                                       Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                                                                                       MipLevels = 1,
                                                                                       OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                                                                                   };
}