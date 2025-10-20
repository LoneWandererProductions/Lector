using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver
{
    public sealed class Weave
    {
        // continuation map: requestId -> continuation that consumes user input
        private readonly Dictionary<string, Func<string, CommandResult>> _continuations
            = new(StringComparer.OrdinalIgnoreCase);

        // your existing command registry (keyed by (name, paramCount) or similar)
        private readonly Dictionary<(string name, int paramCount), ICommand> _commands
            = new();

        public void Register(ICommand command)
            => _commands[(command.Name.ToLowerInvariant(), command.ParameterCount)] = command;

        public CommandResult ProcessInput(string raw)
        {
            // parsing: name(args).extension(...) or name(args)
            // (keep your parser; here is an example split like before)
            var extSplit = raw.Split('.', 2);
            var main = extSplit[0].Trim();
            string? extension = extSplit.Length > 1 ? extSplit[1].Trim() : null;

            var open = main.IndexOf('(');
            var close = main.LastIndexOf(')');
            if (open < 0 || close < 0 || close < open)
                return CommandResult.Fail("Syntax error: expected command(args).");

            var name = main[..open].Trim();
            var args = main[(open + 1)..close]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // find matching command by name + arg count
            if (!_commands.TryGetValue((name.ToLowerInvariant(), args.Length), out var cmd))
                return CommandResult.Fail($"Unknown command '{name}' with {args.Length} parameters.");

            CommandResult result;
            if (extension is not null)
                result = cmd.InvokeExtension(extension.Trim('(', ')', ' '), args);
            else
                result = cmd.Execute(args);

            // if the command asks for feedback, register its continuation
            if (result.Feedback is { } fb)
            {
                // register continuation: calls back into the target command with a special extension name "feedback"
                // commands implement InvokeExtension("feedback", userInput) to resume
                _continuations[fb.RequestId] = (userInput) => cmd.InvokeExtension("feedback", userInput);
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
                var resumed = continuation(userInput ?? "");
                // allow resumed result to itself produce a new Feedback (chainable)
                if (resumed.Feedback is { } nextFb)
                {
                    // optionally replace continuation with the new target
                    // here we assume the same ICommand handles it, so continuation already registered by ProcessInput or command invoke
                    // but to be safe, you can choose to register a default continuation here
                    // _continuations[nextFb.RequestId] = ...;
                }
                return resumed;
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Feedback continuation failed: {ex.Message}");
            }
        }
    }
}