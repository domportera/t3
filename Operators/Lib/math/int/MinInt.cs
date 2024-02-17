using System.Runtime.InteropServices;
using System;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace lib.math.@int
{
	[Guid("d1530d89-32b9-4e16-97fe-c08d095d9d03")]
    public class MinInt : Instance<MinInt>
    {
        [Output(Guid = "82ff7dfe-e65f-415b-989c-8b478650b5d7")]
        public readonly Slot<int> Result = new();


        public MinInt()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var min = Int32.MaxValue;
            
            foreach (var i in Ints.InputConnectionsTyped)
            {
                min = Math.Min(min, i.GetValue(context));
            }

            Result.Value = min;
        }


        [Input(Guid = "6bb2b4eb-f5d9-43f0-8584-4ea3d38f6538")]
        public readonly MultiInputSlot<int> Ints = new();
    }
}
