using System;
using System.Collections.Generic;
using Log = T3.Core.Logging.Log;

// ReSharper disable ConvertToAutoProperty

namespace T3.Core.Operator.Slots
{
    //todo: make this an InputSlot<T[]>?
    public sealed class MultiInputSlot<T>() : MultInputSlot(typeof(T))
    {
        public override bool IsConnected => _outputSlotsConnectedToMe.Count > 0;
        public override IReadOnlyList<OutputSlot> OutputsPluggedInToMe => _outputSlotsConnectedToMe;
        public IReadOnlyList<Slot<T>> OutputSlotsConnectedToMe => _outputSlotsConnectedToMe;

        public override OutputSlot FirstConnection => (_outputSlotsConnectedToMe.Count > 0 ? _outputSlotsConnectedToMe[0] : null)!;
        public override SymbolChild.Input Input { get; set; }

        protected override OutputSlot CreateLinkSlot()
        {
            return new MultiOutputSlot<T>(this);
        }

        public override void AddConnection(OutputSlot sourceSlot)
        {
            var sourceSlotTyped = (Slot<T>)sourceSlot;
            _outputSlotsConnectedToMe.Add(sourceSlotTyped);
        }

        public override void AddConnection(OutputSlot sourceSlot, int index)
        {
            if (sourceSlot is Slot<T> sourceSlotTyped)
            {
                if (index < _outputSlotsConnectedToMe.Count)
                    _outputSlotsConnectedToMe.Insert(index, sourceSlotTyped);
                else
                    _outputSlotsConnectedToMe.Add(sourceSlotTyped);
            }
            else if (sourceSlot is MultiOutputSlot<T> multiOutputSlot)
            {
                var linkSlot = (MultiInputSlot<T>)multiOutputSlot.LinkSlot;
                var slotsToAdd = linkSlot.OutputSlotsConnectedToMe; // todo - replace with field for performance
                if (index < _outputSlotsConnectedToMe.Count)
                    _outputSlotsConnectedToMe.InsertRange(index, slotsToAdd);
                else
                    _outputSlotsConnectedToMe.AddRange(slotsToAdd);

                return;
                var log = $"Adding connection from {multiOutputSlot.LinkSlot.Parent.Symbol.Name} to {this} in {Parent.Symbol.Name}."
                          + " Plugging a MultiOutputSlot into a MultiInputSlot is not yet supported";
                Log.Warning(log);
            }
        }

        public override void RemoveConnection()
        {
            _outputSlotsConnectedToMe.RemoveAt(_outputSlotsConnectedToMe.Count - 1);
        }

        public override void RemoveConnection(int index)
        {
            if (index >= _outputSlotsConnectedToMe.Count)
                return;

            _outputSlotsConnectedToMe.RemoveAt(index);
        }

        public void GetValues(ref T[] resources, EvaluationContext context, bool clearDirty = true)
        {
            var connectedInputs = _outputSlotsConnectedToMe;
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
                    if (_outputSlotsConnectedToMe.Count <= index)
                        continue;

                    var outputSlot = _outputSlotsConnectedToMe[index];
                    totalTarget += outputSlot.Invalidate();
                    outputDirty |= outputSlot.DirtyFlag.IsDirty;
                }
            }
            else
            {
                foreach (var outputSlot in _outputSlotsConnectedToMe)
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

        private readonly List<Slot<T>> _outputSlotsConnectedToMe = new(10);
        public SlotBase FirstConnectedSlot => _outputSlotsConnectedToMe.Count > 0 ? _outputSlotsConnectedToMe[0] : null;

        public IReadOnlyList<T> GetValues(EvaluationContext context, bool clearDirty = true)
        {
            _values.Clear();
            foreach (var input in _outputSlotsConnectedToMe)
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