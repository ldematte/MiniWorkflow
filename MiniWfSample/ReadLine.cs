using MiniWorkflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow.Example
{

    [Serializable]
    public class PrintKey : Activity
    {
        public OutArgument<string> Key = new OutArgument<string>();

        protected override ActivityExecutionStatus Execute(WorkflowContext context)
        {
            // Print the key
            Key.Value = DateTime.Now.Millisecond.ToString();
            Console.WriteLine("here is your key: " + Key.Value);

            return ActivityExecutionStatus.Closed;
        }
    }

    [Serializable]
    public class PrintGreeting : Activity
    {
        public InArgument<string> Key = new InArgument<string>();
        public InArgument<string> Input = new InArgument<string>();

        protected override ActivityExecutionStatus Execute(WorkflowContext context)
        {
            // Print the greeting if the key is provided
            if (Key.Value.Equals(Input.Value))
                Console.WriteLine("hello, world");
            else
                Console.WriteLine("goodbye, world");

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
