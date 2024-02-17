using System.Runtime.InteropServices;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace lib.math.@int
{
	[Guid("cc07b314-4582-4c2c-84b8-bb32f59fc09b")]
    public class IntValue : Instance<IntValue>
    {
        [Output(Guid = "8A65B34B-40BE-4DBF-812C-D4C663464C7F")]
        public readonly Slot<int> Result = new();

        public IntValue()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            Result.Value = Int.GetValue(context);
        }
        
        [Input(Guid = "4515C98E-05BC-4186-8773-4D2B31A8C323")]
        public readonly InputSlot<int> Int = new();

        public bool TryExtractInputsFor(InputSlot inputSlot, out IEnumerable<ExtractedInput> inputParameters)
        {
            if (inputSlot is not InputSlot<int> intSlot)
            {
                inputParameters = Array.Empty<ExtractedInput>();
                return false;
            }

            inputParameters = new[] { new ExtractedInput(Int.Input, intSlot.TypedInputValue) };
            return true;
        }
    }
}
