/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        CommandWithoutTry.cs
 * PURPOSE:     Test Extension integration with commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator
{
    public sealed class CommandWithoutTry : ICommand
    {
        /// <inheritdoc />
        public string Name => "noTry";

        /// <inheritdoc />
        public string Description => "test";

        /// <inheritdoc />
        public string Namespace => "test";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
            => new CommandResult { Message = $"EXEC {args[0]}", Success = true };

        /// <inheritdoc />
        /// <summary>
        /// Here is the important part:
        /// </summary>
        public CommandResult InvokeExtension(string ext, params string[] args)
        {
            if (ext == "tryrun")
                return new CommandResult { Message = $"[Preview-Fallback] {args[0]}", Success = true };

            return CommandResult.Fail("no ext");
        }
    }
}