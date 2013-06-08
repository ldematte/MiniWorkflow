using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    public class Parallel : CompositeActivity
    {
        protected internal override ActivityExecutionStatus Execute(WorkflowInstanceContext context)
        {
            foreach (var activity in children)
            {
                context.RunProgramStatement(activity, this.ContinueAt);
            }
            return ActivityExecutionStatus.Executing;
        }

        private void ContinueAt(WorkflowInstanceContext context, object arg)
        {
            foreach (var activity in children)
            {
                //Check every activity is closes...
                if (activity.ExecutionStatus != ActivityExecutionStatus.Closed)
                    return;
            }
            CloseActivity(context);
        }
    }
}
