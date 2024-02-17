using System;
using System.Collections.Generic;

namespace T3.Core.Operator.Slots
{
    public abstract class MultInputSlot(Type type) : InputSlot(type, true)
    {
        public abstract IReadOnlyList<OutputSlot> OutputsPluggedInToMe { get; }
        public readonly List<int> LimitMultiInputInvalidationToIndices = [];

        public abstract void AddConnection(OutputSlot sourceSlot, int index);
        public abstract void RemoveConnection(int index);
    }
}