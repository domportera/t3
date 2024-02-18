#nullable enable
using System;
using System.Runtime.CompilerServices;
using T3.Core.Logging;
using T3.Core.Stats;

namespace T3.Core.Operator.Slots
{
    public abstract class InputSlot : SlotBase
    {
        public Type MappedType { get; internal set; }

        public bool TryGetAsMultiInput(out MultInputSlot multiInput)
        {
            multiInput = ThisAsMultInputSlot;
            return IsMultiInput;
        }

        public readonly bool IsMultiInput;

        protected InputSlot(Type type, bool isMultiInput) : base(type)
        {
            MappedType = type;
            IsMultiInput = isMultiInput;

            if (isMultiInput)
            {
                ThisAsMultInputSlot = (MultInputSlot)this;
            }
        }

        private MultInputSlot ThisAsMultInputSlot { get; }
        public abstract OutputSlot FirstConnection { get; }
        public abstract SymbolChild.Input Input { get; set; }

        /// <summary>
        /// used to connect an instance's input slots into its children's input slots
        /// </summary>
        public OutputSlot LinkSlot
        {
            get
            {
                if (_linkSlot != null)
                    return _linkSlot;

                _linkSlot = CreateLinkSlot();
                _linkSlot.Id = Id;
                _linkSlot.Parent = Parent;
                return _linkSlot;
            }
        }

        private OutputSlot? _linkSlot;

        protected abstract OutputSlot CreateLinkSlot();
        public abstract void AddConnection(OutputSlot sourceSlot);
        public abstract void RemoveConnection();
        public abstract bool IsConnected { get; }
        protected int UpdateMode = NormalUpdate;
        protected const int NormalUpdate = 0;
        
        // animation

        public void RestoreUpdateAction()
        {
            if (UpdateMode == AnimationUpdate)
            {
                DirtyFlag.Trigger = _keepDirtyFlagTrigger;
                AnimationUpdateAction = null;
            }

            UpdateMode = NormalUpdate;
            DirtyFlag.Invalidate();
        }

        public void OverrideWithAnimationAction(Action<EvaluationContext> newAction)
        {
            AnimationUpdateAction = newAction;
            UpdateMode = AnimationUpdate;
            _keepDirtyFlagTrigger = DirtyFlag.Trigger;
            DirtyFlag.Invalidate();
        }

        private DirtyFlagTrigger _keepDirtyFlagTrigger;
        private protected Action<EvaluationContext>? AnimationUpdateAction;
        private protected const int AnimationUpdate = 1;
    }

    public sealed class InputSlot<T> : InputSlot
    {
        public T Value;

        public override int Invalidate()
        {
            if (AlreadyInvalidated(out var dirtyFlag))
                return dirtyFlag.Target;

            if (_isConnected)
            {
                dirtyFlag.Target = _connectedOutput!.Invalidate();
                return dirtyFlag.Target;
            }

            if (dirtyFlag.Trigger != DirtyFlagTrigger.None)
            {
                dirtyFlag.Invalidate();
                dirtyFlag.SetVisited();
                return _hasLinkSlot ? _linkSlot.Invalidate() : dirtyFlag.Target;
            }

            dirtyFlag.SetVisited();
            return dirtyFlag.Target;
        }

        public InputSlot(T value = default!) : base(typeof(T), false)
        {
            var typedInputValue = new InputValue<T>(value);
            TypedInputValue = typedInputValue;
            TypedDefaultValue = new InputValue<T>(value);
            Value = typedInputValue.Value;
        }

        private SymbolChild.Input _input;

        public override SymbolChild.Input Input
        {
            get => _input;
            set
            {
                _input = value;
                TypedInputValue = (InputValue<T>)value.Value;
                TypedDefaultValue = (InputValue<T>)value.DefaultValue;

                if (_input.IsDefault && TypedDefaultValue.IsEditableInputReferenceType)
                {
                    TypedInputValue.AssignClone(TypedDefaultValue);
                }
            }
        }

        protected override OutputSlot CreateLinkSlot()
        {
            _linkSlot = new Slot<T>(TypedDefaultValue.Value);
            if (!_linkSlot.TrySetBypassToInput(this))
            {
                Log.Error($"Failed to set bypass to link slot");
            }

            _hasLinkSlot = true;
            return _linkSlot;
        }

        public T GetValue(EvaluationContext context)
        {
            Update(context);
            return Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Update(EvaluationContext context)
        {
            if (!DirtyFlag.IsDirty && !ValueIsCommand)
                return;

            switch (UpdateMode)
            {
                case NormalUpdate:
                    InputUpdate(context);
                    break;

                case AnimationUpdate:
                    AnimationUpdateAction?.Invoke(context);
                    break;
            }

            DirtyFlag.Clear();
            DirtyFlag.SetUpdated();
            OpUpdateCounter.CountUp();
        }

        public T GetCurrentValue()
        {
            return IsConnected
                       ? Value
                       : TypedInputValue.Value;
        }

        public void SetTypedInputValue(T newValue)
        {
            Input.IsDefault = false;
            TypedInputValue.Value = newValue;
            Value = newValue;
            DirtyFlag.Invalidate();
        }

        public override void AddConnection(OutputSlot sourceSlot)
        {
            // todo - generic version of this function?
            if (sourceSlot is not Slot<T> correctSourceSlot)
            {
                Log.Warning($"Type mismatch during connection: [{sourceSlot.Parent}].{sourceSlot.GetType()} --> [{Parent}].{GetType()}");
                return;
            }

            if (!_isConnected)
            {
                DirtyFlag.Target = sourceSlot.DirtyFlag.Target;
                DirtyFlag.Reference = DirtyFlag.Target - 1;
            }

            _connectedOutput = correctSourceSlot;
            _isConnected = true;
        }

        public override void RemoveConnection()
        {
            if (!_isConnected)
                return;

            _isConnected = false;
            _connectedOutput = null;
            RestoreUpdateAction();
            DirtyFlag.Invalidate();
        }

        // todo: this probably doesnt need to run as frequently as it does if it is not connected
        private void InputUpdate(EvaluationContext context)
        {
            if (_isConnected)
                Value = _connectedOutput!.GetValue(context);
            else
                Value = Input.IsDefault ? TypedDefaultValue.Value : TypedInputValue.Value;
        }

        public InputValue<T> TypedInputValue;
        public InputValue<T> TypedDefaultValue;

        private Slot<T>? _connectedOutput;
        public override OutputSlot FirstConnection => _connectedOutput!;
        private Slot<T> _linkSlot;
        private bool _hasLinkSlot;
        public override bool IsConnected => _isConnected;
        private bool _isConnected;
    }
}