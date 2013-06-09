
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiniWorkflow;
using NLog;

namespace MiniWorkflow.Example
{
    public class SampleProgram
    {
        public static Activity Create()
        {
            var openSesameProgram = new Sequence();
            var printKey = new PrintKey();
            var read = new ReadLine();
            var printGreeting = new PrintGreeting();

            WorkflowRuntime.Bind(printKey.Key, printGreeting.Key);
            WorkflowRuntime.Bind(read.Text, printGreeting.Input);

            openSesameProgram.Add(printKey);
            openSesameProgram.Add(read);
            openSesameProgram.Add(printGreeting);

            return openSesameProgram;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LogManager.ThrowExceptions = true;
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("Starting app");

            var runtime = new WorkflowRuntime();

            var handle = runtime.CreateProgram();
            Guid programId = handle.ProgramId;

            handle.Start(SampleProgram.Create());

            while (true)
            {
                // Save the workflow
                handle.Passivate();

                // Force GC
                handle = null;
                GC.Collect();

                // receive some input
                string input = Console.ReadLine();
                // determine from the input which program to use
                // ...
                // determine from the input which bookmark to resume
                string bookmarkName = "ReadLine";

                handle = runtime.GetProgramHandle(programId);
                handle.Resume(bookmarkName, input);
            }
        }
    }
}
