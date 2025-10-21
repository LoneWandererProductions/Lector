using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator
{
    public sealed class DeleteCommand : ICommand
    {
        public string Namespace => "system"; // or any other logical group you like
        public string Name => "delete";
        public string Description => "Deletes a resource by name.";
        public int ParameterCount => 1;
        public int ExtensionParameterCount => 1;

        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        public IReadOnlyDictionary<string, int>? Extensions => null;

        public CommandResult Execute(params string[] args)
        {
            var target = args[0];
            return new CommandResult
            {
                Message = $"Are you sure you want to delete '{target}'?",
                RequiresConfirmation = true,
                Feedback = new FeedbackRequest
                {
                    Prompt = $"Delete '{target}'? (yes/no/cancel)",
                    Options = new[] { "yes", "no", "cancel" }
                }
            };
        }

        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            if (string.Equals(extensionName, "feedback", StringComparison.OrdinalIgnoreCase))
            {
                var input = (args.Length > 0) ? args[0].Trim() : "";

                return input.ToLowerInvariant() switch
                {
                    "yes" => CommandResult.Ok("Resource deleted successfully."),
                    "no" => CommandResult.Fail("Deletion cancelled by user."),
                    "cancel" => CommandResult.Fail("Operation cancelled."),
                    _ => new CommandResult
                    {
                        Message = $"Unrecognized response '{input}'. Please answer yes/no/cancel.",
                        Feedback = new FeedbackRequest
                        {
                            Prompt = "Please answer: yes / no / cancel",
                            Options = new[] { "yes", "no", "cancel" }
                        }
                    }
                };
            }

            if (string.Equals(extensionName, "help", StringComparison.OrdinalIgnoreCase))
                return CommandResult.Ok("Usage: delete(name) — deletes the resource by name.");

            return CommandResult.Fail($"Unknown extension '{extensionName}'.");
        }
    }
}
