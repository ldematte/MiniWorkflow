using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniWorkflow
{
    abstract class PersistenceEngine
    {
        internal abstract void Save(Guid programId, WorkflowStatus status);
        internal abstract WorkflowStatus Load(Guid programId);
    }
}
