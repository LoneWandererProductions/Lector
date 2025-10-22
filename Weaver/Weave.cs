/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        Weave.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ParseEngine;

namespace Weaver
{
    /// <summary>
    /// Represents a unique command signature consisting of namespace, name, and parameter count.
    /// </summary>
    /// <param name="Namespace">The namespace of the command.</param>
    /// <param name="Name">The name of the command.</param>
    /// <param name="ParameterCount">The number of parameters the command expects.</param>
    public record CommandSignature(string Namespace, string Name, int ParameterCount);

    /// <summary>
    /// Core command execution engine.
    /// Manages registration, lookup, execution, and feedback for commands with optional extensions.
    /// </summary>
    public sealed class Weave
    {
        // Stores pending feedback continuations, keyed by request ID.
        private readonly Dictionary<string, Func<string, CommandResult>> _continuations
            = new(StringComparer.OrdinalIgnoreCase);

        // Stores registered commands, keyed by (namespace, name, parameter count).
        private readonly Dictionary<(string ns, string name, int paramCount), ICommand> _commands
            = new();

        // Stores per-command extension definitions, keyed by (namespace, name, parameter count).
        private readonly Dictionary<(string ns, string name, int paramCount), Dictionary<string, int>> _commandExtensions
            = new();

        // Built-in extensions injected into every command.
        private static readonly Dictionary<string, CommandExtension> _globalExtensions
            = new(StringComparer.OrdinalIgnoreCase)
            {
                ["help"] = new CommandExtension { Name = "help", ParameterCount = 0, IsInternal = true },
                ["tryrun"] = new CommandExtension { Name = "tryrun", ParameterCount = 0, IsInternal = true, IsPreview = true }
            };

        private ICommand? _pendingFeedbackCommand;
        private FeedbackRequest? _pendingFeedback;

        public Weave()
        {
            // Register internal programs at startup
            // 'help' is both a command and an extension
            var help = new Core.HelpCommand(GetCommands());
            Register(help);  // only called once here
        }

        /// <summary>
        /// Registers a command in the engine.
        /// Combines command-defined extensions with global extensions for internal use.
        /// </summary>
        /// <param name="command">The command to register.</param>
        public void Register(ICommand command)
        {
            // Build a merged extension dictionary (command + global)
            var mergedExtensions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Include command-defined extensions
            if (command.Extensions != null)
            {
                foreach (var kv in command.Extensions)
                    mergedExtensions[kv.Key] = kv.Value;
            }

            // Include global extensions, skipping duplicates
            foreach (var kv in _globalExtensions)
                if (!mergedExtensions.ContainsKey(kv.Key))
                    mergedExtensions[kv.Key] = kv.Value.ParameterCount;

            // Store extensions for internal lookup during execution
            _commandExtensions[(command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)]
                = mergedExtensions;

            // Store the command itself for execution
            _commands[(command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)] = command;
        }

        /// <summary>
        /// Retrieves all registered commands, optionally filtered by namespace.
        /// </summary>
        /// <param name="ns">Optional namespace filter; if null, returns all commands.</param>
        /// <returns>An enumerable of commands matching the filter.</returns>
        public IEnumerable<ICommand> GetCommands(string? ns = null)
        {
            return ns == null
                ? _commands.Values
                : _commands.Values.Where(c => c.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Processes a raw command input string and executes the corresponding command.
        /// </summary>
        /// <param name="raw">The raw command input.</param>
        /// <returns>
        /// A <see cref="CommandResult"/> containing the result of execution, or a failure message if the input is invalid.
        /// </returns>
        /// <remarks>
        /// <para>The supported command syntax is:</para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <c>command()</c> – A command with zero arguments.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>command(arg1, arg2, ...)</c> – A command with one or more arguments, separated by commas.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>namespace:command()</c> or <c>namespace:command(arg1, ...)</c> – A namespaced command.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>command(...).Extension()</c> – An optional extension applied to a command, with optional extension arguments.
        ///       Extensions are invoked after the main command executes.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>namespace:command(...).Extension(...)</c> – Full namespaced command with extension and optional arguments.
        ///     </description>
        ///   </item>
        /// </list>
        /// <para>Notes:</para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>Parentheses are required for both commands and extensions, even if empty.</description>
        ///   </item>
        ///   <item>
        ///     <description>Arguments may be quoted with single or double quotes to allow commas inside a single argument.</description>
        ///   </item>
        ///   <item>
        ///     <description>Only one extension is supported per command in the current parser.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        public CommandResult ProcessInput(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return CommandResult.Fail("Empty input.");

            // 1️⃣ Pending feedback loop
            if (_pendingFeedbackCommand != null && _pendingFeedback != null)
            {
                var input = raw.Trim();
                var result = _pendingFeedbackCommand.InvokeExtension("feedback", input);

                if (result.Feedback == null)
                {
                    _pendingFeedbackCommand = null;
                    _pendingFeedback = null;
                }
                else
                {
                    _pendingFeedback = result.Feedback;
                }

                return result;
            }

            // 2️⃣ Parse command
            ParsedCommand parsed;
            try { parsed = SimpleCommandParser.Parse(raw); }
            catch (Exception ex) { return CommandResult.Fail($"Syntax error: {ex.Message}"); }

            var cmd = FindCommand(parsed.Name, parsed.Args.Length, parsed.Namespace);
            if (cmd == null)
                return CommandResult.Fail($"Unknown command '{parsed.Name}'{(string.IsNullOrEmpty(parsed.Namespace) ? "" : $" in namespace '{parsed.Namespace}'")}.");

            // 3️⃣ Check for global extensions
            if (!string.IsNullOrEmpty(parsed.Extension))
            {
                var ext = parsed.Extension.ToLowerInvariant();

                // Global .help() returns the description
                if (ext == "help")
                    return CommandResult.Ok(cmd.Description);

                // Global .tryrun() calls TryRun if implemented
                if (ext == "tryrun")
                {
                    var preview = cmd.TryRun(parsed.Args) ?? cmd.Execute(parsed.Args);
                    var feedback = new FeedbackRequest
                    {
                        Prompt = $"Preview:\n{preview.Message}\nProceed with actual execution? (yes/no)",
                        Options = new[] { "yes", "no" }
                    };

                    _pendingFeedbackCommand = cmd;
                    _pendingFeedback = feedback;

                    return new CommandResult
                    {
                        Message = feedback.Prompt,
                        Feedback = feedback
                    };
                }

                // Let command handle other extensions
                var resultExt = cmd.InvokeExtension(ext, parsed.Args);
                return resultExt;
            }

            // 4️⃣ Execute normal command
            var resultExec = cmd.Execute(parsed.Args);

            // 5️⃣ Store feedback if requested
            if (resultExec.Feedback != null)
            {
                _pendingFeedbackCommand = cmd;
                _pendingFeedback = resultExec.Feedback;
            }

            return resultExec;
        }

        /// <summary>
        /// Finds the command. Based on Namespace.
        /// If namespace is null, searches all commands for name and argCount.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="argCount">The argument count.</param>
        /// <param name="ns">The ns.</param>
        /// <returns>Command we called.</returns>
        private ICommand? FindCommand(string name, int argCount, string? ns = null)
        {
            if (!string.IsNullOrWhiteSpace(ns))
            {
                // Exact match with namespace
                _commands.TryGetValue((ns.ToLowerInvariant(), name.ToLowerInvariant(), argCount), out var cmd);
                return cmd;
            }

            // No namespace provided: search all commands
            var matches = _commands.Values
                .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            (c.ParameterCount == argCount || c.ParameterCount == 0)) // allow variable-count
                .ToList();

            if (matches.Count == 1)
                return matches[0];

            if (matches.Count > 1)
                return matches[0]; // or optionally throw an ambiguity error

            return null; // not found
        }

    }
}
