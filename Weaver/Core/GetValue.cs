/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        GetValue.cs
 * PURPOSE:     Retrieves a stored value and type from the Scripter Engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    ///     Internal command, retrieves a value and type from the Scripter registry.
    /// </summary>
    public sealed class GetValue : ICommand
    {
        private readonly IVariableRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetValue"/> class.
        /// </summary>
        /// <param name="registry">The variable registry to read from.</param>
        public GetValue(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "GetValue";

        /// <inheritdoc />
        public string Description =>
            "Gets a value from the registry. If the key does not exist, returns a null value with type 'Wstring'.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length != 1)
                return CommandResult.Fail("Usage: getValue([key])");

            var key = args[0];
            if (string.IsNullOrWhiteSpace(key))
                return CommandResult.Fail("Key cannot be empty.");

            if (!_registry.TryGet(key, out var value, out var type))
            {
                return new CommandResult
                {
                    Success = false,
                    Message = $"Key '{key}' not found.",
                    Type = EnumTypes.Wstring,
                    Value = null
                };
            }

            return new CommandResult
            {
                Success = true,
                Message = $"Retrieved key '{key}' of type {type}.",
                Type = type,
                Value = value
            };
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail("'getValue' has no extensions.");
        }
    }
}
