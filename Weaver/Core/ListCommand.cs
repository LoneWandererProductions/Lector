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
    public sealed class ListCommand : ICommand
    {
        public string Namespace => "internal";
        public string Name => "list";
        public string Description => "Lists all commands.";
        public int ParameterCount => 0; // we’ll allow 0

        private readonly Func<IEnumerable<ICommand>> _getCommands;

        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        public IReadOnlyDictionary<string, int>? Extensions => null;

        public ListCommand(Func<IEnumerable<ICommand>> getCommands)
        {
            _getCommands = getCommands;
        }


        public CommandResult Execute(params string[] args)
        {
            var allCommands = _getCommands();

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

        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail("'list' has no extensions.");
        }
    }
}
