using System;
using System.Collections.Generic;

// ReSharper disable ConvertToAutoProperty

namespace T3.Core.Operator.Slots
{
    //todo: make this an InputSlot<T[]>?
    public sealed class MultiInputSlot<T>(Type type) : MultiInputSlot(type)
    {
        public override bool IsConnected => _inputConnectionsTyped.Count > 0;
        public override IReadOnlyList<OutputSlot> InputConnections => _inputConnectionsTyped;

        public void GetValues(ref T[] resources, EvaluationContext context, bool clearDirty = true)
        {
            var connectedInputs = _inputConnectionsTyped;
            if (connectedInputs.Count != resources.Length)
            {
                resources = new T[connectedInputs.Count];
            }

            for (int i = 0; i < connectedInputs.Count; i++)
            {
                resources[i] = connectedInputs[i].GetValue(context);
            }

            if (clearDirty)
                DirtyFlag.Clear();
        }

        public override int Invalidate()
        {
            if (AlreadyInvalidated(out var dirtyFlag))
                return dirtyFlag.Target;

            if (!IsConnected)
            {
                if (dirtyFlag.Trigger != DirtyFlagTrigger.None)
                    dirtyFlag.Invalidate();
                
                return dirtyFlag.Target;
            }

            var totalTarget = 0;
            bool outputDirty = dirtyFlag.IsDirty;

            if (LimitMultiInputInvalidationToIndices.Count > 0)
            {
                foreach (var index in LimitMultiInputInvalidationToIndices)
                {
                    if (_inputConnectionsTyped.Count <= index)
                        continue;

                    var outputSlot = _inputConnectionsTyped[index];
                    totalTarget += outputSlot.Invalidate();
                    outputDirty |= outputSlot.DirtyFlag.IsDirty;
                }
            }
            else
            {
                foreach (var outputSlot in _inputConnectionsTyped)
                {
                    totalTarget += outputSlot.Invalidate();
                    outputDirty |= outputSlot.DirtyFlag.IsDirty;
                }
            }

            if (outputDirty || (dirtyFlag.Trigger & DirtyFlagTrigger.Animated) == DirtyFlagTrigger.Animated)
            {
                dirtyFlag.Invalidate();
            }

            dirtyFlag.SetVisited();
            return dirtyFlag.Target;
        }

        private readonly List<Slot<T>> _inputConnectionsTyped = new(10);
    }
}