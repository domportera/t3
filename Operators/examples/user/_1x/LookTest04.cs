using System.Runtime.InteropServices;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace examples.user._1x
{
    [Guid("d46236a7-b549-41b7-8b55-1b809310d191")]
    public class LookTest04 : Instance<LookTest04>
    {

        [Output(Guid = "4fd5be5e-ff6b-48f4-875a-066cd0703850")]
        public readonly Slot<SharpDX.Direct3D11.Texture2D> TextureOutput = new Slot<SharpDX.Direct3D11.Texture2D>();


    }
}
