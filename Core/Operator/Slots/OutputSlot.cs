using System;
using Log = T3.Core.Logging.Log;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace T3.Core.Operator.Slots;

public abstract class OutputSlot(Type type) : SlotBase(type)
{
    public sealed override int Invalidate()
    {
        if (AlreadyInvalidated(out var dirtyFlag))
            return dirtyFlag.Target;

        bool outputDirty = dirtyFlag.IsDirty;
        foreach (var input in Parent.Inputs)
        {
            input.Invalidate();
            outputDirty |= input.DirtyFlag.IsDirty;
        }

        if (outputDirty || (dirtyFlag.Trigger & DirtyFlagTrigger.Animated) == DirtyFlagTrigger.Animated)
        {
            dirtyFlag.Invalidate();
            if(_hasLinkSlot)
                _linkSlot.Invalidate();
        }

        dirtyFlag.SetVisited();
        return dirtyFlag.Target;
    }

    public InputSlot LinkSlot
    {
        get
        {
            if (_linkSlot != null)
                return _linkSlot;
            
            _linkSlot = CreateAndSetBypassToLinkSlot();
            _hasLinkSlot = true;
            return _linkSlot;
        }
    }

    protected abstract InputSlot CreateAndSetBypassToLinkSlot();

    private bool _hasLinkSlot;
    private InputSlot _linkSlot;

    protected virtual void SetDisabled(bool shouldBeDisabled)
    {
        if (shouldBeDisabled == _isDisabled)
            return;

        if (shouldBeDisabled)
        {
            if (KeepOriginalUpdateAction != null)
            {
                Log.Warning("Is already bypassed or disabled");
                return;
            }

            KeepOriginalUpdateAction = UpdateAction;
            KeepDirtyFlagTrigger = DirtyFlag.Trigger;
            UpdateAction = EmptyAction;
            DirtyFlag.Invalidate();
        }
        else
        {
            RestoreUpdateAction();
        }
    }

    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            if (_isDisabled == value)
                return;

            SetDisabled(value);
            _isDisabled = value;
        }
    }

    private bool _isDisabled;
}

internal sealed class MultiOutputSlot<T> : Slot<T[]>
{
    private readonly MultiInputSlot<T> _targetInputForBypass;

    public MultiOutputSlot(MultiInputSlot<T> targetInputForBypass) : base(Array.Empty<T>())
    {
        Parent = targetInputForBypass.Parent;
        _targetInputForBypass = targetInputForBypass;

        KeepOriginalUpdateAction = UpdateAction;
        KeepDirtyFlagTrigger = DirtyFlag.Trigger;
        UpdateAction = context => _targetInputForBypass.GetValues(ref Value, context);
    }

    protected override InputSlot CreateAndSetBypassToLinkSlot()
    {
        return _targetInputForBypass;
    }
}

public class Slot<T>(T defaultValue = default) : OutputSlot(typeof(T))
{
    public T Value = defaultValue;

    public T GetValue(EvaluationContext context)
    {
        Update(context);
        return Value;
    }

    public bool TrySetBypassToInput(InputSlot<T> targetSlot)
    {
        if (KeepOriginalUpdateAction != null)
        {
            //Log.Warning("Already disabled or bypassed");
            return false;
        }

        KeepOriginalUpdateAction = UpdateAction;
        KeepDirtyFlagTrigger = DirtyFlag.Trigger;
        UpdateAction = ByPassUpdate;
        DirtyFlag.Invalidate();
        _targetInputForBypass = targetSlot;
        return true;
    }

    private void ByPassUpdate(EvaluationContext context)
    {
        Value = _targetInputForBypass.GetValue(context);
    }

    private InputSlot<T> _targetInputForBypass;

    protected override InputSlot CreateAndSetBypassToLinkSlot()
    {
        var input = new InputSlot<T>();
        var set = TrySetBypassToInput(input);
        if (!set)
        {
            Log.Error($"{Parent.Symbol.Name}.{GetType()} Failed to set bypass to input");
        }

        return input;
    }
}