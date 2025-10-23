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
        /// <inheritdoc />
        public string Namespace => "system";

        /// <inheritdoc />
        public string Name => "delete";

        /// <inheritdoc />
        public string Description => "Deletes a resource by name.";

        /// <inheritdoc />
        public int ParameterCount => 1;


        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["feedback"] = 1
        };


        /// <inheritdoc />
        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);


        /// <inheritdoc />
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

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            if (extensionName.Equals("feedback", StringComparison.OrdinalIgnoreCase))
            {
                var input = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "";

                return input switch
                {
                    "yes" => CommandResult.Ok("Resource deleted successfully."),
                    "no" => CommandResult.Fail("Deletion cancelled by user."),
                    "cancel" => CommandResult.Fail("Deletion cancelled by user."),
                    _ => new CommandResult
                    {
                        Message = $"Unrecognized response '{input}'. Please answer yes/no.",
                        RequiresConfirmation = true,
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