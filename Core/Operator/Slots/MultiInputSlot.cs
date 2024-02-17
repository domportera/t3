using System;
using System.Collections.Generic;

// ReSharper disable ConvertToAutoProperty

namespace T3.Core.Operator.Slots
{
    //todo: make this an InputSlot<T[]>?
    public sealed class MultiInputSlot<T>() : MultInputSlot(typeof(T))
    {
        public override bool IsConnected => _inputConnectionsTyped.Count > 0;
        public override IReadOnlyList<OutputSlot> OutputsPluggedInToMe => _inputConnectionsTyped;
        public IReadOnlyList<Slot<T>> InputConnectionsTyped => _inputConnectionsTyped;

        public override OutputSlot FirstConnection => (_inputConnectionsTyped.Count > 0 ? _inputConnectionsTyped[0] : null)!;
        public override SymbolChild.Input Input { get; set; }

        protected override OutputSlot CreateLinkSlot()
        {
            return new MultiOutputSlot<T>(this);
        }

        public override void AddConnection(OutputSlot sourceSlot)
        {
            var sourceSlotTyped = (Slot<T>)sourceSlot;
            _inputConnectionsTyped.Add(sourceSlotTyped);
        }

        public override void AddConnection(OutputSlot sourceSlot, int index)
        {
            if (sourceSlot is Slot<T> sourceSlotTyped)
            {
                if (index < _inputConnectionsTyped.Count)
                    _inputConnectionsTyped.Insert(index, sourceSlotTyped);
                else
                    _inputConnectionsTyped.Add(sourceSlotTyped);
            }
            else if (sourceSlot is MultiOutputSlot<T> multiOutputSlot)
            {
                throw new Exception("Plugging a MultiOutputSlot into a MultiInputSlot is not supported");
            }
        }

        public override void RemoveConnection()
        {
            _inputConnectionsTyped.RemoveAt(_inputConnectionsTyped.Count - 1);
        }

        public override void RemoveConnection(int index)
        {
            if (index >= _inputConnectionsTyped.Count)
                return;

            _inputConnectionsTyped.RemoveAt(index);
        }

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
        public override SlotBase FirstConnectedSlot => _inputConnectionsTyped.Count > 0 ? _inputConnectionsTyped[0] : null;

        public IReadOnlyList<T> GetValues(EvaluationContext context, bool clearDirty = true)
        {
            _values.Clear();
            foreach (var input in _inputConnectionsTyped)
            {
                _values.Add(input.GetValue(context));
            }
            
            if (clearDirty)
                DirtyFlag.Clear();
            
            return _values;
        }

        private readonly List<T> _values = new();
    }
}