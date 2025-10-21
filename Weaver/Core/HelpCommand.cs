using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    public sealed class HelpCommand : ICommand
    {
        private readonly IEnumerable<ICommand> _allCommands;

        public HelpCommand(IEnumerable<ICommand> allCommands)
        {
            _allCommands = _allCommands = allCommands;
        }

        public string Namespace => "internal";
        public string Name => "help";
        public string Description => "Lists all commands or shows information about a specific command.";
        public int ParameterCount => 1; // we’ll allow 0 or 1 dynamically
        public int ExtensionParameterCount => 0;

        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        public IReadOnlyDictionary<string, int>? Extensions => null;

        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                var grouped = _allCommands
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
            var match = _allCommands.FirstOrDefault(c =>
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
