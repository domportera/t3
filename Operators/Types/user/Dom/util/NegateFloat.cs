using System;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_e9da4504_bd3b_4091_af32_b12a5efdb465
{
    public class NegateFloat : Instance<NegateFloat>
    {

        [Input(Guid = "2fda6585-df56-4ec3-a413-c6c31107b272")]
        public readonly InputSlot<float> A = new InputSlot<float>();

        [Output(Guid = "b407347e-10dc-4816-9e8b-0226f5a78383")]
        public readonly Slot<float> Result = new Slot<float>();

    }
}

