using System;
using SharpDX.DXGI;

namespace T3.Core.Resource.Generators.DX11;

public partial class DX11ResourceGenerator
{
    private TextureFormat MapTextureFormat(Format descriptionFormat)
    {
        throw new NotImplementedException();
        switch (descriptionFormat)
        {
            case Format.Unknown:
                break;
            case Format.R32G32B32A32_Typeless:
                break;
            case Format.R32G32B32A32_Float:
                break;
            case Format.R32G32B32A32_UInt:
                break;
            case Format.R32G32B32A32_SInt:
                break;
            case Format.R32G32B32_Typeless:
                break;
            case Format.R32G32B32_Float:
                break;
            case Format.R32G32B32_UInt:
                break;
            case Format.R32G32B32_SInt:
                break;
            case Format.R16G16B16A16_Typeless:
                break;
            case Format.R16G16B16A16_Float:
                break;
            case Format.R16G16B16A16_UNorm:
                break;
            case Format.R16G16B16A16_UInt:
                break;
            case Format.R16G16B16A16_SNorm:
                break;
            case Format.R16G16B16A16_SInt:
                break;
            case Format.R32G32_Typeless:
                break;
            case Format.R32G32_Float:
                break;
            case Format.R32G32_UInt:
                break;
            case Format.R32G32_SInt:
                break;
            case Format.R32G8X24_Typeless:
                break;
            case Format.D32_Float_S8X24_UInt:
                break;
            case Format.R32_Float_X8X24_Typeless:
                break;
            case Format.X32_Typeless_G8X24_UInt:
                break;
            case Format.R10G10B10A2_Typeless:
                break;
            case Format.R10G10B10A2_UNorm:
                break;
            case Format.R10G10B10A2_UInt:
                break;
            case Format.R11G11B10_Float:
                break;
            case Format.R8G8B8A8_Typeless:
                break;
            case Format.R8G8B8A8_UNorm:
                break;
            case Format.R8G8B8A8_UNorm_SRgb:
                break;
            case Format.R8G8B8A8_UInt:
                break;
            case Format.R8G8B8A8_SNorm:
                break;
            case Format.R8G8B8A8_SInt:
                break;
            case Format.R16G16_Typeless:
                break;
            case Format.R16G16_Float:
                break;
            case Format.R16G16_UNorm:
                break;
            case Format.R16G16_UInt:
                break;
            case Format.R16G16_SNorm:
                break;
            case Format.R16G16_SInt:
                break;
            case Format.R32_Typeless:
                break;
            case Format.D32_Float:
                break;
            case Format.R32_Float:
                break;
            case Format.R32_UInt:
                break;
            case Format.R32_SInt:
                break;
            case Format.R24G8_Typeless:
                break;
            case Format.D24_UNorm_S8_UInt:
                break;
            case Format.R24_UNorm_X8_Typeless:
                break;
            case Format.X24_Typeless_G8_UInt:
                break;
            case Format.R8G8_Typeless:
                break;
            case Format.R8G8_UNorm:
                break;
            case Format.R8G8_UInt:
                break;
            case Format.R8G8_SNorm:
                break;
            case Format.R8G8_SInt:
                break;
            case Format.R16_Typeless:
                break;
            case Format.R16_Float:
                break;
            case Format.D16_UNorm:
                break;
            case Format.R16_UNorm:
                break;
            case Format.R16_UInt:
                break;
            case Format.R16_SNorm:
                break;
            case Format.R16_SInt:
                break;
            case Format.R8_Typeless:
                break;
            case Format.R8_UNorm:
                break;
            case Format.R8_UInt:
                break;
            case Format.R8_SNorm:
                break;
            case Format.R8_SInt:
                break;
            case Format.A8_UNorm:
                break;
            case Format.R1_UNorm:
                break;
            case Format.R9G9B9E5_Sharedexp:
                break;
            case Format.R8G8_B8G8_UNorm:
                break;
            case Format.G8R8_G8B8_UNorm:
                break;
            case Format.BC1_Typeless:
                break;
            case Format.BC1_UNorm:
                break;
            case Format.BC1_UNorm_SRgb:
                break;
            case Format.BC2_Typeless:
                break;
            case Format.BC2_UNorm:
                break;
            case Format.BC2_UNorm_SRgb:
                break;
            case Format.BC3_Typeless:
                break;
            case Format.BC3_UNorm:
                break;
            case Format.BC3_UNorm_SRgb:
                break;
            case Format.BC4_Typeless:
                break;
            case Format.BC4_UNorm:
                break;
            case Format.BC4_SNorm:
                break;
            case Format.BC5_Typeless:
                break;
            case Format.BC5_UNorm:
                break;
            case Format.BC5_SNorm:
                break;
            case Format.B5G6R5_UNorm:
                break;
            case Format.B5G5R5A1_UNorm:
                break;
            case Format.B8G8R8A8_UNorm:
                break;
            case Format.B8G8R8X8_UNorm:
                break;
            case Format.R10G10B10_Xr_Bias_A2_UNorm:
                break;
            case Format.B8G8R8A8_Typeless:
                break;
            case Format.B8G8R8A8_UNorm_SRgb:
                break;
            case Format.B8G8R8X8_Typeless:
                break;
            case Format.B8G8R8X8_UNorm_SRgb:
                break;
            case Format.BC6H_Typeless:
                break;
            case Format.BC6H_Uf16:
                break;
            case Format.BC6H_Sf16:
                break;
            case Format.BC7_Typeless:
                break;
            case Format.BC7_UNorm:
                break;
            case Format.BC7_UNorm_SRgb:
                break;
            case Format.AYUV:
                break;
            case Format.Y410:
                break;
            case Format.Y416:
                break;
            case Format.NV12:
                break;
            case Format.P010:
                break;
            case Format.P016:
                break;
            case Format.Opaque420:
                break;
            case Format.YUY2:
                break;
            case Format.Y210:
                break;
            case Format.Y216:
                break;
            case Format.NV11:
                break;
            case Format.AI44:
                break;
            case Format.IA44:
                break;
            case Format.P8:
                break;
            case Format.A8P8:
                break;
            case Format.B4G4R4A4_UNorm:
                break;
            case Format.P208:
                break;
            case Format.V208:
                break;
            case Format.V408:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(descriptionFormat), descriptionFormat, null);
        }
    }
}