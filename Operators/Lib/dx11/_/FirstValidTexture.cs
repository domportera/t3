using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace lib.dx11.@_
{
	[Guid("db73b47d-3d42-4b17-b8fd-08b6f1286716")]
    public class FirstValidTexture : Instance<FirstValidTexture>
    {
        [Output(Guid = "3d3d2dbd-dadc-492d-bf03-b780b21e738e")]
        public readonly Slot<Texture2D> Output = new();
        
        
        public FirstValidTexture()
        {
            Output.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var foundSomethingValid = false;
            
            _complainedOnce |= Input.DirtyFlag.IsDirty;
            
            var connections = Input.InputConnectionsTyped;
            if (connections != null && connections.Count > 0)
            {
                for (int index = 0; index < connections.Count; index++)
                {
                    var v = connections[index].GetValue(context);
                    if (v != null)
                    {
                        Output.Value = v;
                        foundSomethingValid = true;
                        break;
                    }
                }
            }

            if (!foundSomethingValid && !_complainedOnce)
            {
                Log.Debug("No valid texture found", this);
                _complainedOnce = true;
            }
        }

        private bool _complainedOnce;
        
        [Input(Guid = "1725F61D-44E5-4718-9331-F6520F105657")]
        public readonly MultiInputSlot<Texture2D> Input = new();
    }
}

