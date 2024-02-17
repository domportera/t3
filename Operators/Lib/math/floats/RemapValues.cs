using System.Runtime.InteropServices;
using System;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace lib.math.floats
{
	[Guid("efb9ecfc-5aa2-45f1-87b0-1455d7702aa7")]
    public class RemapValues : Instance<RemapValues>
    {
        [Output(Guid = "18bd8395-7116-425c-b580-1ce944beda65")]
        public readonly Slot<float> Result = new();

        public RemapValues()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var inputValue = InputValue.GetValue(context);

            var minDistances = float.PositiveInfinity;
            var bestValue = 0f;
            var bestMatchingIndex = -1;

            var list = InputAndOutputPairs.OutputSlotsConnectedToMe;
            for (var index = 0; index < list.Count; index++)
            {
                var input = list[index];
                var lookUpValue = input.GetValue(context);
                var distance = MathF.Abs(lookUpValue.X - inputValue);
                if (distance < minDistances)
                {
                    minDistances = distance;
                    bestValue = lookUpValue.Y;
                    bestMatchingIndex = index;
                }
            }

            if (bestMatchingIndex == -1)
            {
                Log.Warning("RemapValues requires at least one remap pair", this);
                Result.Value = 0;
            }
            else
            {
                Result.Value = bestValue;
            }

        }

        [Input(Guid = "9871AD71-30B0-4454-9418-0916FD58AFA8")]
        public readonly InputSlot<float> InputValue = new();

        
        [Input(Guid = "4B0D7BBC-E29A-4E06-BD80-93413419634C")]
        public readonly MultiInputSlot<System.Numerics.Vector2> InputAndOutputPairs = new();
    }
}