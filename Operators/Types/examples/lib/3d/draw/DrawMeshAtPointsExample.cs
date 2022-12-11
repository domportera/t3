using T3.Core;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Resource;

namespace T3.Operators.Types.Id_4f113e4a_eb27_4e40_8843_d15d54610f33
{
    public class DrawMeshAtPointsExample : Instance<DrawMeshAtPointsExample>
    {

        [Output(Guid = "823e0f6a-518b-46cd-a929-7e069fe653a7")]
        public readonly Slot<SharpDX.Direct3D11.Texture2D> ImageOutput = new Slot<SharpDX.Direct3D11.Texture2D>();


    }
}

