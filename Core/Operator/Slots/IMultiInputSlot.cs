using System;
using System.Collections.Generic;

namespace T3.Core.Operator.Slots
{
    public abstract class MultiInputSlot(Type type) : InputSlot(type, true)
    {
        public abstract IReadOnlyList<OutputSlot> InputConnections { get; }
        public readonly List<int> LimitMultiInputInvalidationToIndices = [];
    }
}