using System.Runtime.InteropServices;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace lib.math.vec3
{
	[Guid("94a5de3b-ee6a-43d3-8d21-7b8fe94b042b")]
    public class Vector3 : Instance<Vector3>, IExtractable
    {
        [Output(Guid = "AEDAEAD8-CCF0-43F0-9188-A79AF8D45250")]
        public readonly Slot<System.Numerics.Vector3> Result = new();

        public Vector3()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            Result.Value = new System.Numerics.Vector3(X.GetValue(context), Y.GetValue(context), Z.GetValue(context));
        }
        
        [Input(Guid = "084D5D0D-8FD4-431D-BF6C-8F082CCE1D3F")]
        public readonly InputSlot<float> X = new();

        [Input(Guid = "458891B9-0244-401A-B0A5-3A7EE365E7CB")]
        public readonly InputSlot<float> Y = new();
        
        [Input(Guid = "627F766E-056C-413E-8530-838D673BD031")]
        public readonly InputSlot<float> Z = new();

        public bool TryExtractInputsFor(InputSlot inputSlot, out IEnumerable<ExtractedInput> inputParameters)
        {
            if (inputSlot is not InputSlot<System.Numerics.Vector3> vecSlot)
            {
                inputParameters = Array.Empty<ExtractedInput>();
                return false;
            }

            var typedInputValue = vecSlot.TypedInputValue.Value;
            
            inputParameters = new[]
                                  {
                                      new ExtractedInput(X.Input, new InputValue<float>(typedInputValue.X)), 
                                      new ExtractedInput(Y.Input, new InputValue<float>(typedInputValue.Y)),
                                      new ExtractedInput(Z.Input, new InputValue<float>(typedInputValue.Z)),
                                  };
            return true;
        }
    }
}
