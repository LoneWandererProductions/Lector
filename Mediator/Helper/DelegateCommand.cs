/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Helper
 * FILE:        DelegateCommand.cs
 * PURPOSE:     Helper for Script Testing
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Mediator.Helper
{
    /// <inheritdoc />
    /// <summary>
    /// Simple delegate command for testing
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class DelegateCommand : System.Windows.Input.ICommand
    {
        /// <summary>
        /// The execute
        /// </summary>
        private readonly Action _execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        public DelegateCommand(Action execute) => _execute = execute;

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <inheritdoc />
        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns>
        ///   <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        /// </returns>
        public bool CanExecute(object? parameter) => true;

        /// <inheritdoc />
        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object? parameter) => _execute();
    }
}