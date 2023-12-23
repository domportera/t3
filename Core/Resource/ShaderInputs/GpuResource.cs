using System;

namespace T3.Core.Resource.ShaderInputs;

public abstract class GpuResource : IDisposable
{
    public abstract object GetShaderView(bool unorderedReadWrite);
    public abstract void DisposeObjects();

    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        DisposeObjects();
    }

    public string DebugName { get; set; }
    public bool IsDisposed { get; private set; }
}