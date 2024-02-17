using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using T3.Core.Logging;
using T3.Core.Operator.Slots;
using T3.Core.Resource;
using T3.Core.Utils;

namespace T3.Core.Operator
{
    public abstract class Instance : IDisposable, IGuidPathContainer
    {
        public abstract Type Type { get; }
        public Guid SymbolChildId { get; set; }
        public Instance Parent { get; internal set; }
        public abstract Symbol Symbol { get; }

        public List<OutputSlot> Outputs { get; set; } = new();
        public List<Instance> Children { get; set; } = new();
        public List<InputSlot> Inputs { get; set; } = new();

        protected internal ResourceFileWatcher ResourceFileWatcher => Symbol.SymbolPackage.ResourceFileWatcher;

        private List<string> _resourceFolders = null;

        public IReadOnlyList<string> ResourceFolders
        {
            get
            {
                if (_resourceFolders != null)
                    return _resourceFolders;

                GatherResourceFolders(this, out _resourceFolders);
                return _resourceFolders;
            }
        }

        /// <summary>
        /// get input without GC allocations 
        /// </summary>
        public InputSlot GetInput(Guid guid)
        {
            //return Inputs.SingleOrDefault(input => input.Id == guid);
            foreach (var i in Inputs)
            {
                if (i.Id == guid)
                    return i;
            }

            return null;
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
        }

        protected void SetupInputAndOutputsFromType()
        {
            var assemblyInfo = Symbol.SymbolPackage.AssemblyInformation;
            foreach (var input in assemblyInfo.InputFields[Type])
            {
                var inputSlot = (InputSlot)input.Field.GetValue(this);
                inputSlot!.Parent = this;
                inputSlot.Id = input.Attribute.Id;
                inputSlot.MappedType = input.Attribute.MappedType;
                Inputs.Add(inputSlot);
            }

            // outputs identified by attribute
            foreach (var output in assemblyInfo.OutputFields[Type])
            {
                var slot = (OutputSlot)output.Field.GetValue(this);
                slot!.Parent = this;
                slot.Id = output.Attribute.Id;
                Outputs.Add(slot);
            }
        }

        public bool TryAddConnection(Symbol.Connection connection, int multiInputIndex)
        {
            var gotSource = TryGetSourceSlot(connection, out var sourceSlot);
            var gotTarget = TryGetTargetSlot(connection, out var targetSlot);

            if (!gotSource || !gotTarget)
                return false;

            if (!targetSlot.TryGetAsMultiInput(out var multiInput))
            {
                targetSlot.AddConnection(sourceSlot);
            }
            else
            {
                multiInput.AddConnection(sourceSlot, multiInputIndex);
            }

            sourceSlot.DirtyFlag.Invalidate();
            return true;
        }

        public void RemoveConnection(Symbol.Connection connection, int index)
        {
            var success = TryGetTargetSlot(connection, out var targetSlot);
            if (!success)
                return;

            if (targetSlot.IsMultiInput)
            {
                _ = targetSlot.TryGetAsMultiInput(out var multiInput);
                multiInput.RemoveConnection(index);
            }
            else
            {
                targetSlot.RemoveConnection();
            }
        }

        private bool TryGetSourceSlot(Symbol.Connection connection, out OutputSlot sourceSlot)
        {
            var compositionInstance = this;

            // Get source Instance
            Instance sourceInstance = null;
            var gotSourceInstance = false;

            foreach (var child in compositionInstance.Children)
            {
                if (child.SymbolChildId != connection.SourceParentOrChildId)
                    continue;

                sourceInstance = child;
                gotSourceInstance = true;
                break;
            }

            // Evaluate correctness of slot source Instance
            var connectionBelongsToThis = connection.SourceParentOrChildId == Guid.Empty;
            if (!gotSourceInstance && !connectionBelongsToThis)
            {
                Log.Error($"Connection has incorrect source slot: {connection.SourceParentOrChildId}");
                sourceSlot = null;
                return false;
            }

            if (gotSourceInstance)
            {
                var outputs = sourceInstance.Outputs;
                foreach (var output in outputs)
                {
                    if (output.Id != connection.SourceSlotId)
                        continue;

                    sourceSlot = output;
                    return true;
                }
            }
            else
            {
                var inputs = compositionInstance.Inputs;
                foreach (var input in inputs)
                {
                    if (input.Id != connection.SourceSlotId)
                        continue;

                    sourceSlot = input.LinkSlot;
                    return true;
                }
            }

            sourceSlot = null;
            return false;
        }

        private bool TryGetTargetSlot(Symbol.Connection connection, out InputSlot targetSlot)
        {
            var compositionInstance = this;

            // Get target Instance

            Instance targetInstance = null;
            bool gotTargetInstance = false;

            foreach (var child in compositionInstance.Children)
            {
                if (child.SymbolChildId != connection.TargetParentOrChildId)
                    continue;

                targetInstance = child;
                gotTargetInstance = true;
                break;
            }

            if (gotTargetInstance)
            {
                foreach (var input in targetInstance.Inputs)
                {
                    if (input.Id != connection.TargetSlotId)
                        continue;

                    targetSlot = input;
                    return true;
                }
            }
            else
            {
                foreach (var output in compositionInstance.Outputs)
                {
                    if (output.Id != connection.TargetSlotId)
                        continue;

                    targetSlot = output.LinkSlot;
                    return true;
                }
            }
            #if DEBUG
            if (!gotTargetInstance)
            {
                Debug.Assert(connection.TargetParentOrChildId == Guid.Empty);
            }
            #endif

            targetSlot = null;
            return false;
        }

        private static void GatherResourceFolders(Instance instance, out List<string> resourceFolders)
        {
            resourceFolders = [instance.ResourceFileWatcher.WatchedFolder];

            while (instance.Parent != null)
            {
                instance = instance.Parent;
                var resourceFolder = instance.ResourceFileWatcher.WatchedFolder;

                if (!resourceFolders.Contains(resourceFolder))
                    resourceFolders.Add(resourceFolder);
            }
        }

        public IList<Guid> InstancePath => OperatorUtils.BuildIdPathForInstance(this).ToArray();
    }

    public class Instance<T> : Instance where T : Instance
    {
        public override Type Type { get; } = typeof(T);
        public override Symbol Symbol => _typeSymbol;

        // ReSharper disable once StaticMemberInGenericType
        #pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private static Symbol _typeSymbol; // this is set with reflection in Symbol.UpdateType()
        #pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        protected Instance()
        {
            SetupInputAndOutputsFromType();
        }
    }
}