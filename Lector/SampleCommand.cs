/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Lector
 * FILE:        SampleCommand.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Lector
{
    /// <summary>
    /// Template command for the Weaver engine.
    /// Implements ICommand with optional preview support.
    /// </summary>
    public sealed class SampleCommand : ICommand
    {
        public string Namespace => "system";
        public string Name => "sample";
        public string Description => "A sample command demonstrating feedback and preview usage.";
        public int ParameterCount => 1;
        public IReadOnlyDictionary<string, int>? Extensions => null; // no manual feedback extension needed
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// Normal execution of the command.
        /// Uses FeedbackRequest for confirmation.
        /// </summary>
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("No argument provided.");

            var target = args[0];

            var feedback = new FeedbackRequest(
                prompt: $"Process '{target}'? (yes/no)",
                options: new[] { "yes", "no" },
                onRespond: input =>
                {
                    return input.Trim().ToLowerInvariant() switch
                    {
                        "yes" => CommandResult.Ok($"'{target}' processed successfully."),
                        "no" => CommandResult.Fail($"Processing of '{target}' cancelled by user."),
                        _ => new CommandResult
                        {
                            Message = $"Unrecognized response '{input}'. Please answer yes/no.",
                            RequiresConfirmation = true,
                            Feedback = new FeedbackRequest(
                                prompt: "Please answer: yes / no",
                                options: new[] { "yes", "no" },
                                onRespond: s => throw new NotImplementedException() // recursive handled by Weave
                            )
                        }
                    };
                });

            return new CommandResult
            {
                Message = $"Do you want to process '{target}'?",
                RequiresConfirmation = true,
                Feedback = feedback
            };
        }

        /// <summary>
        /// Optional preview mode called by extensions like .tryrun()
        /// </summary>
        public CommandResult? TryRun(params string[] args)
        {
            if (args.Length == 0)
                return null;

            var target = args[0];
            return new CommandResult
            {
                Message = $"[Preview] This would process '{target}'",
                Success = true
            };
        }

        /// <summary>
        /// No extension handling required: all feedback handled via IFeedback.
        /// </summary>
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail($"Unknown extension '{extensionName}'.");
        }
    }
}