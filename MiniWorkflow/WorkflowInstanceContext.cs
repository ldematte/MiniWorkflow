using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MiniWorkflow
{
    [Serializable]
    internal class WorkflowStatus
    {
        private readonly Dictionary<string, Bookmark> bookmarks = new Dictionary<string, Bookmark>();

        internal void AddBookmark(string name, Bookmark bookmark)
        {
            bookmarks.Add(name, bookmark);
        }

        //TODO use queues, and deque an item, instead of having just one
        internal Bookmark Get(string bookmarkName)
        {
            var bookmark = bookmarks[bookmarkName];
            bookmarks.Remove(bookmarkName);
            return bookmark;
        }
    }

    [Serializable]
    public class WorkflowContext
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

       
        private readonly WorkflowRuntime runtime;

        private readonly WorkflowStatus status;
        internal WorkflowContext(WorkflowStatus status, WorkflowRuntime runtime)
        {
            this.status = status;
            this.runtime = runtime;
        }

        public void CreateBookmark(string name, Action<WorkflowContext, object> continuation)
        {
            status.AddBookmark(name, new Bookmark
                {
                    ContinueAt = continuation,
                    Name = name,
                    ActivityExecutionContext = this
                });
        }

        internal void ResumeBookmark(string bookmarkName, object payload)
        {
            var bookmark = status.Get(bookmarkName);
            runtime.Post(() => bookmark.ContinueAt(this, payload));
        }

        internal void ExecuteActivity(Activity activity)
        {
            runtime.Post(() =>
            {
                logger.Debug("Executing " + activity.GetType().Name);
                Debug.Assert(activity.ExecutionStatus == ActivityExecutionStatus.Initialized);
                activity.executionStatus = activity.Execute(this);

                if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    CloseActivity(activity);
                }
                // TODO: handle other conditions
            });
        }

        internal void CloseActivity(Activity activity)
        {
            logger.Debug("Context::CloseActivity");

            logger.Debug("Closing " + activity.GetType().Name);

            // Someone just completed this activity.
            // Do we need to resume something?
            if (activity.Closed != null)
            {            
                logger.Debug("Context: resuming internal bookmark");
                runtime.Post(() => activity.Closed(this, null));
            }
            activity.executionStatus = ActivityExecutionStatus.Closed;
        }

        // This method, called from an Activity, says: "the next statement to run is this. 
        // Run it, and when you are done, continue with this continuation (call me back there)"
        internal void RunProgramStatement(Activity activity, Action<WorkflowContext, object> continueAt)
        {
            logger.Debug("Context::RunProgramStatement");

            // Add the "bookmark"
            activity.Closed = continueAt;

            // Execute the a activity
            Debug.Assert(activity.ExecutionStatus == ActivityExecutionStatus.Initialized);
            ExecuteActivity(activity);
        }        
    }
}
