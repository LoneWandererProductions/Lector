/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        CommandTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Core;

namespace Mediator
{
    [TestClass]
    public class CommandTests
    {
        private List<ICommand> _allCommands = new();
        private HelpCommand _helpCommand = null!;
        private DeleteCommand _deleteCommand = null!;

        [TestInitialize]
        public void Setup()
        {
            // Create commands
            _deleteCommand = new DeleteCommand();
            _allCommands = new List<ICommand> { _deleteCommand };
            _helpCommand = new HelpCommand(_allCommands);
        }

        [TestMethod]
        public void HelpCommand_ListAllCommands_ReturnsDescription()
        {
            var result = _helpCommand.Execute();
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Message.Contains("delete"));
            Assert.IsTrue(result.Message.Contains("Deletes a resource"));
        }

        [TestMethod]
        public void HelpCommand_SpecificCommand_ReturnsCorrectInfo()
        {
            var result = _helpCommand.Execute("delete");
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Message.Contains("delete"));
            Assert.IsTrue(result.Message.Contains("Deletes a resource"));
        }

        [TestMethod]
        public void DeleteCommand_Execute_FeedbackRequested()
        {
            var result = _deleteCommand.Execute("file.txt");
            Assert.IsFalse(result.Success); // It's just preview, not confirmed
            Assert.IsNotNull(result.Feedback);
            Assert.AreEqual("Delete 'file.txt'? (yes/no/cancel)", result.Feedback!.Prompt);
        }

        [TestMethod]
        public void DeleteCommand_InvokeExtension_Feedback_Yes()
        {
            var feedback = _deleteCommand.Execute("file.txt").Feedback!;
            var result = _deleteCommand.InvokeExtension("feedback", "yes");
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Resource deleted successfully.", result.Message);
        }

        [TestMethod]
        public void DeleteCommand_InvokeExtension_Feedback_No()
        {
            var result = _deleteCommand.InvokeExtension("feedback", "no");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Deletion cancelled by user.", result.Message);
        }

        [TestMethod]
        public void DeleteCommand_InvokeExtension_Feedback_Invalid()
        {
            var result = _deleteCommand.InvokeExtension("feedback", "maybe");
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Feedback);
            Assert.AreEqual("Please answer: yes / no / cancel", result.Feedback!.Prompt);
        }
    }
}
