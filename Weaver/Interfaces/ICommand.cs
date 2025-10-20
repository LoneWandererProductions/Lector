using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weaver.Messages;

namespace Weaver.Interfaces
{
    public interface ICommand
    {
        string Name { get; }

        string Description { get; }

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
    }
}
