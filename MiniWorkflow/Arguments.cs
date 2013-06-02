using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    [Serializable]
    public sealed class OutArgument<T>
    {
        public T Value { get; set; }
    }

    [Serializable]
    public sealed class InArgument<T>
    {
        public OutArgument<T> LinkedArg { get; set; }
        public T Value
        {
            get
            {
                if (LinkedArg != null)
                    return LinkedArg.Value;
                else
                    return default(T);
            }

        }
    }
}
