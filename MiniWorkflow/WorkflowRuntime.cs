﻿using System;
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
        private readonly WorkflowRuntime runtime;
        internal WorkflowHandle(WorkflowStatus status, WorkflowRuntime runtime)
        {
            this.status = status;
            this.programId = Guid.NewGuid();
            this.runtime = runtime;
        }

        internal WorkflowHandle(Guid programId, WorkflowRuntime runtime)
        {
            this.status = null;
            this.programId = programId;
            this.runtime = runtime;
        }

        private WorkflowStatus status;

        // Unique identifier for this program
        private readonly Guid programId;
        public Guid ProgramId { get { return programId; } }

        internal bool IsPassivated { get { return status == null; } }

        // Passivate the program
        public void Passivate()
        {
            Persist();
            status = null;
        }

        public void Persist()
        {
            runtime.ExecutionQueue.Pause();
            // Persisting means: saving the status
            runtime.PersistenceEngine.Save(programId, status);
            runtime.ExecutionQueue.Resume();
        }

        // Resume a bookmarked program
        public void Resume(string bookmarkName, object payload)
        {
            // Check that it is not passivated..
            if (IsPassivated)
            {
                runtime.PersistenceEngine.Load(programId);
            }

            var context = new WorkflowContext(status, runtime);
            context.ResumeBookmark(bookmarkName, payload);
        }

        public void Start(Activity program)
        {
            if (!program.IsRoot)
                throw new InvalidOperationException("You can only start a root activity");
            var context = new WorkflowContext(status, runtime);            
            context.ExecuteActivity(program);
        }
    }

    public class WorkflowRuntime: IDisposable
    {
        private readonly Dictionary<Guid, WorkflowHandle> memoryWorkflows = new Dictionary<Guid, WorkflowHandle>();

        // TODO: make both execution queue and persitence pluggable
        private readonly ExecutionQueue executionQueue = new ExecutionQueue();
        private readonly PersistenceEngine persistenceEngine = new FilePersistenceEngine();

        internal PersistenceEngine PersistenceEngine { get { return persistenceEngine; } }
        internal ExecutionQueue ExecutionQueue { get { return executionQueue; } }

        public WorkflowRuntime()
        {
            executionQueue.Start();
        }

        public void Dispose()
        {
            executionQueue.Stop();
        }

        // Starts a new program
        public WorkflowHandle CreateProgram()
        {
            var status = new WorkflowStatus();
            var handle = new WorkflowHandle(status, this);
            memoryWorkflows.Add(handle.ProgramId, handle);
            
            return handle;
        }

        // Returns a handle to a previously started program
        public WorkflowHandle GetProgramHandle(Guid programId)
        {
            WorkflowHandle handle;
            if (!memoryWorkflows.ContainsKey(programId))            
               handle = new WorkflowHandle(programId, this);            
            else
               handle =  memoryWorkflows[programId];
            return handle;
        }

        public void Post(Action action)
        {
            executionQueue.Post(action);
        }

        // Passivates all in-memory programs
        public void Shutdown()
        {
            executionQueue.Stop();
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
