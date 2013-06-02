using NLog;
using System;
using System.Collections.Generic;
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
        private Bookmark InternalBookmark = null;

        public void CreateBookmark(string name, Action<WorkflowInstanceContext, object> continuation)
        {
            bookmarks.Add(name, new Bookmark
                {
                    ContinueAt = continuation,
                    Name = name,
                    ActivityExecutionContext = this
                });
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
        }

        // This method, called from an Activity, says: "the next statement to run is this. 
        // Run it, and when you are done, continue with this continuation (call me back there)"
        internal void RunProgramStatement(Activity activity, Action<WorkflowInstanceContext, object> continueAt)
        {
            logger.Debug("Context::RunProgramStatement");
            // This code replaces
            // context.Add(new Bookmark(activity.Name, ContinueAt));
            
            var result = activity.Execute(this);
            // The activity already completed?
            if (result == ActivityExecutionStatus.Closed)
                continueAt(this, null);
            else
            {
                // Save for later...
                InternalBookmark = new Bookmark
                {
                    ContinueAt = continueAt,
                    Name = "",
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
