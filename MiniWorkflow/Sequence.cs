using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{   

    [Serializable]
    public class Sequence : CompositeActivity
    {
        int currentIndex;
        protected internal override ActivityExecutionStatus Execute(WorkflowInstanceContext context)
        {
            logger.Debug("Sequence::Execute");
            currentIndex = 0;
            // Empty statement block
            if (children.Count == 0)
            {
                context.CloseActivity();
                return ActivityExecutionStatus.Closed;
            }
            else
            {
                context.RunProgramStatement(children[0], ContinueAt);
                return ActivityExecutionStatus.Executing;
            }
        }

        public void ContinueAt(WorkflowInstanceContext context, object value)
        {
            logger.Debug("Sequence::ContinueAt");
            // If we've run all the statements, we're done
            if (++currentIndex == children.Count) 
                context.CloseActivity();
            else // Else, run the next statement
                context.RunProgramStatement(children[currentIndex], ContinueAt);
        }        
    }


    

}
