using NLog;
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
        [NonSerialized]
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        internal Action<WorkflowInstanceContext, object> Closed = null;

        public Activity()
        {
            this.name = this.GetType().Name;
        }

        private Activity parent = null;
        internal Activity Parent
        {
            get { return parent; }
            set
            {
                if (parent == null)
                    parent = value;
                else
                    throw new InvalidOperationException("This activity already has a parent");
            }
        }

        public bool IsRoot { get { return Parent == null; } }

        abstract protected internal ActivityExecutionStatus Execute(WorkflowInstanceContext context);

        protected readonly string name;
        public string Name
        {
            get { return name; }
        }

        // TODO: implement a setter that enforce the ExecutionStatus automaton
        internal ActivityExecutionStatus executionStatus = ActivityExecutionStatus.Initialized;
        public ActivityExecutionStatus ExecutionStatus 
        {
            get { return executionStatus; } 
        }
    }

    [Serializable]
    public abstract class CompositeActivity : Activity
    {
        protected List<Activity> children = new List<Activity>();
        public ICollection<Activity> Children
        {
            get { return children; }
        }

        public void Add(Activity activity)
        {
            activity.Parent = this;
            children.Add(activity);
        }
    }
}
