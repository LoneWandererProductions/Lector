using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator
{

    /// <summary>
    /// Test extension that appends "[EXT]" to the command message
    /// </summary>
    public sealed class AppendExtension : ICommandExtension
    {
        public string Name => "append";

        public string Description => "Test";

        public string? Namespace => "Test";

        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            // Execute the command normally
            var result = executor(args);

            // Append a note
            return new CommandResult
            {
                Message = $"{result.Message} [EXT]",
                Success = result.Success,
                Feedback = result.Feedback,
                RequiresConfirmation = result.RequiresConfirmation
            };
        }
    }
}