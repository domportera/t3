using SharpDX.Direct3D11;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Resource;

namespace T3.Operators.Types.Id_a9bd30b1_8b58_4b88_8c8e_5f14b425edec
{
    public class VJSpermZoomer : Instance<VJSpermZoomer>
    {
        [Output(Guid = "e747675e-ca85-471c-9661-baae32112caa")]
        public readonly Slot<Texture2D> Output = new Slot<Texture2D>();

        [Input(Guid = "42d2618e-e968-441e-8fb7-453d4e326509")]
        public readonly InputSlot<Command> Scene = new InputSlot<Command>();


    }
}

