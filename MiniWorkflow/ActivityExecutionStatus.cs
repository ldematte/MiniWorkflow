using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniWorkflow
{
    public enum ActivityExecutionStatus
    {
        Initialized,
        Executing,
        Canceling,
        Closed, 
        Compensating,
        Faulting
    }
}
