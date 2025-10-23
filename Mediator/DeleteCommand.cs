/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     UnknownNamespace
 * FILE:        DeleteCommand.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

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
        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        public IReadOnlyDictionary<string, int>? Extensions => null; // No need for 'feedback' extension now

        /// <summary>
        /// Executes the delete command.
        /// </summary>
        public CommandResult Execute(params string[] args)
        {
            var target = args[0];

            // Build a feedback request using IFeedback
            var feedback = new FeedbackRequest(
                prompt: $"Delete '{target}'? (yes/no/cancel)",
                options: new[] { "yes", "no", "cancel" },
                onRespond: input =>
                {
                    switch (input.Trim().ToLowerInvariant())
                    {
                        case "yes":
                            return CommandResult.Ok($"Resource '{target}' deleted successfully.");
                        case "no":
                        case "cancel":
                            return CommandResult.Fail("Deletion cancelled by user.");
                        default:
                            // Keep feedback pending for invalid input
                            return new CommandResult
                            {
                                Message = $"Unrecognized response '{input}'. Please answer yes/no/cancel.",
                                RequiresConfirmation = true,
                                Feedback = new FeedbackRequest(
                                    prompt: "Please answer: yes / no / cancel",
                                    options: new[] { "yes", "no", "cancel" },
                                    onRespond: s => throw new NotImplementedException() // recursive wrapping handled by Weave
                                )
                            };
                    }
                }
            );

            return new CommandResult
            {
                Message = $"Are you sure you want to delete '{target}'?",
                RequiresConfirmation = true,
                Feedback = feedback
            };
        }

        /// <summary>
        /// No longer needed: all feedback is handled via IFeedback.
        /// </summary>
        // In DeleteCommand
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            if (!extensionName.Equals("feedback", StringComparison.OrdinalIgnoreCase))
                return CommandResult.Fail($"Unknown extension '{extensionName}'.");

            if (args.Length == 0)
                return new CommandResult
                {
                    Message = "No input provided for feedback.",
                    RequiresConfirmation = true
                };

            var input = args[0].Trim().ToLowerInvariant();

            return input switch
            {
                "yes" => CommandResult.Ok("Resource deleted successfully."),
                "no" => CommandResult.Fail("Deletion cancelled by user."),
                "cancel" => CommandResult.Fail("Deletion cancelled by user."),
                _ => new CommandResult
                {
                    Message = $"Unrecognized response '{input}'. Please answer yes/no/cancel.",
                    RequiresConfirmation = true,
                    Feedback = new FeedbackRequest(
                        prompt: "Please answer: yes / no / cancel",
                        options: new[] { "yes", "no", "cancel" },
                        onRespond: null!)
                }
            };
        }
    }
}