using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using T3.Core.DataTypes;
using T3.Core.Logging;
using T3.Core.Operator.Interfaces;
using T3.Core.Stats;

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
            _valueIsCommand = ValueType == typeof(Command);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(EvaluationContext context)
        {
            if (DirtyFlag.IsDirty || _valueIsCommand)
            {
                OpUpdateCounter.CountUp();
                UpdateAction?.Invoke(context);
                DirtyFlag.Clear();
                DirtyFlag.SetUpdated();
            }
        }

        public abstract int Invalidate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool AlreadyInvalidated(out DirtyFlag dirtyFlag)
        {
            dirtyFlag = DirtyFlag;
            return dirtyFlag.IsAlreadyInvalidated || dirtyFlag.HasBeenVisited;
        }

        private readonly bool _valueIsCommand;

        public void RestoreUpdateAction()
        {
            // This will happen when operators are recompiled and output slots are disconnected
            if (KeepOriginalUpdateAction == null)
            {
                UpdateAction = null;
                return;
            }

            UpdateAction = KeepOriginalUpdateAction;
            KeepOriginalUpdateAction = null;
            DirtyFlag.Trigger = KeepDirtyFlagTrigger;
            DirtyFlag.Invalidate();
        }

        public void OverrideWithAnimationAction(Action<EvaluationContext> newAction)
        {
            // Animation actions are updated regardless if operator was already animated
            if (KeepOriginalUpdateAction == null)
            {
                KeepOriginalUpdateAction = UpdateAction;
                KeepDirtyFlagTrigger = DirtyFlag.Trigger;
            }

            UpdateAction = newAction;
            DirtyFlag.Invalidate();
        }

        public virtual Action<EvaluationContext> UpdateAction { get; set; }

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly Action<EvaluationContext> EmptyAction = _ => { };

        protected Action<EvaluationContext> KeepOriginalUpdateAction;
        protected DirtyFlagTrigger KeepDirtyFlagTrigger;
    }
}