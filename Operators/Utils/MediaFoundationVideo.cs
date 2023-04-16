using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using SharpGLTF.Schema2;
using T3.Core.Logging;
using T3.Core.Resource;
using T3.Core.Utils;
using T3.Operators.Types.Id_3631c727_36a0_4f26_ae76_ee9c100efc33;

namespace Operators.Utils;

public static class MediaFoundationVideo
{
    public static VideoNormalizedRect? ToVideoRect(RectangleF? rect)
    {
        if (rect.HasValue)
        {
            var r = rect.Value;
            return new VideoNormalizedRect()
                       {
                           Left = r.Left.Clamp(0f, 1f),
                           Bottom = r.Bottom.Clamp(0f, 1f),
                           Right = r.Right.Clamp(0f, 1f),
                           Top = r.Top.Clamp(0f, 1f)
                       };
        }

        return default;
    }

    public static RawColorBGRA? ToRawColorBgra(Color4? color)
    {
        if (color.HasValue)
        {
            color.Value.ToBgra(out var r, out var g, out var b, out var a);
            return new RawColorBGRA(b, g, r, a);
        }

        return default;
    }

    public static void UpdateVideo(MediaEngine engine, bool shouldSeek, float seekTime, ref Texture2D texture)
    {
        /** The readiness state of the media. */
        ReadyStates ReadyState = (ReadyStates)engine.ReadyState;
        if (ReadyState <= ReadyStates.HaveNothing)
        {
            _texture = null; // FIXME: this is probably stupid
            return;
        }

        if (ReadyState >= ReadyStates.HaveMetadata)
        {
            if (shouldSeek)
            {
                seekTime = seekTime.Clamp(0, Duration);
                engine.CurrentTime = seekTime;
                Seek = false;
            }

            if (Loop)
            {
                var currentTime = CurrentTime;
                var loopStartTime = LoopStartTime.Clamp(0f, Duration);
                var loopEndTime = (LoopEndTime < 0 ? float.MaxValue : LoopEndTime).Clamp(0f, Duration);
                if (currentTime < loopStartTime || currentTime > loopEndTime)
                {
                    if (PlaybackRate >= 0)
                        _engine.CurrentTime = loopStartTime;
                    else
                        _engine.CurrentTime = loopEndTime;
                }
            }

            if (_play && engine.IsPaused)
                engine.Play();

            else if (!_play && !engine.IsPaused)
                engine.Pause();
        }

        if (ReadyState < ReadyStates.HaveCurrentData || !_engine.OnVideoStreamTick(out var presentationTimeTicks))
            return;

        if (_invalidated || _texture == null)
        {
            _invalidated = false;

            engine.GetNativeVideoSize(out var width, out var height);
            Log.Debug($"should set size to: {width}x{height}");
            SetupTexture(new Size2(width, height));

            // _SRGB doesn't work :/ Getting invalid argument exception in TransferVideoFrame
            //_renderTarget = Texture.New2D(graphicsDevice, width, height, PixelFormat.B8G8R8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
        }

        if (_texture == null)
            return;

        engine.TransferVideoFrame(
                                  _texture,
                                  ToVideoRect(SourceBounds),
                                  //new RawRectangle(0, 0, renderTarget.ViewWidth, renderTarget.ViewHeight),
                                  new RawRectangle(0, 0, _textureSize.Width, _textureSize.Height),
                                  ToRawColorBgra(BorderColor));
        Texture.Value = _texture;
    }

    public static void SetupTexture(Size2 size)
    {
        if (size.Width <= 0 || size.Height <= 0)
            size = new Size2(512, 512);

        Texture.DirtyFlag.Clear();

        if (_texture != null && size == _size)
            return;

        var resourceManager = ResourceManager.Instance();
        var device = ResourceManager.Device;
        _texture = new Texture2D(device,
                                 new Texture2DDescription
                                     {
                                         ArraySize = 1,
                                         BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                                         CpuAccessFlags = CpuAccessFlags.None,
                                         Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                                         Width = size.Width,
                                         Height = size.Height,
                                         MipLevels = 0,
                                         OptionFlags = ResourceOptionFlags.None,
                                         SampleDescription = new SampleDescription(1, 0),
                                         Usage = ResourceUsage.Default
                                     });
        _size = size;
    }

    public static void SetupMediaFoundation(out DXGIDeviceManager dxgiDeviceManager, out MediaEngine mediaEngine, MediaEngineNotifyDelegate enginePlaybackEventHandler)
    {
        using var mediaEngineAttributes = new MediaEngineAttributes
                                              {
                                                  // _SRGB doesn't work :/ Getting invalid argument exception later in TransferVideoFrame
                                                  AudioCategory = SharpDX.Multimedia.AudioStreamCategory.GameMedia,
                                                  AudioEndpointRole = SharpDX.Multimedia.AudioEndpointRole.Multimedia,
                                                  VideoOutputFormat = (int)SharpDX.DXGI.Format.B8G8R8A8_UNorm
                                              };

        var device = ResourceManager.Device;
        if (device != null)
        {
            // Add multi thread protection on device (MF is multi-threaded)
            using var deviceMultithread = device.QueryInterface<DeviceMultithread>();
            deviceMultithread.SetMultithreadProtected(true);

            // Reset device
            using var manager = new DXGIDeviceManager();
            manager.ResetDevice(device);
            mediaEngineAttributes.DxgiManager = manager;
        }

        // Setup Media Engine attributes and create a DXGI Device Manager
        dxgiDeviceManager = new DXGIDeviceManager();
        dxgiDeviceManager.ResetDevice(device);
        var attributes = new MediaEngineAttributes
                             {
                                 DxgiManager = dxgiDeviceManager,
                                 VideoOutputFormat = (int)SharpDX.DXGI.Format.B8G8R8A8_UNorm
                                 //VideoOutputFormat = (int)SharpDX.DXGI.Format.NV12                                     
                             };

        mediaEngine = new MediaEngine(MediaEngineFactory, attributes, MediaEngineCreateFlags.None, enginePlaybackEventHandler);
    }

    static MediaFoundationVideo()
    {
        MediaManager.Startup();
    }

    private static readonly MediaEngineClassFactory MediaEngineFactory =  new MediaEngineClassFactory();

    private enum ReadyStates : short
    {
        /** information is available about the media resource. */
        HaveNothing,

        /** ugh of the media resource has been retrieved that the metadata attributes are initialized. Seeking will no longer raise an exception. */
        HaveMetadata,

        /** a is available for the current playback position, but not enough to actually play more than one frame. */
        HaveCurrentData,

        /** a for the current playback position as well as for at least a little bit of time into the future is available (in other words, at least two frames of video, for example). */
        HaveFutureData,

        /** ugh data is available—and the download rate is high enough—that the media can be played through to the end without interruption.*/
        HaveEnoughData
    }
}