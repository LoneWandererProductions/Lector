/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     UnknownNamespace
 * FILE:        DeleteCommand.cs
 * PURPOSE:     Test command that deletes a resource with confirmation via feedback.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator
{
    /// <inheritdoc />
    /// <summary>
    /// Test command that deletes a resource with confirmation via feedback.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class DeleteCommand : ICommand
    {
        /// <inheritdoc />
        public string Namespace => "Test";

        /// <inheritdoc />
        public string Name => "delete";

        /// <inheritdoc />
        public string Description => "Deletes a resource by name.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
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
                                    onRespond: s =>
                                        throw new NotImplementedException() // recursive wrapping handled by Weave
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

        /// <inheritdoc />
        /// <summary>
        /// No longer needed: all feedback is handled via IFeedback.
        /// </summary>
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return extensionName.ToLowerInvariant() switch
            {
                "tryrun" => HandleTryRun(args),
                "feedback" => HandleFeedback(args),
                _ => CommandResult.Fail($"Unknown extension '{extensionName}'.")
            };
        }

        /// <summary>
        /// Handles the try run.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The CommandResult</returns>
        private CommandResult HandleTryRun(string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Missing argument: target file.");

            var target = args[0];

            FeedbackRequest? feedback = null;
            var cache = feedback;

            feedback = new FeedbackRequest(
                prompt: $"Preview: Are you sure you want to delete '{target}'? (yes/no/cancel)",
                options: new[] { "yes", "no", "cancel" },
                onRespond: input =>
                {
                    input = input.Trim().ToLowerInvariant();
                    return input switch
                    {
                        "yes" => Execute(target),
                        "no" => CommandResult.Fail("Deletion cancelled by user."),
                        "cancel" => CommandResult.Fail("Deletion cancelled by user."),
                        _ => new CommandResult
                        {
                            Message = "Please answer yes/no/cancel",
                            RequiresConfirmation = true,
                            Feedback = cache
                        }
                    };
                });

            return new CommandResult
            {
                Message = $"Are you sure you want to delete '{target}'?",
                Feedback = feedback,
                RequiresConfirmation = true,
                Success = true
            };
        }

        /// <summary>
        /// Handles the feedback.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The CommandResult</returns>
        private CommandResult HandleFeedback(string[] args)
        {
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