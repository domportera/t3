using System;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace T3.Core.Operator.Slots;

public abstract class OutputSlot(Type type) : SlotBase(type)
{
    public sealed override bool IsConnected => _isConnected;

    public void AddConnection(InputSlot _)
    {
        _isConnected = true;
    }

    public void RemoveConnection()
    {
        _isConnected = false;
    }

    public override int Invalidate()
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
        }

        dirtyFlag.SetVisited();
        return dirtyFlag.Target;
    }

    private bool _isConnected;
}

public sealed class Slot<T>(T defaultValue = default) : OutputSlot(typeof(T))
{
    public T Value = defaultValue;

    public T GetValue(EvaluationContext context)
    {
        Update(context);
        return Value;
    }

    public bool TrySetBypassToInput(InputSlot<T> targetSlot)
    {
        if (_keepOriginalUpdateAction != null)
        {
            //Log.Warning("Already disabled or bypassed");
            return false;
        }

        _keepOriginalUpdateAction = UpdateAction;
        _keepDirtyFlagTrigger = DirtyFlag.Trigger;
        UpdateAction = ByPassUpdate;
        DirtyFlag.Invalidate();
        _targetInputForBypass = targetSlot;
        return true;
    }

    void ByPassUpdate(EvaluationContext context)
    {
        Value = _targetInputForBypass.GetValue(context);
    }

    private InputSlot<T> _targetInputForBypass; // todo - bypass is just for outputs, no?
}