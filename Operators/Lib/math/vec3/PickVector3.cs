using System.Runtime.InteropServices;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Utils;

namespace lib.math.vec3
{
	[Guid("19b91067-8a0f-4a3c-a68e-b353bffd9657")]
    public class PickVector3 : Instance<PickVector3>
    {
        [Output(Guid = "2e186c60-56d0-4f0f-839e-ab2dbfd49ae1")]
        public readonly Slot<System.Numerics.Vector3> Selected = new();

        public PickVector3()
        {
            Selected.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var connections = Input.OutputSlotsConnectedToMe;
            if (connections == null || connections.Count == 0)
                return;

            var index = Index.GetValue(context).Mod(connections.Count);
            Selected.Value = connections[index].GetValue(context);
        }

        [Input(Guid = "0aef2a2e-df91-4a03-a59e-e87ed0b916cb")]
        public readonly MultiInputSlot<System.Numerics.Vector3> Input = new();

        [Input(Guid = "ea7a0ae7-2f4b-4952-9da4-45b9691c154e")]
        public readonly InputSlot<int> Index = new(0);
    }
}