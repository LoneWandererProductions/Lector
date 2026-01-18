/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Core
 * FILE:        DeleteCommand.cs
 * PURPOSE:     Test command that deletes a resource with confirmation via feedback.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Test command that deletes a resource with confirmation via feedback.
    /// </summary>
    /// <seealso cref="ICommand" />
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
        public IReadOnlyDictionary<string, int>? Extensions => new Dictionary<string, int>
        {
            { "test", 1 } // "test"  does not exist extension but expects at least 1 parameter (or variable)
        };


        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        /// 
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Missing target file.");

            var target = args[0];

            // Build a recursive feedback request
            FeedbackRequest? feedback = null;
            feedback = new FeedbackRequest(
                prompt: $"Delete '{target}'? (yes/no/cancel)",
                options: new[] { "yes", "no", "cancel" },
                onRespond: input =>
                {
                    switch (input.Trim().ToLowerInvariant())
                    {
                        case "yes": return CommandResult.Ok($"Resource '{target}' deleted successfully.");
                        case "no":
                        case "cancel": return CommandResult.Fail("Deletion cancelled by user.");
                        default:
                            // Return a new CommandResult with the same Feedback to re-prompt
                            return new CommandResult
                            {
                                Message = $"Unrecognized response '{input}'. Please answer yes/no/cancel.",
                                RequiresConfirmation = true,
                                Feedback = feedback
                            };
                    }
                });

            return new CommandResult
            {
                Message = $"Are you sure you want to delete '{target}'?",
                RequiresConfirmation = true,
                Feedback = feedback
            };
        }

        /// <inheritdoc />
        public CommandResult TryRun(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Missing target for preview.");

            var target = args[0];

            FeedbackRequest? feedback = null;
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
                            Message = $"Unrecognized response '{input}'. Please answer yes/no/cancel.",
                            RequiresConfirmation = true,
                            Feedback = feedback
                        }
                    };
                });

            return new CommandResult
            {
                Message = $"Preview: Are you sure you want to delete '{target}'?",
                RequiresConfirmation = true,
                Feedback = feedback,
                Success = false
            };
        }
    }
}