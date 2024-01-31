#nullable enable
using System;
using T3.Core.Logging;

namespace T3.Core.Operator.Slots
{
    public abstract class InputSlot : SlotBase
    {
        public readonly Type MappedType;

        public bool TryGetAsMultiInput(out MultiInputSlot multiInput)
        {
            multiInput = ThisAsMultiInputSlot;
            return IsMultiInput;
        }

        public readonly bool IsMultiInput;

        protected InputSlot(Type type, bool isMultiInput) : base(type)
        {
            MappedType = type;
            IsMultiInput = isMultiInput;
            
            if (isMultiInput)
            {
                ThisAsMultiInputSlot = (MultiInputSlot)this;
            }
        }

        private MultiInputSlot ThisAsMultiInputSlot { get; }
    }

    public sealed class InputSlot<T> : InputSlot
    {
        public T Value { get; private set; }

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
            var typedInputValue = new InputValue<T>(value);
            UpdateAction = InputUpdate;
            _keepOriginalUpdateAction = UpdateAction;
            TypedInputValue = typedInputValue;
            Value = typedInputValue.Value;
        }

        private SymbolChild.Input _input;

        public SymbolChild.Input Input
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

        public void AddConnection(OutputSlot sourceSlot)
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

        public void RemoveConnection()
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
    }
}