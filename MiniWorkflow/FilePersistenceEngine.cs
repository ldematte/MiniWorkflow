using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MiniWorkflow
{
    class FilePersistenceEngine: PersistenceEngine
    {

        internal override void Save(Guid programId, WorkflowStatus status)
        {
            using (var stream = File.OpenWrite(programId.ToString() + ".bin"))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, status);
            }
        }

        internal override WorkflowStatus Load(Guid programId)
        {
            using (var stream = File.OpenRead(programId.ToString() + ".bin"))
            {
                var formatter = new BinaryFormatter();
                return (WorkflowStatus)formatter.Deserialize(stream);
            }
        }
    }
}
