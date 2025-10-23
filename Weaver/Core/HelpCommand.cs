/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        HelpCommand.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    ///     Internal command, shows basic information about available commands and about Weaver itself.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class HelpCommand : ICommand
    {
        /// <summary>
        /// The get commands
        /// </summary>
        private readonly Func<IEnumerable<ICommand>> _getCommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpCommand"/> class.
        /// </summary>
        /// <param name="getCommands">The get commands.</param>
        public HelpCommand(Func<IEnumerable<ICommand>> getCommands)
        {
            _getCommands = getCommands;
        }

        /// <inheritdoc />
        public string Namespace => "internal";

        /// <inheritdoc />
        public string Name => "help";

        /// <inheritdoc />
        public string Description => "Lists all commands or shows information about a specific command.";

        /// <inheritdoc />
        public int ParameterCount => 1; // we’ll allow 0 or 1 dynamically

        /// <inheritdoc />
        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            var allCommands = _getCommands();


            if (args.Length == 0)
            {
                var grouped = allCommands
                    .GroupBy(c => c.Namespace)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var entries = string.Join("\n", g.Select(c => $"  {c.Name} — {c.Description}"));
                        return $"{g.Key}:\n{entries}";
                    });

                return CommandResult.Ok("Available commands:\n\n" + string.Join("\n\n", grouped));
            }

            var cmdName = args[0];
            var match = allCommands.FirstOrDefault(c =>
                c.Name.Equals(cmdName, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return CommandResult.Ok($"{match.Namespace}:{match.Name} — {match.Description}");

            return CommandResult.Fail($"Unknown command '{cmdName}'.");
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail("'help' has no extensions.");
        }
    }
}