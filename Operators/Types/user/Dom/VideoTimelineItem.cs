using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_deb6b806_db8f_41e1_adea_8229953bd76f
{
    public class VideoTimelineItem : Instance<VideoTimelineItem>
    {

        [Input(Guid = "e4cd8557-cd10-4bc0-95b5-59ab59ebe36c")]
        public readonly InputSlot<string> Path = new InputSlot<string>();

        [Input(Guid = "141ae1aa-8384-4542-a654-b2818f84db22")]
        public readonly MultiInputSlot<float> ClipStartTimes = new MultiInputSlot<float>();

        [Input(Guid = "a6b5d9eb-6b30-4d40-b54d-eb0a662ea0fc")]
        public readonly InputSlot<int> SelectedClip = new InputSlot<int>();

        [Input(Guid = "4ad01b88-0f36-44ad-89a2-b8eeeea13f1d")]
        public readonly InputSlot<float> Volume = new InputSlot<float>();

        [Input(Guid = "9728fd42-2a36-419d-81b5-a1024b15ef3e")]
        public readonly InputSlot<float> RestartThreshold = new InputSlot<float>();

        [Input(Guid = "16415c0d-7da2-4948-9405-40c96c5de885")]
        public readonly InputSlot<float> Speed = new InputSlot<float>();

        [Input(Guid = "3cf434cf-7184-47b5-ac6d-a5c1fc9d3944")]
        public readonly InputSlot<T3.Core.DataTypes.Command> Command = new InputSlot<T3.Core.DataTypes.Command>();

        [Input(Guid = "a04af213-f0f5-4d98-ab5b-7e20b0212ae8")]
        public readonly InputSlot<SharpDX.DXGI.Format> VideoFormatConversion = new InputSlot<SharpDX.DXGI.Format>();

        [Output(Guid = "a1a31c94-02ae-4382-a626-a4a3d0378288")]
        public readonly TimeClipSlot<T3.Core.DataTypes.Command> ExecuteCommand = new TimeClipSlot<T3.Core.DataTypes.Command>();

        [Output(Guid = "a3264eb6-71df-4003-b4a1-88d14171463d")]
        public readonly TimeClipSlot<SharpDX.Direct3D11.Texture2D> OutTex = new TimeClipSlot<SharpDX.Direct3D11.Texture2D>();


    }
}

