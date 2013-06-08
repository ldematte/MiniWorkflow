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
                    Activity = currentExecutingActivity,
                    ActivityExecutionContext = this
                });
        }

        private Bookmark InternalBookmark = null;

        private Activity currentExecutingActivity = null;

        internal void ExecuteActivity(Activity activity)
        {
            Debug.Assert(activity.ExecutionStatus == ActivityExecutionStatus.Initialized);
            currentExecutingActivity = activity;
            activity.executionStatus = activity.Execute(this);

            if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                CloseActivity();
            }
            // TODO: handle other conditions
        }

        public void CloseActivity()
        {
            logger.Debug("Context::CloseActivity");
            // Someone just completed an activity.
            // Do we need to resume something?
            if (InternalBookmark != null)
            {
                logger.Debug("Context: resuming internal bookmark");
                var continuation = InternalBookmark.ContinueAt;
                var context = InternalBookmark.ActivityExecutionContext;
                var value = InternalBookmark.Payload;
                InternalBookmark = null;
                continuation(context, value);
            }

            if (currentExecutingActivity != null)
            {
                currentExecutingActivity.executionStatus = ActivityExecutionStatus.Closed;
                currentExecutingActivity = null;
            }
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
            currentExecutingActivity = activity;
            activity.executionStatus = activity.Execute(this);
            
            // The activity already completed?
            if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                continueAt(this, null);
                currentExecutingActivity = null;
            }
            else
            {
                // Save for later...
                InternalBookmark = new Bookmark
                {
                    ContinueAt = continueAt,
                    Name = "",
                    Activity = activity,
                    ActivityExecutionContext = this
                };
            }
        }

        internal void ResumeBookmark(string bookmarkName, object payload)
        {            
            var bookmark = bookmarks[bookmarkName];
            //TODO use queues, and deque an item, instead of having just one
            bookmarks.Remove(bookmarkName);
            bookmark.ContinueAt(this, payload);
        }

        
    }
}
