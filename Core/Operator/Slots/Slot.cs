using System;
using System.Runtime.CompilerServices;
using T3.Core.DataTypes;
using T3.Core.Logging;
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

        public abstract bool IsConnected { get; }
        private readonly bool _valueIsCommand;

        public void RestoreUpdateAction()
        {
            // This will happen when operators are recompiled and output slots are disconnected
            if (_keepOriginalUpdateAction == null)
            {
                UpdateAction = null;
                return;
            }

            UpdateAction = _keepOriginalUpdateAction;
            _keepOriginalUpdateAction = null;
            DirtyFlag.Trigger = _keepDirtyFlagTrigger;
            DirtyFlag.Invalidate();
        }

        protected virtual void SetDisabled(bool shouldBeDisabled)
        {
            if (shouldBeDisabled == _isDisabled)
                return;

            if (shouldBeDisabled)
            {
                if (_keepOriginalUpdateAction != null)
                {
                    Log.Warning("Is already bypassed or disabled");
                    return;
                }

                _keepOriginalUpdateAction = UpdateAction;
                _keepDirtyFlagTrigger = DirtyFlag.Trigger;
                UpdateAction = EmptyAction;
                DirtyFlag.Invalidate();
            }
            else
            {
                RestoreUpdateAction();
            }

            _isDisabled = shouldBeDisabled;
        }

        public void OverrideWithAnimationAction(Action<EvaluationContext> newAction)
        {
            // Animation actions are updated regardless if operator was already animated
            if (_keepOriginalUpdateAction == null)
            {
                _keepOriginalUpdateAction = UpdateAction;
                _keepDirtyFlagTrigger = DirtyFlag.Trigger;
            }

            UpdateAction = newAction;
            DirtyFlag.Invalidate();
        }

        public virtual Action<EvaluationContext> UpdateAction { get; set; }

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly Action<EvaluationContext> EmptyAction = _ => { };

        protected Action<EvaluationContext> _keepOriginalUpdateAction;
        protected DirtyFlagTrigger _keepDirtyFlagTrigger;

        public bool IsDisabled { get => _isDisabled; set => SetDisabled(value); }
        protected bool _isDisabled;
    }

    // Todo: this is an output slot - should be renamed in the future
}