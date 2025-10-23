/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        Weave.cs
 * PURPOSE:     Command execution and extension management engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ParseEngine;

namespace Weaver
{
    /// <summary>
    /// Core command execution engine.
    /// Manages registration, lookup, execution, and feedback for commands with optional extensions.
    /// </summary>
    public sealed class Weave
    {
        /// <summary>
        /// Stores registered commands, keyed by (namespace, name, parameter count)
        /// </summary>
        private readonly Dictionary<(string ns, string name, int paramCount), ICommand> _commands
            = new();

        /// <summary>
        /// Stores per-command extension definitions
        /// </summary>
        private readonly Dictionary<(string ns, string name, int paramCount), Dictionary<string, int>>
            _commandExtensions
                = new();

        /// <summary>
        /// Built-in extensions injected into every command
        /// </summary>
        private static readonly Dictionary<string, CommandExtension> GlobalExtensions
            = new(StringComparer.OrdinalIgnoreCase)
            {
                ["help"] = new CommandExtension { Name = "help", ParameterCount = 0, IsInternal = true },
                ["tryrun"] = new CommandExtension
                    { Name = "tryrun", ParameterCount = 0, IsInternal = true, IsPreview = true }
            };

        /// <summary>
        /// The pending feedback command
        /// </summary>
        private ICommand? _pendingFeedbackCommand;

        /// <summary>
        /// The pending feedback
        /// </summary>
        private FeedbackRequest? _pendingFeedback;

        /// <summary>
        /// The extensions
        /// </summary>
        private readonly List<ICommandExtension> _extensions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Weave"/> class.
        /// </summary>
        public Weave()
        {
            // Register internal commands
            var list = new ListCommand(() => GetCommands());
            Register(list);

            var help = new HelpCommand(() => GetCommands());
            Register(help);

            _extensions.Add(new HelpExtension());
            _extensions.Add(new TryRunExtension());
        }

        /// <summary>
        /// Registers a command in the engine.
        /// Combines command-defined extensions with global extensions for internal use.
        /// </summary>
        /// <param name="command">The command to register.</param>
        public void Register(ICommand command)
        {
            var mergedExtensions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Include command-defined extensions
            if (command.Extensions != null)
            {
                foreach (var kv in command.Extensions)
                    mergedExtensions[kv.Key] = kv.Value;
            }

            // Include global extensions, skipping duplicates
            foreach (var kv in GlobalExtensions)
                if (!mergedExtensions.ContainsKey(kv.Key))
                    mergedExtensions[kv.Key] = kv.Value.ParameterCount;

            // Store extensions for internal lookup
            _commandExtensions[
                    (command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)] =
                mergedExtensions;

            // Store the command itself
            _commands[(command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)] =
                command;
        }

        /// <summary>
        /// Registers an external command extension with the runtime.
        /// These extensions are applied globally to all commands.
        /// </summary>
        /// <param name="extension">The extension to register.</param>
        public void RegisterExtension(ICommandExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            // Avoid duplicate registration
            if (_extensions.Any(e => e.Name.Equals(extension.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Extension '{extension.Name}' is already registered.");

            _extensions.Add(extension);
        }

        /// <summary>
        /// Retrieves all registered commands, optionally filtered by namespace.
        /// </summary>
        private IEnumerable<ICommand> GetCommands(string? ns = null)
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
        /// A <see cref="CommandResult" /> containing the result of execution, or a failure message if the input is invalid.
        /// </returns>
        /// <remarks>
        /// <para>The supported command syntax is:</para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <c>command()</c> – A command with zero arguments.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>command(arg1, arg2, ...)</c> – A command with one or more arguments, separated by commas.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>namespace:command()</c> or <c>namespace:command(arg1, ...)</c> – A namespaced command.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>command(...).Extension()</c> – An optional extension applied to a command, with optional extension arguments.
        /// Extensions are invoked after the main command executes.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>namespace:command(...).Extension(...)</c> – Full namespaced command with extension and optional arguments.
        /// </description>
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

            raw = raw.Trim();

            // 1️⃣ Handle pending feedback first
            if (_pendingFeedbackCommand != null && _pendingFeedback != null)
            {
                var feedbackResult = _pendingFeedbackCommand.InvokeExtension("feedback", raw);

                if (!feedbackResult.RequiresConfirmation)
                    ClearFeedbackState();
                else
                    _pendingFeedback = feedbackResult.Feedback;

                return feedbackResult;
            }

            // 2️⃣ Parse the command
            ParsedCommand parsed;
            try
            {
                parsed = SimpleCommandParser.Parse(raw);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Syntax error: {ex.Message}");
            }

            var cmd = FindCommand(parsed.Name, parsed.Args.Length, parsed.Namespace);
            if (cmd == null)
                return CommandResult.Fail(
                    $"Unknown command '{parsed.Name}'{(string.IsNullOrEmpty(parsed.Namespace) ? "" : $" in namespace '{parsed.Namespace}'")}.");

            // 3️⃣ Handle extension if present
            if (!string.IsNullOrEmpty(parsed.Extension))
            {
                var extName = parsed.Extension.ToLowerInvariant();

                CommandResult resultExec;
                // Check registered extensions
                var ext = _extensions.FirstOrDefault(e => e.Name.Equals(extName, StringComparison.OrdinalIgnoreCase));
                if (ext != null)
                    resultExec = ext.Invoke(cmd, parsed.Args, cmd.Execute); // <-- returns preview with Feedback
                else
                    resultExec = cmd.InvokeExtension(extName, parsed.Args);

                // Store feedback if needed
                if (resultExec.RequiresConfirmation && resultExec.Feedback != null)
                {
                    _pendingFeedbackCommand = cmd;
                    _pendingFeedback = resultExec.Feedback;
                }

                return resultExec;
            }

            // 4️⃣ Execute normal command
            var result = cmd.Execute(parsed.Args);

            // 5️⃣ Store pending feedback if returned
            if (result.RequiresConfirmation)
            {
                _pendingFeedbackCommand = cmd;
                _pendingFeedback = result.Feedback;
            }

            return result;
        }


        /// <summary>
        /// Finds the command.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="argCount">The argument count.</param>
        /// <param name="ns">The ns.</param>
        /// <returns>The first available fitting command.</returns>
        private ICommand? FindCommand(string name, int argCount, string? ns = null)
        {
            if (!string.IsNullOrWhiteSpace(ns))
            {
                _commands.TryGetValue((ns.ToLowerInvariant(), name.ToLowerInvariant(), argCount), out var cmd);
                return cmd;
            }

            var matches = _commands.Values
                .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                            && (c.ParameterCount == argCount || c.ParameterCount == 0))
                .ToList();

            if (matches.Count == 1)
                return matches[0];

            if (matches.Count > 1)
                return matches[0]; // TODO: optionally handle ambiguity

            return null;
        }

        /// <summary>
        /// Clears feedback state when finished.
        /// </summary>
        private void ClearFeedbackState()
        {
            _pendingFeedbackCommand = null;
            _pendingFeedback = null;
        }
    }
}