/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        ScriptExecutionContext.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.ScriptEngine;

namespace Mediator
{
    /// <summary>
    /// Dummy context for command registration
    /// </summary>
    public class ScriptExecutionContext
    {
        /// <summary>
        /// The commands
        /// </summary>
        private readonly Dictionary<string, DelegateCommand> _commands = new();

        public void RegisterCommand(string name, DelegateCommand command)
        {
            _commands[name] = command;
        }

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