#nullable enable
using System;
using T3.Core.Logging;

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
                return _linkSlot;
            }
        }

        private OutputSlot? _linkSlot;
        
        protected abstract OutputSlot CreateLinkSlot();
        public abstract void AddConnection(OutputSlot sourceSlot);
        public abstract void RemoveConnection();
    }

    public sealed class InputSlot<T> : InputSlot
    {
        public T Value;

        public override int Invalidate()
        {
            if (AlreadyInvalidated(out var dirtyFlag))
                return dirtyFlag.Target;

            if (IsConnected)
            {
                dirtyFlag.Target = _connectedOutput.Invalidate();
            }
            else
            {
                if (dirtyFlag.Trigger != DirtyFlagTrigger.None)
                    dirtyFlag.Invalidate();
            }

            dirtyFlag.SetVisited();
            return dirtyFlag.Target;
        }

        public override bool IsConnected { get; }

        public InputSlot(T value = default!) : base(typeof(T), false)
        {
            UpdateAction = InputUpdate;
            KeepOriginalUpdateAction = UpdateAction;
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

        protected  override OutputSlot CreateLinkSlot()
        {
            _linkSlot = new Slot<T>(TypedDefaultValue.Value);
            _linkSlot.TrySetBypassToInput(this);
            return _linkSlot;
        }

        public T GetValue(EvaluationContext context)
        {
            Update(context);
            return Value;
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
            if (sourceSlot is not Slot<T> correctOutputSlot)
            {
                Log.Warning("Type mismatch during connection");
                return;
            }
            
            if (!IsConnected)
            {
                _actionBeforeAddingConnecting = UpdateAction;
                UpdateAction = ConnectedUpdate;
                DirtyFlag.Target = correctOutputSlot.DirtyFlag.Target;
                DirtyFlag.Reference = DirtyFlag.Target - 1;
            }

            _connectedOutput = correctOutputSlot;
        }

        public override void RemoveConnection()
        {
            bool hasRemoved = false;
            if (IsConnected)
            {
                _connectedOutput = null;
                hasRemoved = true;
            }

            if (!hasRemoved)
            {
                if (_actionBeforeAddingConnecting != null)
                {
                    UpdateAction = _actionBeforeAddingConnecting;
                }
                else
                {
                    // if no connection is set anymore restore the default update action
                    RestoreUpdateAction();
                }

                DirtyFlag.Invalidate();
            }
        }

        private void ConnectedUpdate(EvaluationContext context)
        {
            Value = _connectedOutput.GetValue(context);
        }

        private void InputUpdate(EvaluationContext context)
        {
            Value = Input.IsDefault ? TypedDefaultValue.Value : TypedInputValue.Value;
        }

        private Action<EvaluationContext>? _actionBeforeAddingConnecting;

        public InputValue<T> TypedInputValue;
        public InputValue<T> TypedDefaultValue;

        private Slot<T>? _connectedOutput;
        public override SlotBase FirstConnectedSlot => _connectedOutput!;
        public override OutputSlot FirstConnection => _connectedOutput!;
        private Slot<T> _linkSlot;
    }
}