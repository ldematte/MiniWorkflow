using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    public class WorkflowHandle
    {
        internal WorkflowHandle(WorkflowInstanceContext context)
        {
            this.context = context;
            this.programId = Guid.NewGuid();
        }

        internal WorkflowHandle(Guid programId)
        {
            this.context = null;
            this.programId = programId;
        }

        public WorkflowInstanceContext context { get; private set; }        

        // Unique identifier for this program
        private readonly Guid programId;
        public Guid ProgramId { get { return programId; } }

        internal bool IsPassivated { get { return context == null; } }

        // Passivate the program
        public void Passivate()
        {
            Persist();
            context = null;
        }

        public void Persist()
        {
            // Persisting means:
            // Saving the context
            // TODO: separate this concern
            using (var stream = File.OpenWrite(programId.ToString() + ".bin"))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, context);
            }
        }

        // Resume a bookmarked program
        public void Resume(string bookmarkName, object payload)
        {
            // Check that it is not passivated..
            if (IsPassivated)
            {
                // TODO: separate this concern
                using (var stream = File.OpenRead(programId.ToString() + ".bin"))
                {
                    var formatter = new BinaryFormatter();
                    context = (WorkflowInstanceContext)formatter.Deserialize(stream);
                }
            }

            context.ResumeBookmark(bookmarkName, payload);
        }
    }

    public class WorkflowRuntime
    {
        private readonly Dictionary<Guid, WorkflowHandle> memoryWorkflows = new Dictionary<Guid, WorkflowHandle>();

        // Starts a new program
        public WorkflowHandle RunProgram(Activity program)
        {
            var context = new WorkflowInstanceContext();            
            // Q: should a context be completely independent?
            // but the queue(s) in the context should be serialized anyway...
            var handle = new WorkflowHandle(context);
            memoryWorkflows.Add(handle.ProgramId, handle);

            // TODO: Separate creation and execution.
            // Consider introducing an execution queue?
            context.ExecuteActivity(program);
            
            return handle;
        }

        // Returns a handle to a previously started program
        public WorkflowHandle GetProgramHandle(Guid programId)
        {
            WorkflowHandle handle;
            if (!memoryWorkflows.ContainsKey(programId))            
               handle = new WorkflowHandle(programId);            
            else
               handle =  memoryWorkflows[programId];
            return handle;
        }

        // Passivates all in-memory programs
        public void Shutdown()
        {
            foreach (var transientWorkflow in memoryWorkflows.Values)
                transientWorkflow.Passivate();
        }

        // Helper method to bind in/out arguments of two activities (very naive, ATM)
        public static void Bind<T>(OutArgument<T> from, InArgument<T> to)
        {
            to.LinkedArg = from;
        }
    }
}
