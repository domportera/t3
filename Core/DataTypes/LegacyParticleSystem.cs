using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using T3.Core.Logging;
using T3.Core.Resource;
using T3.Core.Resource.Generators;
using T3.Core.Resource.ShaderInputs;

namespace T3.Core.DataTypes;

public class LegacyParticleSystem
{
    public StructuredBuffer<Particle> ParticleBuffer;
    public UnorderedAccessView ParticleBufferUav;
    public ShaderResourceView ParticleBufferSrv;

    public StructuredBuffer<ParticleIndex> DeadParticleIndices;
    public UnorderedAccessView DeadParticleIndicesUav;

    public StructuredBuffer<ParticleIndex> AliveParticleIndices;
    public UnorderedAccessView AliveParticleIndicesUav;
    public ShaderResourceView AliveParticleIndicesSrv;

    public Buffer<UInt4> IndirectArgsBuffer;
    public UnorderedAccessView IndirectArgsBufferUav;

    public Buffer<Vector4> ParticleCountConstBuffer;

    public int MaxCount { get; set; } = 20480;

    public void Init()
    {
        if (_initDeadListShaderResource == null)
        {
            string sourcePath = @"lib\particles\particle-dead-list-init.hlsl";
            string entryPoint = "main";
            string debugName = "particle-dead-list-init";
            var resourceManager = ResourceManager.Instance();
            resourceManager.TryCreateShaderResource(resource: out _initDeadListShaderResource,
                                                    fileName: sourcePath,
                                                    errorMessage: out var errorMessage,
                                                    name: entryPoint,
                                                    entryPoint: debugName,
                                                    fileChangedAction: null);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                Log.Error($"{nameof(LegacyParticleSystem)}: {errorMessage}");
        }

        InitParticleBufferAndViews();
        InitDeadParticleIndices();
        InitAliveParticleIndices();
        InitIndirectArgBuffer();
        InitParticleCountConstBuffer();
    }

    private void InitParticleBufferAndViews()
    {
        ResourceManager.Instance();
        _particleData = Enumerable.Repeat(DefaultParticle, MaxCount).ToArray(); // init with negative lifetime other values doesn't matter
        ResourceManager.SetupStructuredBuffer(_particleData, ref ParticleBuffer);
        ParticleBufferUav = (UnorderedAccessView)ParticleBuffer.GetShaderView(true, StructuredBufferFlags.None);
        ParticleBufferSrv = (ShaderResourceView)ParticleBuffer.GetShaderView(false, StructuredBufferFlags.None);
    }

    private const int ParticleIndexSizeInBytes = 8;

    private void InitDeadParticleIndices()
    {
        // init the buffer 
        _deadParticleIndices = Enumerable.Repeat(DefaultParticleIndex, MaxCount).ToArray();
        ResourceManager.SetupStructuredBuffer(_deadParticleIndices, ref DeadParticleIndices);
        DeadParticleIndicesUav = (UnorderedAccessView)DeadParticleIndices.GetShaderView(true, StructuredBufferFlags.Append);

        // init counter of the dead list buffer (must be done due to uav binding)
        ComputeShader deadListInitShader = _initDeadListShaderResource.Shader;
        var device = ResourceManager.Device;
        var deviceContext = device.ImmediateContext;
        var csStage = deviceContext.ComputeShader;
        var prevShader = csStage.Get();
        var prevUavs = csStage.GetUnorderedAccessViews(0, 1);

        // set and call the init shader
        _initDeadListShaderResource.TryGetThreadGroups(out var threadGroups);

        csStage.Set(deadListInitShader);
        csStage.SetUnorderedAccessView(0, DeadParticleIndicesUav, 0);
        int dispatchCount = MaxCount / (threadGroups.X > 0 ? threadGroups.X : 1);
        Log.Info($"particle system: maxcount {MaxCount}  dispatchCount: {dispatchCount} *64: {dispatchCount * 64}");
        deviceContext.Dispatch(dispatchCount, 1, 1);

        // restore prev setup
        csStage.SetUnorderedAccessView(0, prevUavs[0]);
        csStage.Set(prevShader);
    }

    private void InitAliveParticleIndices()
    {
        _aliveParticleIndices = Enumerable.Repeat(DefaultParticleIndex, MaxCount).ToArray();
        ResourceManager.SetupStructuredBuffer(_aliveParticleIndices,  ref AliveParticleIndices);
        AliveParticleIndicesUav = (UnorderedAccessView)AliveParticleIndices.GetShaderView(true, StructuredBufferFlags.Counter);
        AliveParticleIndicesSrv = (ShaderResourceView)AliveParticleIndices.GetShaderView(false, StructuredBufferFlags.None);
    }

    private void InitIndirectArgBuffer()
    {
        ResourceManager.SetupIndirectBuffer(default, ref IndirectArgsBuffer);
        IndirectArgsBufferUav = (UnorderedAccessView)IndirectArgsBuffer.GetShaderView(true);
    }

    private void InitParticleCountConstBuffer()
    {
        ResourceManager.SetupConstBuffer(Vector4.Zero, ref ParticleCountConstBuffer);
        ParticleCountConstBuffer.DebugName = "ParticleCountConstBuffer";
    }

    private ShaderResource<ComputeShader> _initDeadListShaderResource;

    private Particle[] _particleData;
    private ParticleIndex[] _aliveParticleIndices;
    private ParticleIndex[] _deadParticleIndices;
    static readonly Particle DefaultParticle = new()
                                                   {
                                                       Position = Vector3.Zero,
                                                       Lifetime = -10,
                                                       Velocity = Vector3.Zero,
                                                       Mass = 0,
                                                       Color = Vector4.Zero,
                                                       EmitterId = 0,
                                                       Normal = Vector3.Zero,
                                                       EmitTime = 0,
                                                       Size = 0,
                                                       Padding = Vector2.Zero,
                                                   };
    
    static readonly ParticleIndex DefaultParticleIndex = new() { Index = -1, SquaredDistToCamera = float.MaxValue };

    [StructLayout(LayoutKind.Sequential)]
    public struct UInt4
    {
        public uint X;
        public uint Y;
        public uint Z;
        public uint W;
    }
    
    // see Resources/lib/shared/particle.hlsl
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle
    {
        public Vector3 Position;
        public float Lifetime;
        public Vector3 Velocity;
        public float Mass;
        public Vector4 Color;
        public int EmitterId;
        public Vector3 Normal;
        public float EmitTime;
        public float Size;
        public Vector2 Padding;

        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Particle));
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleIndex
    {
        public int Index;
        public float SquaredDistToCamera;

        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(ParticleIndex));
    }
}