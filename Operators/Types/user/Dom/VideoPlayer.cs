using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_dbd6650c_19a9_4e56_836d_1c26c99a6eae
{
    public class VideoPlayer : Instance<VideoPlayer>
    {

        [Output(Guid = "67b0df16-bdb2-4e33-b49d-17d626fd206d")]
        public readonly TimeClipSlot<T3.Core.DataTypes.Command> ClipCommand = new TimeClipSlot<T3.Core.DataTypes.Command>();

        [Output(Guid = "a3443ccc-70f3-48f2-9bc9-9e97c144bd88")]
        public readonly Slot<SharpDX.Direct3D11.Texture2D> Texture = new Slot<SharpDX.Direct3D11.Texture2D>();

        [Input(Guid = "c0059007-19f5-46df-bd02-747ae39fa59b")]
        public readonly InputSlot<string> Path = new InputSlot<string>();

        [Input(Guid = "5c589a61-ff28-4789-9635-7cdb098afd9e")]
        public readonly InputSlot<float> StartTime = new InputSlot<float>();

        [Input(Guid = "efa1af06-c559-4a16-b3e0-a96200f48375")]
        public readonly InputSlot<float> Volume = new InputSlot<float>();

        [Input(Guid = "793a470f-1b94-4766-899f-3010b03a7834")]
        public readonly InputSlot<float> RestartThreshold = new InputSlot<float>();

        [Input(Guid = "66b93a87-a80c-47bf-bff2-1e2cc178555a")]
        public readonly InputSlot<float> Speed = new InputSlot<float>();

        [Input(Guid = "361e62b4-fa0d-44e1-a1d7-65f99c38a45f")]
        public readonly InputSlot<T3.Core.DataTypes.Command> Command = new InputSlot<T3.Core.DataTypes.Command>();

        [Input(Guid = "3a3b9866-85e9-4048-bb1c-faccd4e513c7")]
        public readonly InputSlot<SharpDX.DXGI.Format> VideoFormatConversion = new InputSlot<SharpDX.DXGI.Format>();

    }
}

