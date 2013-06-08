using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MiniWorkflow
{
    [Serializable]
    public class WorkflowInstanceContext
    {
        [NonSerialized]
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, Bookmark> bookmarks = new Dictionary<string,Bookmark>();
        
        public void CreateBookmark(string name, Action<WorkflowInstanceContext, object> continuation)
        {
            bookmarks.Add(name, new Bookmark
                {
                    ContinueAt = continuation,
                    Name = name,
                    ActivityExecutionContext = this
                });
        }

        internal void ResumeBookmark(string bookmarkName, object payload)
        {
            var bookmark = bookmarks[bookmarkName];
            //TODO use queues, and deque an item, instead of having just one
            bookmarks.Remove(bookmarkName);
            bookmark.ContinueAt(this, payload);
        }

        internal void ExecuteActivity(Activity activity)
        {
            Debug.Assert(activity.ExecutionStatus == ActivityExecutionStatus.Initialized);
            activity.executionStatus = activity.Execute(this);

            if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                CloseActivity(activity);
            }
            // TODO: handle other conditions
        }

        internal void CloseActivity(Activity activity)
        {
            logger.Debug("Context::CloseActivity");

            // Someone just completed this activity.
            // Do we need to resume something?
            if (activity.Closed != null)
            {            
                logger.Debug("Context: resuming internal bookmark");
                activity.Closed(this, null);
            }
            activity.executionStatus = ActivityExecutionStatus.Closed;
        }

        // This method, called from an Activity, says: "the next statement to run is this. 
        // Run it, and when you are done, continue with this continuation (call me back there)"
        internal void RunProgramStatement(Activity activity, Action<WorkflowInstanceContext, object> continueAt)
        {
            logger.Debug("Context::RunProgramStatement");
            // This code replaces
            // context.Add(new Bookmark(activity.Name, ContinueAt));

            // Execute the a activity
            Debug.Assert(activity.ExecutionStatus == ActivityExecutionStatus.Initialized);
            activity.executionStatus = activity.Execute(this);
            
            // The activity already completed?
            if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                continueAt(this, null);
            }
            else
            {
                // Save for later...
                activity.Closed = continueAt;
            }
        }        
    }
}
