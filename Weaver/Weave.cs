using Weaver.Core;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.Parser;

namespace Weaver
{
    public record CommandSignature(string Namespace, string Name, int ParameterCount);

    public sealed class Weave
    {
        private readonly Dictionary<string, Func<string, CommandResult>> _continuations
            = new(StringComparer.OrdinalIgnoreCase);

        // key: (namespace, name, paramCount)
        private readonly Dictionary<(string ns, string name, int paramCount), ICommand> _commands
            = new();

        private readonly Dictionary<(string ns, string name, int paramCount), Dictionary<string, int>> _commandExtensions
            = new();

        // built-in extensions to inject into every command
        private static readonly Dictionary<string, CommandExtension> _globalExtensions
            = new(StringComparer.OrdinalIgnoreCase)
            {
                ["help"] = new CommandExtension { Name = "help", ParameterCount = 0, IsInternal = true },
                ["tryrun"] = new CommandExtension { Name = "tryrun", ParameterCount = 0, IsInternal = true, IsPreview = true }
            };


        public void Register(ICommand command)
        {
            // Build a merged dictionary for internal use
            var mergedExtensions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Command-defined extensions
            if (command.Extensions != null)
            {
                foreach (var kv in command.Extensions)
                    mergedExtensions[kv.Key] = kv.Value;
            }

            // Global extensions
            foreach (var kv in _globalExtensions)
                if (!mergedExtensions.ContainsKey(kv.Key))
                    mergedExtensions[kv.Key] = kv.Value.ParameterCount;

            // Store internally as needed for ProcessInput
            _commandExtensions[(command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)]
                = mergedExtensions;

            _commands[(command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)] = command;
        }


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

            ParsedCommand parsed;
            try
            {
                //parser
                parsed = SimpleCommandParser.Parse(raw);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Syntax error: {ex.Message}");
            }

            // lookup command
            if (!_commands.TryGetValue((parsed.Namespace.ToLowerInvariant(), parsed.Name.ToLowerInvariant(), parsed.Args.Length), out var cmd))
                return CommandResult.Fail($"Unknown command '{parsed.Name}' in namespace '{parsed.Namespace}' with {parsed.Args.Length} parameters.");

            CommandResult result;

            if (!string.IsNullOrEmpty(parsed.Extension))
            {
                // Extension exists?
                if (!cmd.Extensions.TryGetValue(parsed.Extension, out var extParamCount))
                    return CommandResult.Fail($"Unknown extension '{parsed.Extension}' for command '{cmd.Name}'.");

                if (parsed.ExtensionArgs.Length != extParamCount)
                    return CommandResult.Fail($"Extension '{parsed.Extension}' expects {extParamCount} arguments, got {parsed.ExtensionArgs.Length}.");

                // Check for global .help / .tryrun
                if (_globalExtensions.TryGetValue(parsed.Extension, out var globalExt))
                {
                    if (globalExt.Name == "help")
                    {
                        result = CommandResult.Ok($"{cmd.Namespace}:{cmd.Name} — {cmd.Description}");
                    }
                    else if (globalExt.IsPreview)
                    {
                        var preview = cmd.Execute(parsed.Args);
                        result = new CommandResult
                        {
                            Message = $"Preview:\n{preview.Message}\n\nRun for real? (yes/no)",
                            Feedback = new FeedbackRequest
                            {
                                Prompt = "Confirm execution (yes/no)",
                                Options = new[] { "yes", "no" },
                                RequestId = Guid.NewGuid().ToString()
                            }
                        };
                    }
                    else
                    {
                        // fallback
                        result = cmd.InvokeExtension(parsed.Extension, parsed.ExtensionArgs);
                    }
                }
                else
                {
                    result = cmd.InvokeExtension(parsed.Extension, parsed.ExtensionArgs);
                }
            }
            else
            {
                result = cmd.Execute(parsed.Args);
            }

            // register feedback continuation
            if (result.Feedback is { } fb)
            {
                _continuations[fb.RequestId] = userInput =>
                {
                    if (!string.IsNullOrEmpty(parsed.Extension) &&
                        parsed.Extension.Equals("tryrun", StringComparison.OrdinalIgnoreCase))
                    {
                        return userInput.Equals("yes", StringComparison.OrdinalIgnoreCase) ? cmd.Execute(parsed.Args) : CommandResult.Fail("Execution cancelled.");
                    }

                    return cmd.InvokeExtension("feedback", userInput);
                };
            }

            return result;
        }

        public CommandResult ContinueFeedback(string requestId, string userInput)
        {
            if (string.IsNullOrEmpty(requestId))
                return CommandResult.Fail("Invalid feedback id.");

            if (!_continuations.TryGetValue(requestId, out var continuation))
                return CommandResult.Fail($"No pending feedback with id '{requestId}'.");

            _continuations.Remove(requestId);

            try
            {
                return continuation(userInput ?? "");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Feedback continuation failed: {ex.Message}");
            }
        }
    }
}
