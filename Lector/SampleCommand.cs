using System;
using System.Collections.Generic;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Lector
{

    /// <summary>
    /// Template command for the Weaver engine.
    /// Implements ICommand with optional extensions and preview support.
    /// </summary>
    public sealed class SampleCommand : ICommand
    {
        public string Namespace => "system";          // Example namespace
        public string Name => "sample";               // Command name
        public string Description => "A sample command demonstrating extension and preview usage.";
        public int ParameterCount => 1;               // Expected number of arguments
        public IReadOnlyDictionary<string, int>? Extensions => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["feedback"] = 1  // Optional built-in extension for feedback loop
        };

        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        /// <summary>
        /// Normal execution of the command.
        /// </summary>
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("No argument provided.");

            var target = args[0];

            // For demonstration: require user confirmation via feedback
            return new CommandResult
            {
                Message = $"Do you want to process '{target}'?",
                Feedback = new FeedbackRequest
                {
                    Prompt = $"Process '{target}'? (yes/no)",
                    Options = new[] { "yes", "no" }
                },
                RequiresConfirmation = true
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
        /// Optional extension calls (e.g., feedback, custom extensions)
        /// </summary>
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            if (extensionName.Equals("feedback", StringComparison.OrdinalIgnoreCase))
            {
                var input = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "";

                return input switch
                {
                    "yes" => CommandResult.Ok("Action completed successfully."),
                    "no" => CommandResult.Fail("Action cancelled by user."),
                    _ => new CommandResult
                    {
                        Message = $"Unrecognized response '{input}'. Please answer yes/no.",
                        Feedback = new FeedbackRequest
                        {
                            Prompt = "Please answer: yes / no",
                            Options = new[] { "yes", "no" }
                        },
                        RequiresConfirmation = true
                    }
                };
            }

            return CommandResult.Fail($"Unknown extension '{extensionName}'.");
        }
    }
}