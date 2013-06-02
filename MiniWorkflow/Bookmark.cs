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

        public Action<WorkflowInstanceContext, object> ContinueAt { get; set; }

        public object Payload { get; set; }
        
        public WorkflowInstanceContext ActivityExecutionContext { get; set; }
    }
}
