using Weaver.Interfaces;
using Weaver.Messages;

namespace Lector
{
    public sealed class SampleExtension : ICommandExtension
    {
        public string Name => "sample"; // name of the extension
        public string Description => "A sample extension that wraps command execution.";
        public string Namespace => "system";

        /// <summary>
        /// Wraps the execution of the command. Can run before/after logic or inject feedback.
        /// </summary>
        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            // Before hook
            BeforeExecute(command, args);

            // Optionally, preview or manipulate the execution
            var result = executor(args);

            // After hook
            AfterExecute(command, result);

            // Optionally inject feedback or modify result
            if (command.Name.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                if (!result.RequiresConfirmation)
                {
                    result = new CommandResult
                    {
                        Message = $"[SampleExtension] Executed {command.Name}.",
                        RequiresConfirmation = false,
                        Success = result.Success
                    };
                }
            }

            return result;
        }

        public void BeforeExecute(ICommand command, string[]? args = null)
        {
            // Optional pre-processing
            Console.WriteLine($"[SampleExtension] Before executing {command.Name}");
        }

        public void AfterExecute(ICommand command, CommandResult result)
        {
            // Optional post-processing
            Console.WriteLine($"[SampleExtension] After executing {command.Name}: Success={result.Success}");
        }
    }
}