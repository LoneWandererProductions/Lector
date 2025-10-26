/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        DelegateCommand.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.ScriptEngine;

namespace Mediator
{
    /// <summary>
    /// Simple delegate command for testing
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class DelegateCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;
        public DelegateCommand(Action execute) => _execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns>
        ///   <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        /// </returns>
        public bool CanExecute(object? parameter) => true;

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object? parameter) => _execute();

        private static IEnumerable<(string Category, string Statement)> FlattenNodes(List<ScriptNode> nodes)
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case LabelNode ln:
                        yield return ("Label", ln.Name);

                        break;
                    case GotoNode gn:
                        yield return ("Goto", gn.Target);

                        break;
                    case CommandNode cn:
                        yield return ("Command", cn.Command);

                        break;
                    case AssignmentNode an:
                        yield return ("Assignment", $"{an.Variable} = {an.Expression}");

                        break;
                    case IfNode ifn:
                        yield return ("If_Condition", ifn.Condition);

                        foreach (var child in FlattenNodes(ifn.TrueBranch))
                            yield return child;

                        if (ifn.FalseBranch != null)
                            foreach (var child in FlattenNodes(ifn.FalseBranch))
                                yield return child;

                        break;
                    case DoWhileNode dw:
                        foreach (var child in FlattenNodes(dw.Body))
                            yield return child;

                        yield return ("While_Condition", dw.Condition);

                        break;
                }
            }
        }
    }
}