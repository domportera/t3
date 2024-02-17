using System.Runtime.InteropServices;
using System.Numerics;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Utils;

namespace lib.math.vec3
{
	[Guid("fc201df2-8b05-4567-9f24-0d9128aa8507")]
    public class BlendVector3 : Instance<BlendVector3>
    {
        [Output(Guid = "A24028C7-5611-4F86-9580-B8D9DDF2CA25")]
        public readonly Slot<Vector3> Result = new();

        public BlendVector3()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            Result.Value = Vector3.Zero;

            var collectedTypedInputs = Vectors.OutputSlotsConnectedToMe;
            var count = collectedTypedInputs.Count;
            if (count == 0)
                return;
            
            var f = F.GetValue(context);
            
            var index1 = (int)MathUtils.Fmod((int)f, count);
            var index2 = (int)MathUtils.Fmod((int)(f+1), count);
            var mix = MathUtils.Fmod(f, 1);

            Result.Value = MathUtils.Lerp(collectedTypedInputs[index1].GetValue(context),
                                          collectedTypedInputs[index2].GetValue(context),
                                          mix);
        }
        
        
        [Input(Guid = "83C7B887-E1AF-4B9F-AD2F-469867940BDA")]
        public readonly MultiInputSlot<Vector3> Vectors = new();
        
        [Input(Guid = "f5f12cf3-5750-4a3c-807e-9da29f950c29")]
        public readonly InputSlot<float> F = new();

    }
}