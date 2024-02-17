using System.Runtime.InteropServices;
using System.Collections.Generic;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Utils;

namespace lib.math.@float
{
	[Guid("6832d272-e63f-4ab2-b48c-85466b4ede1b")]
    public class PickFloatList : Instance<PickFloatList>
    {
        [Output(Guid = "32A7D943-E6AF-4186-9E1A-0800A62523C2")]
        public readonly Slot<List<float>> Selected = new();

        public PickFloatList()
        {
            Selected.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var connections = Input.InputConnectionsTyped;
            if (connections == null || connections.Count == 0)
                return;

            var index = Index.GetValue(context).Mod(connections.Count);
            Selected.Value = connections[index].GetValue(context);
        }

        [Input(Guid = "5A70A1AF-D6C5-43BF-ACB8-FCE2C9A2DE3C")]
        public readonly MultiInputSlot<List<float>> Input = new();

        [Input(Guid = "b22e2055-ccfd-4379-a3ef-d9b21d347f3d")]
        public readonly InputSlot<int> Index = new(0);
    }
}