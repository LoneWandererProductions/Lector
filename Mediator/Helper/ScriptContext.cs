/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Helper
 * FILE:        ScriptContext.cs
 * PURPOSE:     Helper class for command registration.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Mediator.Helper
{
    /// <summary>
    /// Dummy context for command registration
    /// </summary>
    public class ScriptContext
    {
        /// <summary>
        /// The commands
        /// </summary>
        private readonly Dictionary<string, DelegateCommand> _commands = new();

        /// <summary>
        /// Registers the command.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        public void RegisterCommand(string name, DelegateCommand command)
        {
            _commands[name] = command;
        }

        /// <summary>
        /// Tries the get command.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public bool TryGetCommand(string? statement, out DelegateCommand? command)
        {
            if (string.IsNullOrWhiteSpace(statement))
            {
                command = null;
                return false;
            }

            // Simple matching: strip parentheses and whitespace
            var key = statement.Split('(')[0].Trim();
            return _commands.TryGetValue(key, out command);
        }
    }
}