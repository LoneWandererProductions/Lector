/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        Memory.cs
 * PURPOSE:     Internal Command, for scripting Engine, lists all stored variables.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Internal Command, for scripting Engine, lists all stored variables.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class Memory : ICommand
    {
        private readonly IVariableRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="Memory"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public Memory(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "memory";

        /// <inheritdoc />
        public string Description => "Lists all stored variables with values and types.";

        /// <inheritdoc />
        public int ParameterCount => 0;

        /// <inheritdoc />
        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            var content = _registry.ToString();
            return CommandResult.Ok(content);
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail("'memory' has no extensions.");
    }
}