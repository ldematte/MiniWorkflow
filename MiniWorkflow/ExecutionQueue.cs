using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    public class ExecutionQueue
    {
        private readonly BlockingCollection<Action> workItemQueue = new BlockingCollection<Action>();

        public void Post(Action workItem)
        {
            workItemQueue.Add(workItem);
        }

        private Thread thread = null;

        public void Stop()
        {
            workItemQueue.CompleteAdding();
            thread.Join();
            thread = null;
        }

        public void Start()
        {
            if (thread != null)
                throw new InvalidOperationException("Already started");

            thread = new Thread(() =>
            {
                while (!workItemQueue.IsCompleted)
                {

                    Action workItem = null;
                    // Blocks if number.Count == 0 
                    // IOE means that Take() was called on a completed collection. 
                    // Some other thread can call CompleteAdding after we pass the 
                    // IsCompleted check but before we call Take.  
                    // Here we can simply catch the exception since the  
                    // loop will break on the next iteration. 
                    try
                    {
                        workItem = workItemQueue.Take();
                    }
                    catch (InvalidOperationException) { }

                    if (workItem != null)
                    {
                        workItem.Invoke();
                    }
                }
            });

            thread.Start();
        }
    }
}
