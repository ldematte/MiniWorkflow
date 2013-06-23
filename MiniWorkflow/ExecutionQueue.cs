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
        private readonly ConcurrentQueue<Action> workItemQueue = new ConcurrentQueue<Action>();

        public void Post(Action workItem)
        {
            workItemQueue.Enqueue(workItem);
            items.Release();
        }

        private Thread thread = null;
        
        private ManualResetEvent finished = new ManualResetEvent(false);
        private Semaphore items = new Semaphore(0, 32);

        //private Object executionLock = new Object();
        SpinLock executionLock = new SpinLock();

        public void Pause()
        {            
            //Monitor.Enter(executionLock);
            var lockTaken = false;
            while (!lockTaken)
                executionLock.Enter(ref lockTaken);
        }

        public void Resume()
        {
            executionLock.Exit();
        }        

        public void Stop()
        {
            finished.Set();
            thread.Join();
            thread = null;
        }

        public void Start()
        {
            if (thread != null)
                throw new InvalidOperationException("Already started");

            thread = new Thread(() =>
            {
                while (true) {
                    int reason = WaitHandle.WaitAny(new WaitHandle[] { finished, items });

                    if (reason == 0)
                        break;
                    
                    var lockTaken = false;
                    // Block "executing", to make other threads to wait on items execution...
                    try
                    {
                        while (!lockTaken)
                            executionLock.Enter(ref lockTaken);

                        Action workItem = null;
                        if (workItemQueue.TryDequeue(out workItem))
                            workItem.Invoke();
                    }
                    finally
                    {
                        if (lockTaken)
                            executionLock.Exit();
                    }
                }
            });

            thread.Start();
        }
    }
}
