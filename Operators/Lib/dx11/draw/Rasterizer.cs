using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Resource;

namespace lib.dx11.draw
{
	[Guid("fbd7f0f0-36a3-4fbb-91e1-cb33d4666d09")]
    public class Rasterizer : Instance<Rasterizer>
    {
        [Output(Guid = "C723AD69-FF0C-47B2-9327-BD27C0D7B6D1")]
        public readonly Slot<Command> Output = new(new Command());

        public Rasterizer()
        {
            Output.UpdateAction = Update;
            Output.Value.RestoreAction = Restore;
        }

        private void Update(EvaluationContext context)
        {
            var resourceManager = ResourceManager.Instance();
            var device = ResourceManager.Device;
            var deviceContext = device.ImmediateContext;
            var rasterizer = deviceContext.Rasterizer;

            ScissorRectangles.GetValues(context);
            _prevViewports = rasterizer.GetViewports<RawViewportF>();
            
            Viewports.GetValues(ref _viewports, context);

            _prevState = rasterizer.State; 
            var newState = RasterizerState.GetValue(context);
            rasterizer.State = newState;

            if (_viewports.Length > 0)
                rasterizer.SetViewports(_viewports, _viewports.Length);
        }

        private void Restore(EvaluationContext context)
        {
            var deviceContext = ResourceManager.Device.ImmediateContext;
            var rasterizer = deviceContext.Rasterizer;
            rasterizer.SetViewports(_prevViewports, _prevViewports.Length);
            rasterizer.State = _prevState;
        }

        private RawViewportF[] _viewports = new RawViewportF[0];
        private RawViewportF[] _prevViewports;

        [Input(Guid = "35A52074-1E82-4352-91C3-D8E464F73BC7")]
        public readonly InputSlot<RasterizerState> RasterizerState = new();
        [Input(Guid = "73945E5D-3C3C-4742-B341-A061B0DC116F")]
        public readonly MultiInputSlot<RawViewportF> Viewports = new();
        [Input(Guid = "3F71BE22-9DC2-4E47-8B3A-1EF3C9ECBD9D")]
        public readonly MultiInputSlot<RawRectangle> ScissorRectangles = new();

        private RasterizerState _prevState;
    }
}