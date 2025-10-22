using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator
{
    public sealed class DeleteCommand : ICommand
    {
        public string Namespace => "system";
        public string Name => "delete";
        public string Description => "Deletes a resource by name.";
        public int ParameterCount => 1;
        public IReadOnlyDictionary<string, int>? Extensions => null;

        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        public CommandResult Execute(params string[] args)
        {
            var target = args[0];
            return new CommandResult
            {
                Message = $"Are you sure you want to delete '{target}'?",
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
                var input = (args.Length > 0) ? args[0].Trim().ToLowerInvariant() : "";

                return input switch
                {
                    "yes" => CommandResult.Ok("Resource deleted successfully."),
                    "no" or "cancel" => CommandResult.Fail("Deletion cancelled by user."),
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

            return CommandResult.Fail($"Unknown extension '{extensionName}'.");
        }
    }
}
