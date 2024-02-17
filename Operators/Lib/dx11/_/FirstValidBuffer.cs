using System.Runtime.InteropServices;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace lib.dx11._
{
	[Guid("b4a8a055-6ae3-4b56-8b65-1b7b5f87d19a")]
    public class FirstValidBuffer : Instance<FirstValidBuffer>
    {
        [Output(Guid = "bf3a690e-8611-470c-aad0-8099908e63c8")]
        public readonly Slot<BufferWithViews> Output = new();
        
        
        public FirstValidBuffer()
        {
            Output.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var connections = Input.OutputSlotsConnectedToMe;
            if (connections == null || connections.Count == 0)
                return;

            for (int index = 0; index < connections.Count; index++)
            {
                var v =  connections[index].GetValue(context);
                if (v != null)
                {
                    Output.Value = v;
                    break;
                }
            }
        }        
        
        [Input(Guid = "73cf2380-b592-4c63-9e62-70411e4f3ad5")]
        public readonly MultiInputSlot<BufferWithViews> Input = new();
    }
}

