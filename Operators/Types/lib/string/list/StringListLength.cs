using System.Collections.Generic;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_5ebb6041_2857_4656_9e42_8b13095f63ed
{
    public class StringListLength : Instance<StringListLength>
    {
        [Output(Guid = "019e49f2-584f-4cda-973b-8a4d16d3409d")]
        public readonly Slot<int> Length = new Slot<int>();

        public StringListLength()
        {
            Length.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var list = Input.GetValue(context);
            if (list == null)
            {
                Length.Value = 0;
                return;
            }
            
            Length.Value = list.Count;
        }

        [Input(Guid = "2a97ee00-5a10-4dec-a13a-bd047aa131fb")]
        public readonly InputSlot<List<string>> Input = new InputSlot<List<string>>();
    }
}