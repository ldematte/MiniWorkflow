using MiniWorkflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow.Example
{

    [Serializable]
    public class GenerateKey : Activity
    {
        public OutArgument<string> Key = new OutArgument<string>();

        protected override ActivityExecutionStatus Execute(WorkflowContext context)
        {
            // Print the key
            Key.Value = DateTime.Now.Millisecond.ToString();
            logger.Debug("Generated key: " + Key.Value);

            return ActivityExecutionStatus.Closed;
        }
    }

    [Serializable]
    public class CheckMatching : Activity
    {
        public InArgument<string> Key = new InArgument<string>();
        public InArgument<string> Input = new InArgument<string>();

        public OutArgument<bool> Result = new OutArgument<bool>();

        protected override ActivityExecutionStatus Execute(WorkflowContext context)
        {
            // Check key matching
            if (Key.Value.Equals(Input.Value))
                Result.Value = true;
            else
                Result.Value = false;

            return ActivityExecutionStatus.Closed;
        }
    }

    [Serializable]
    public class ReadLine : Activity
    {
        public OutArgument<string> Text = new OutArgument<string>();

        protected override ActivityExecutionStatus Execute(WorkflowContext context)
        {
            context.CreateBookmark(this.Name, this.ContinueAt);
            return ActivityExecutionStatus.Executing;
        }

        void ContinueAt(WorkflowContext context, object value)
        {
            this.Text.Value = (string)value;
            CloseActivity(context);
        }
    }
}
