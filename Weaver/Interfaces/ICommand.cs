/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        ICommand.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;

namespace Weaver.Interfaces
{
    public interface ICommand
    {
        string Name { get; }

        string Description { get; }

        string Namespace { get; }

        CommandSignature Signature { get; }

        int ParameterCount => 0; // default means variable
        int ExtensionParameterCount => 0;

        /// <summary>
        /// Executes the command with given arguments.
        /// Returns a result that can include text, status, or further options.
        /// </summary>
        CommandResult Execute(params string[] args);

        /// <summary>
        /// Optional extension calls like .help(), .save(), .tryrun()
        /// </summary>
        CommandResult InvokeExtension(string extensionName, params string[] args);

        /// <summary>
        /// Optional: expose available extensions with parameter counts
        /// </summary>
        IReadOnlyDictionary<string, int>? Extensions { get; }

        /// <summary>
        /// Optional preview mode: called by .tryrun().
        /// Returns a preview result without committing side effects.
        /// </summary>
        CommandResult? TryRun(params string[] args) => null;
    }
}
