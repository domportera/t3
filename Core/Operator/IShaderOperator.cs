using System;
using System.IO;
using System.Linq;
using T3.Core.Logging;
using T3.Core.Model;
using T3.Core.Operator.Slots;
using T3.Core.Resource;

namespace T3.Core.Operator;

/// <summary>
/// An interface for shader operators (PixelShader, VertexShader, etc.)
/// Shader updating can be complex, so this interface is used to provide the common functionality for all of them
/// </summary>
/// <typeparam name="T">The type of shader (i.e. SharpDX.D3D11.PixelShader)</typeparam>
public interface IShaderOperator<T> where T : class, IDisposable
{
    public Slot<T> Shader { get; }
    public InputSlot<string> Source { get; }
    public InputSlot<string> EntryPoint { get; }
    public InputSlot<string> DebugName { get; }
    public Instance Instance { get; }
    bool SourceIsSourceCode { get; }
    public ShaderResource<T> ShaderResource { get; set; }

    public bool TryUpdateShader(EvaluationContext context, ref string cachedSource, out string message)
    {
        // cache interface values to avoid additional virtual method calls
        bool isSourceCode = SourceIsSourceCode;
        var sourceSlot = Source;
        var entryPointSlot = EntryPoint;
        var debugNameSlot = DebugName;

        var shouldUpdate = !isSourceCode || sourceSlot.DirtyFlag.IsDirty || entryPointSlot.DirtyFlag.IsDirty || debugNameSlot.DirtyFlag.IsDirty;

        if (!shouldUpdate)
        {
            message = string.Empty;
            return false;
        }

        var source = sourceSlot.GetValue(context);
        
        // prevent rapid recompilation attempts when a user is typing a value
        if (isSourceCode && !source.AsSpan().EndsWith(".hlsl"))
        {
            message = "Source code must be HLSL and file must end with \".hlsl\" file extension.";
            return false;
        }
        
        var entryPoint = entryPointSlot.GetValue(context);
        var debugName = debugNameSlot.GetValue(context);
        var instance = Instance;

        var type = GetType();

        if (!TryGetDebugName(out message, ref debugName))
        {
            LogUpdateFailure(instance, debugName, message);
            return false;
        }

        //Log.Debug($"Attempting to update shader \"{debugName}\" ({GetType().Name}) with entry point \"{entryPoint}\".");

        // Cache ShaderResource to avoid additional virtual method calls
        var shaderResource = ShaderResource;
        var needsNewResource = shaderResource == null;

        if (!isSourceCode)
        {
            needsNewResource = needsNewResource || cachedSource != source;
        }

        bool updated;

        if (needsNewResource)
        {
            updated = TryCreateResource(source, entryPoint, debugName, isSourceCode, Shader, instance, out message, out shaderResource);
            if (updated)
                ShaderResource = shaderResource;
        }
        else
        {
            updated = isSourceCode
                          ? shaderResource.TryUpdateFromSource(source, entryPoint, instance.AvailableResourcePackages, out message)
                          : shaderResource.TryUpdateFromFile(source, entryPoint, instance.AvailableResourcePackages, out message);
        }

        if (updated && shaderResource != null)
        {
            shaderResource.UpdateDebugName(debugName);
            Shader.Value = shaderResource.Shader;
            Shader.DirtyFlag.Invalidate();
        }
        else
        {
            LogUpdateFailure(instance, debugName, message);
        }

        cachedSource = source;
        return updated;

        bool TryGetDebugName(out string dbgMessage, ref string dbgName)
        {
            dbgMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(dbgName))
                return true;

            if (isSourceCode)
            {
                dbgName = $"{type.Name}({entryPoint}) - {sourceSlot.Id}";
                return true;
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                dbgMessage = "Source path is empty.";
                return false;
            }

            try
            {
                dbgName = Path.GetFileNameWithoutExtension(source) + " - " + entryPoint;
                return true;
            }
            catch (Exception e)
            {
                dbgMessage = $"Invalid source path for shader: {source}: {e.Message}";
                return false;
            }
        }

        static bool TryCreateResource(string source, string entryPoint, string debugName, bool isSourceCode, ISlot shaderSlot, Instance instance,
                                      out string errorMessage, out ShaderResource<T> shaderResource)
        {
            bool updated;
            var resourceManager = ResourceManager.Instance();

            if (isSourceCode)
            {
                updated = resourceManager.TryCreateShaderResourceFromSource(out shaderResource,
                                                                            shaderSource: source,
                                                                            instance: instance,
                                                                            entryPoint: entryPoint,
                                                                            name: debugName,
                                                                            reason: out errorMessage);
            }
            else
            {
                updated = resourceManager.TryCreateShaderResource(out shaderResource,
                                                                  instance: instance,
                                                                  relativePath: source,
                                                                  entryPoint: entryPoint,
                                                                  name: debugName,
                                                                  fileChangedAction: () =>
                                                                                     {
                                                                                         //sourceSlot.DirtyFlag.Invalidate();
                                                                                         shaderSlot.DirtyFlag.Invalidate();
                                                                                         //Log.Debug($"Invalidated {sourceSlot}   isDirty: {sourceSlot.DirtyFlag.IsDirty}", sourceSlot.Parent);
                                                                                     },
                                                                  reason: out errorMessage);
            }

            string instanceName = instance.SymbolChild?.ReadableName ?? instance.Symbol.Name;
            if (!updated)
            {
                Log.Error($"[{instanceName}] Failed to create shader resource for shader \"{debugName}\" in package \"{instance.Symbol.SymbolPackage.AssemblyInformation.Name}\":" +
                          $"\n{errorMessage}");
            }
            else
            {
                Log.Debug($"[{instanceName}] Created shader resource for shader \"{debugName}\".");
            }

            return updated;
        }

        static void LogUpdateFailure(Instance instance, string debugName, string message)
        {
            Log.Error($"Failed to update shader \"{debugName}\" in package \"{instance.Symbol.SymbolPackage.AssemblyInformation.Name}\":\n{message}");
        }
    }
}