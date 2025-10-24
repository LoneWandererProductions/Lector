/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        HelpExtension.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <summary>
    /// Provides help information for a command using the .help extension.
    /// </summary>
    public sealed class HelpExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => "help";

        /// <inheritdoc />
        public string Description => "Provides help information for a command using the .help extension.";

        /// <inheritdoc />
        public string Namespace => "global";

        /// <inheritdoc />
        public int ExtensionParameterCount => 0;

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            // Return description; executor not needed
            return CommandResult.Ok(command.Description);
        }
    }
}