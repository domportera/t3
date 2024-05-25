using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace examples.user.newemka980.visuals
{
    [Guid("40a73341-0210-4d77-b893-b57dfd3d9d90")]
    public class SphereFlower : Instance<SphereFlower>
    {
        [Output(Guid = "0527ab8f-e0a6-4630-a4dc-61cf41a47581")]
        public readonly Slot<Texture2D> ColorBuffer = new Slot<Texture2D>();


    }
}
