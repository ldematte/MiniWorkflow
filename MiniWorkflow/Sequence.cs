using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    [Serializable]
    public class Sequence : Activity
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        int currentIndex;
        List<Activity> statements = new List<Activity>();
        public IList<Activity> Statements
        {
            get { return statements; }
        }

        protected internal override ActivityExecutionStatus Execute(WorkflowInstanceContext context)
        {
            logger.Debug("Sequence::Execute");
            currentIndex = 0;
            // Empty statement block
            if (statements.Count == 0)
            {
                context.CloseActivity();
                return ActivityExecutionStatus.Closed;
            }
            else
            {
                context.RunProgramStatement(statements[0], ContinueAt);
                return ActivityExecutionStatus.Executing;
            }
        }

        public void ContinueAt(WorkflowInstanceContext context, object value)
        {
            logger.Debug("Sequence::ContinueAt");
            // If we've run all the statements, we're done
            if (++currentIndex == statements.Count) 
                context.CloseActivity();
            else // Else, run the next statement
                context.RunProgramStatement(statements[currentIndex], ContinueAt);
        }        
    }
}
