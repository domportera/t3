using System;
using T3.Core.Logging;
using T3.Core.Operator.Interfaces;

namespace T3.Core.Operator.Slots
{
    public sealed class TransformCallbackSlot<T> : Slot<T>
    {
        public ITransformable TransformableOp { get; set; }

        private new void Update(EvaluationContext context)
        {
            // FIXME: Casting is ugly. TransformCall should us ITransformable instead 
            TransformableOp.TransformCallback?.Invoke(TransformableOp as Instance, context);
            if (_baseUpdateAction == null)
            {
                Log.Warning("Failed to call base transform gizmo update for " + Parent.SymbolChildId, this.Parent.SymbolChildId);
                return;
            }
            _baseUpdateAction(context);
        }

        private Action<EvaluationContext> _baseUpdateAction;

        public override Action<EvaluationContext> UpdateAction
        {
            set
            {
                _baseUpdateAction = value;
                base.UpdateAction = Update;
            }
        }
    }
}