using System.Runtime.InteropServices;
using T3.Core.DataTypes.Vector;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace Operators.lib._3d._
{
	[Guid("08d526d0-d5e5-4fc9-a039-98189721d2b8")]
    public class _ComputeBRDFLookup : Instance<_ComputeBRDFLookup>
    {

        [Output(Guid = "21e0ee79-8e98-45aa-86e9-194ca6d70989")]
        public readonly Slot<SharpDX.Direct3D11.Texture2D> BRDF = new();

        [Input(Guid = "e22057e4-1aae-4698-b7f6-120dde027a5d")]
        public readonly InputSlot<Int2> Size = new();

        private enum Modes
        {
            Linear,
            LegacyDOF,
        }
    }
}
