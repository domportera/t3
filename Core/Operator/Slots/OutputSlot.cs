using System;
using System.Collections.Generic;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

namespace T3.Core.Operator.Slots;

public abstract class OutputSlot(Type type) : SlotBase(type)
{
    public sealed override bool IsConnected => _isConnected;

    public void AddConnection(InputSlot input)
    {
        _isConnected = true;
        _connectedInput = input;
    }

    public void RemoveConnection()
    {
        _isConnected = false;
        _connectedInput = null;
    }

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
        }
        
        dirtyFlag.SetVisited();
        return dirtyFlag.Target;
    }
    
    public InputSlot ConnectedSlot => _connectedInput;

    public InputSlot LinkSlot
    {
        get
        {
            if (_linkSlot != null)
                return _linkSlot;

            _linkSlot = GetLinkSlot();
            return _linkSlot;
        }
    }

    protected abstract InputSlot GetLinkSlot();

    private InputSlot _linkSlot;
    private InputSlot _connectedInput;
    public override SlotBase FirstConnectedSlot => _connectedInput;
    private bool _isConnected;
}

internal abstract class MultiOutputSlot(Type type) : OutputSlot(type)
{
    
}

internal class MultiOutputSlot<T> : MultiOutputSlot
{
    public IReadOnlyList<T> Values => _values;
    private T[] _values;
    private MultiInputSlot<T> _targetInputForBypass;

    public MultiOutputSlot(MultiInputSlot<T> targetInputForBypass) : base(typeof(T))
    {
        _targetInputForBypass = targetInputForBypass;
        SetBypassToInput(targetInputForBypass);
    }

    private void SetBypassToInput(MultiInputSlot<T> slot)
    {
        KeepOriginalUpdateAction = UpdateAction;
        KeepDirtyFlagTrigger = DirtyFlag.Trigger;
        UpdateAction = ByPassUpdate;
        DirtyFlag.Invalidate();
        _targetInputForBypass = slot;
    }

    private void ByPassUpdate(EvaluationContext context)
    {
        _targetInputForBypass.GetValues(ref _values, context);
    }

    protected override InputSlot GetLinkSlot()
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
    protected override InputSlot GetLinkSlot()
    {
        var input = new InputSlot<T>();
        input.AddConnection(this);
        return input;
    }
}