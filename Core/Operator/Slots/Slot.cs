using System;
using System.Runtime.CompilerServices;
using T3.Core.DataTypes;

// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace T3.Core.Operator.Slots
{
    public abstract class SlotBase
    {
        public Guid Id;
        public readonly Type ValueType;
        public readonly DirtyFlag DirtyFlag = new();
        public Instance Parent;

        protected SlotBase(Type type)
        {
            // UpdateAction = Update;
            ValueType = type;
            ValueIsCommand = ValueType == typeof(Command);
        }
        
        public abstract void Update(EvaluationContext context);

        public abstract int Invalidate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool AlreadyInvalidated(out DirtyFlag dirtyFlag)
        {
            dirtyFlag = DirtyFlag;
            return dirtyFlag.IsAlreadyInvalidated || dirtyFlag.HasBeenVisited;
        }

        protected readonly bool ValueIsCommand;


    }
}