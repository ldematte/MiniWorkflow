using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWorkflow.Example;
using MiniWorkflow;

namespace MiniWorkflowTest
{
    [TestClass]
    public class ExecutionTests
    {
        [TestMethod]
        public void SimpleExecution()
        {
            var runtime = new WorkflowRuntime();
            var handle = runtime.RunProgram(SampleProgram.Create());

            // Simulate input reception
            string input = "400";                
            string bookmarkName = "ReadLine";
            handle.Resume(bookmarkName, input);            

            // Finished without problems            
        }

        [TestMethod]
        public void SuspendedExecution()
        {
            var runtime = new WorkflowRuntime();

            var handle = runtime.RunProgram(SampleProgram.Create());

            Guid programId = handle.ProgramId;

            // Save the workflow
            handle.Passivate();

            // Force GC
            handle = null;
            GC.Collect();

            // Simulate input reception
            string input = "400";
            string bookmarkName = "ReadLine";

            // Get back the handle we "lost"
            handle = runtime.GetProgramHandle(programId);
            handle.Resume(bookmarkName, input);
        }
    }
}
