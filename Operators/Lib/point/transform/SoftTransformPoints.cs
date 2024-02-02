using System.Runtime.InteropServices;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Interfaces;
using T3.Core.Operator.Slots;

namespace lib.point.transform
{
	[Guid("697bbc2d-0b2e-4013-bbc3-d58a28a79f31")]
    public class SoftTransformPoints : Instance<SoftTransformPoints>, ITransformable
    {

        [Output(Guid = "b3309ed0-574f-4907-b477-4a1cf98b2fe5")]
        public readonly TransformCallbackSlot<T3.Core.DataTypes.BufferWithViews> Output = new();

        public SoftTransformPoints()
        {
            Output.TransformableOp = this;
        }
        
        InputSlot ITransformable.TranslationInput => VolumeCenter;
        InputSlot ITransformable.RotationInput => Rotate;
        InputSlot ITransformable.ScaleInput => Scale;
        public Action<Instance, EvaluationContext> TransformCallback { get; set; }

        [Input(Guid = "5fac3f09-d6dd-4cba-8575-983353e60af4")]
        public readonly InputSlot<T3.Core.DataTypes.BufferWithViews> Points = new();

        [Input(Guid = "1055ad89-2aa1-493f-b991-ae55b7fbf2e4")]
        public readonly InputSlot<System.Numerics.Vector3> Translate = new();

        [Input(Guid = "a867c29a-3cca-49c0-92ae-a7d094b5213b")]
        public readonly InputSlot<float> Dither = new();

        [Input(Guid = "663e5d09-da7a-447c-abdd-984cc3ef5e4a")]
        public readonly InputSlot<System.Numerics.Vector3> Stretch = new();

        [Input(Guid = "cdf7c96c-7630-4889-b7a9-4ae0c3160119")]
        public readonly InputSlot<float> Scale = new();

        [Input(Guid = "e1c9d413-00b9-4d5d-81b6-6fa960a159be")]
        public readonly InputSlot<System.Numerics.Vector3> Rotate = new();

        [Input(Guid = "5934035c-098f-41b8-863a-c276f2bd9699")]
        public readonly InputSlot<float> ScaleW = new();

        [Input(Guid = "d82e0f6a-138a-43fd-8774-29dc33ddd672")]
        public readonly InputSlot<float> OffsetW = new();

        [Input(Guid = "3a7828e2-f58e-4229-b6c2-636cd5dbd011")]
        public readonly InputSlot<System.Numerics.Vector3> VolumeCenter = new();

        [Input(Guid = "24a0635f-9599-4a53-a35a-de90f4719f56", MappedType = typeof(Shapes))]
        public readonly InputSlot<int> VolumeType = new();

        [Input(Guid = "76a7afbe-4782-4b3c-bc35-cc818cf06ab2")]
        public readonly InputSlot<System.Numerics.Vector3> VolumeStretch = new();

        [Input(Guid = "36e2d2a8-4910-4c83-b9ff-ced8df59c7f1")]
        public readonly InputSlot<float> VolumeSize = new();

        [Input(Guid = "1e1f40ea-15af-4191-b3ce-d2edc3eee243")]
        public readonly InputSlot<float> FallOff = new();

        [Input(Guid = "f98281bc-89cb-4ac7-9d27-e045e712eb3a")]
        public readonly InputSlot<float> Bias = new();

        [Input(Guid = "8cd72c75-e73d-4d29-a5a9-e2d1a9ebe5e7")]
        public readonly InputSlot<bool> UseWAsWeight = new();

        [Input(Guid = "f9025937-8e74-4f2d-b8f1-90e56e601137")]
        public readonly InputSlot<T3.Core.Operator.GizmoVisibility> Visibility = new();

        
        private enum Shapes
        {
            Sphere,
            Box,
            Plane,
            Zebra,
        }
        
    }
}

