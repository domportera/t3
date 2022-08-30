using SharpDX.Direct3D11;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_7845ec24_e249_4c6a_84e3_63422ede1a1d
{
    public class _VideoFormatConversion : Instance<_VideoFormatConversion>
    {
        [Output(Guid = "a722cd4f-82da-4532-a7bc-7a6bdc30b06e")]
        public readonly Slot<Texture2D> Output = new Slot<Texture2D>();


        [Input(Guid = "4454891b-ddd0-41f2-80c3-75074df8320b")]
        public readonly InputSlot<Texture2D> Texture = new InputSlot<Texture2D>();

    }
}

