using System.Runtime.InteropServices;
using T3.Core.DataTypes;
using T3.Core.IO;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.SystemUi;

namespace lib._3d.gizmo
{
	[Guid("d61d7192-9ca3-494e-91e2-10a530ee9375")]
    public class VisibleGizmos : Instance<VisibleGizmos>
    {
        [Output(Guid = "6c29ce06-0512-4815-bc83-ab2e095c0455")]
        public readonly Slot<Command> Output = new();

        public VisibleGizmos()
        {
            Output.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var visibility = Visibility.GetValue(context);
            if (visibility == T3.Core.Operator.GizmoVisibility.Inherit)
                visibility = context.ShowGizmos;

            var commands = Commands.InputConnectionsTyped;
            if (!_updatedOnce)
            {
                foreach (var t in commands)
                {
                    t.GetValue(context);
                }

                _updatedOnce = true;
            }


            var showIfSelected = false;
            if (visibility == T3.Core.Operator.GizmoVisibility.IfSelected)
            {
                Instance op = this;
                while (op != null)
                {
                    if (MouseInput.SelectedChildId == op.SymbolChildId)
                    {
                        showIfSelected = true;
                        break;
                    }

                    op = op.Parent;
                }
            }

            if (visibility != T3.Core.Operator.GizmoVisibility.On && !showIfSelected)
                return;
            
            
            if (commands.Count == 0)
            {
                return;
            }
            
            foreach (var t in commands)
            {
                t.GetValue(context);
            }
        }

        private bool _updatedOnce = true;

        
        [Input(Guid = "4F52683C-F2AA-4D3F-A964-F5232FA98872")]
        public readonly InputSlot<T3.Core.Operator.GizmoVisibility> Visibility = new();
        
        [Input(Guid = "4d663aa5-e2d4-40e0-8901-abe09cb832c3")]
        public readonly MultiInputSlot<Command> Commands = new();
    }
}