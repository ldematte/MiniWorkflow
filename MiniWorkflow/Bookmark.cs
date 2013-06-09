using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    [Serializable]
    public class Bookmark
    {
        public string Name { get; set; }

        public Activity Activity { get; set; }
        public Action<WorkflowContext, object> ContinueAt { get; set; }

        public object Payload { get; set; }

        public WorkflowContext ActivityExecutionContext { get; set; }
    }
}
