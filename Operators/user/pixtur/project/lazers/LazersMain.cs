using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace Operators.user.pixtur.project.lazers
{
	[Guid("c6f1bd33-7a38-43d0-bfaf-337cf59fcdb9")]
    public class LazersMain : Instance<LazersMain>
    {
        [Output(Guid = "537993d1-f651-454a-b60b-652206d6fc4e")]
        public readonly Slot<Texture2D> ImgOutput = new();


    }
}
