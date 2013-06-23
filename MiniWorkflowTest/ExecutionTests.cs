using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniWorkflow.Example;
using MiniWorkflow;
using System.Threading;

namespace MiniWorkflowTest
{
    [TestClass]
    public class ExecutionTests
    {
        [TestMethod]
        public void SimpleExecution()
        {
            var runtime = new WorkflowRuntime();
            var handle = runtime.CreateProgram();
            handle.Start(SampleProgram.Create());

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

            var handle = runtime.CreateProgram();
            handle.Start(SampleProgram.Create());

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

        [TestMethod]
        public void ParallelExecution()
        {
            var runtime = new WorkflowRuntime();

            var handle = runtime.CreateProgram();

            var workflow = new Parallel();
            var sequence1 = new Sequence();
            var generate1 = new GenerateKey();
            var read1 = new ReadLine();
            var check1 = new CheckMatching();
            sequence1.Add(generate1);
            sequence1.Add(read1);
            sequence1.Add(check1);

            var sequence2 = new Sequence();
            var generate2 = new GenerateKey();
            var read2 = new ReadLine();
            var check2 = new CheckMatching();
            sequence2.Add(generate2);
            sequence2.Add(read2);
            sequence2.Add(check2);

            workflow.Add(sequence1);
            workflow.Add(sequence2);

            handle.Start(workflow);

            Guid programId = handle.ProgramId;

            // Wait a little to arrive to the bookmark(s)
            Thread.Sleep(500);
            // Save the workflow
            handle.Passivate();

            // Force GC
            handle = null;
            GC.Collect();           

            // Get back the handle we "lost"
            handle = runtime.GetProgramHandle(programId);

            // Simulate input reception
            string input = generate1.Key.Value;
            string bookmarkName = "ReadLine";

            handle.Resume(bookmarkName, input);


        }
    }
}
