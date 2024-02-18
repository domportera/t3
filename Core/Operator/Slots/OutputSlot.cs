using System;
using T3.Core.Stats;
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
        foreach (var input in Parent.Inputs) // todo: is this necessary? should we only invalidate actual connected inputs of this slot?
        {
            input.Invalidate();
            outputDirty |= input.DirtyFlag.IsDirty;
        }

        if (_hasLinkSlot)
        {
            _linkSlot.Invalidate();
            outputDirty |= _linkSlot.DirtyFlag.IsDirty;
        }

        if (outputDirty || (dirtyFlag.Trigger & DirtyFlagTrigger.Animated) == DirtyFlagTrigger.Animated)
        {
            dirtyFlag.Invalidate();
        }

        dirtyFlag.SetVisited();
        return dirtyFlag.Target;
    }

    public InputSlot LinkSlot
    {
        get
        {
            lock (_linkSlotLock)
            {
                if (_linkSlot != null)
                    return _linkSlot;

                _linkSlot = InitializeLinkSlot();

                _linkSlot.Parent = Parent;
                _linkSlot.Id = Id;
                _linkSlot.Invalidate();
                _hasLinkSlot = true;
                return _linkSlot;
            }
        }
    }

    protected abstract InputSlot InitializeLinkSlot();

    private bool _hasLinkSlot;
    private InputSlot _linkSlot;

    public bool IsDisabled
    {
        set
        {
            if (_isDisabled == value)
                return;

            _isDisabled = value;
            SetDisabled(value);
            DirtyFlag.Invalidate();
        }
    }

    private bool _isDisabled;
    protected abstract void SetDisabled(bool value);
    public virtual Action<EvaluationContext> UpdateAction { get; set; }
    private readonly object _linkSlotLock = new();
}

/// <summary>
/// This is not a fully fledged slot type - it is only used as a link slot for MultiInputSlots
/// Therefore, the implementation is *purely* as a bypassed slot and is thus very simple
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class MultiOutputSlot<T> : Slot<T[]>
{
    private readonly MultiInputSlot<T> _targetInputForBypass;

    public MultiOutputSlot(MultiInputSlot<T> targetInputForBypass) : base(Array.Empty<T>())
    {
        Parent = targetInputForBypass.Parent;
        _targetInputForBypass = targetInputForBypass;
        UpdateAction = context => _targetInputForBypass.GetValues(ref Value, context);
    }

    protected override InputSlot InitializeLinkSlot()
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
        DirtyFlag.Invalidate();
        _targetInputForBypass = targetSlot;
        _updateMode = BypassUpdate;
        return true;
    }

    protected sealed override void SetDisabled(bool value) => _isDisabled = value;

    public sealed override void Update(EvaluationContext context)
    {
        if (_isDisabled)
            return;

        if (!DirtyFlag.IsDirty && !ValueIsCommand)
            return;

        switch (_updateMode)
        {
            case NormalUpdate:
                UpdateAction?.Invoke(context);
                break;
            case BypassUpdate:
                Value = ByPassUpdate(context, _targetInputForBypass);
                break;
            case LinkUpdate:
                Value = ByPassUpdate(context, _linkSlot);
                //UpdateAction?.Invoke(context);
                
                break;
        }

        DirtyFlag.Clear();
        DirtyFlag.SetUpdated();
        OpUpdateCounter.CountUp();
    }

    public void SetUnBypassed()
    {
        _updateMode = NormalUpdate;
        _targetInputForBypass = null;
        DirtyFlag.Invalidate();
    }

    private T ByPassUpdate(EvaluationContext context, InputSlot<T> linkSlot) => linkSlot.GetValue(context);

    private InputSlot<T> _targetInputForBypass;
    private InputSlot<T> _linkSlot;

    protected override InputSlot InitializeLinkSlot()
    {
        _linkSlot = new InputSlot<T>();
        var inputDefinition = new Symbol.InputDefinition();
        inputDefinition.IsMultiInput = false;
        inputDefinition.Id = Id;
        inputDefinition.DefaultValue = new InputValue<T>(_defaultValue);
        _linkSlot.Input = new SymbolChild.Input(inputDefinition);
        _updateMode = LinkUpdate;
        
        return _linkSlot;
    }

    private readonly T _defaultValue = defaultValue;
    private bool _isDisabled;
    private int _updateMode = NormalUpdate;
    private const int NormalUpdate = 0;
    private const int BypassUpdate = 1;
    private const int LinkUpdate = 2;
}