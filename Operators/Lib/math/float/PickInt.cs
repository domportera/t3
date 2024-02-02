using System.Runtime.InteropServices;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Utils;

namespace lib.math.@float
{
	[Guid("81555155-ae6f-40aa-961d-b6badb77af21")]
    public class PickInt : Instance<PickInt>
    {
        [Output(Guid = "9DDD1C52-865A-4930-84EC-98D3C0FFAA9C")]
        public readonly Slot<int> Selected = new();

        public PickInt()
        {
            Selected.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var connections = InputValues.InputConnectionsTyped;
            if (connections == null || connections.Count == 0)
                return;

            var index = Index.GetValue(context).Mod(connections.Count);
            Selected.Value = connections[index].GetValue(context);
        }

        [Input(Guid = "2C0A4EB2-DA56-449D-91B8-5BA0870FBEB4")]
        public readonly MultiInputSlot<int> InputValues = new();

        [Input(Guid = "8bbc412b-f574-4a2b-9cbc-bf4f60aebb17")]
        public readonly InputSlot<int> Index = new(0);
    }
}