using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    [Serializable]
    public abstract class Activity
    {
        public Activity()
        {
            this.name = this.GetType().Name;
        }

        abstract protected internal ActivityExecutionStatus Execute(WorkflowInstanceContext context);

        protected readonly string name;
        public string Name
        {
            get { return name; }
        }
    }
}
