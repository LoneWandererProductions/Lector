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
    public sealed class HelpCommand : ICommand
    {
        private readonly Func<IEnumerable<ICommand>> _getCommands;

        public HelpCommand(Func<IEnumerable<ICommand>> getCommands)
        {
            _getCommands = getCommands;
        }

        public string Namespace => "internal";
        public string Name => "help";
        public string Description => "Lists all commands or shows information about a specific command.";
        public int ParameterCount => 1; // we’ll allow 0 or 1 dynamically

        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        public IReadOnlyDictionary<string, int>? Extensions => null;

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

        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail("'help' has no extensions.");
        }
    }
}
