using System.Runtime.InteropServices;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Utils;

namespace lib.dx11.buffer
{
	[Guid("e6bbbeef-08d8-4105-b84d-39edadb549c0")]
    public class PickBuffer : Instance<PickBuffer>
    {
        [Output(Guid = "32D2645B-B627-437A-AFEC-7E728E2B54F5")]
        public readonly Slot<BufferWithViews> Output = new();
        
        [Output(Guid = "106C3BD6-BC99-4B7E-A411-E3044476D8E7")]
        public readonly Slot<int> Count = new();
        
        
        public PickBuffer()
        {
            Output.UpdateAction = Update;
            Count.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var connections = Input.InputConnectionsTyped;
            if (connections == null || connections.Count == 0)
            {
                Count.Value = 0;
                Output.DirtyFlag.Clear();
                return;
            }

            Count.Value = connections.Count;

            var index = Index.GetValue(context);
            
            Output.Value = connections[index.Mod(connections.Count)].GetValue(context);
            
            Output.DirtyFlag.Clear();
            Count.DirtyFlag.Clear();
        }        
        
        [Input(Guid = "04776dc8-7b84-41f5-973c-22cadbf44f02")]
        public readonly InputSlot<int> Index = new();

        [Input(Guid = "6B1C6232-819A-4021-82A9-994F8928BE13")]
        public readonly MultiInputSlot<BufferWithViews> Input = new();
    }
}

