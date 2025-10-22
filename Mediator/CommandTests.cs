/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        CommandTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Core;
using Weaver;

namespace Mediator
{
    [TestClass]
    public class CommandTests
    {
        private List<ICommand> _allCommands = new();
        private DeleteCommand _deleteCommand = null!;
        private Weave _weaver = new Weave();

        [TestInitialize]
        public void Setup()
        {
            // Create commands
            _deleteCommand = new DeleteCommand();
            _allCommands = new List<ICommand> { _deleteCommand };
            _weaver.Register(_deleteCommand);
        }

        [TestMethod]
        public void HelpCommand_ListAllCommands_ReturnsDescription()
        {
            var result = _weaver.ProcessInput("list()");
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Message.Contains("delete"));
            Assert.IsTrue(result.Message.Contains("Deletes a resource"));
        }

        [TestMethod]
        public void HelpCommand_SpecificCommand_ReturnsCorrectInfo()
        {
            var result = _weaver.ProcessInput("help(delete)");
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.Message.Contains("delete"));
            Assert.IsTrue(result.Message.Contains("Deletes a resource"));
        }

        [TestMethod]
        public void DeleteCommand_Execute_Weave_FeedbackRequested()
        {
            var result = _weaver.ProcessInput("delete(file.txt)");

            Assert.IsFalse(result.Success, "Delete preview should not be considered success before confirmation.");
            Assert.IsNotNull(result.Feedback, "Feedback should be requested after initial delete call.");
            Assert.AreEqual("Delete 'file.txt'? (yes/no/cancel)", result.Feedback!.Prompt);
        }

        [TestMethod]
        public void DeleteCommand_TryRun_FeedbackFlow_Works()
        {
            // Step 1: simulate .tryrun
            var result = _weaver.ProcessInput("delete(file.txt).tryrun()");

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Feedback);
            Assert.IsTrue(result.Message.Contains("Preview:"));
            Assert.IsTrue(result.Feedback!.Prompt.Contains("Are you sure you want to delete"), result.Feedback!.Prompt);

            var requestId = result.Feedback!.RequestId;
            Assert.IsFalse(string.IsNullOrEmpty(requestId));

            // Step 2: user confirms execution
            var confirmResult = _weaver.ProcessInput( "yes");

            Assert.IsTrue(confirmResult.Success);
            Assert.AreEqual("Resource deleted successfully.", confirmResult.Message);

            // Step 3: user cancels execution
            var cancelResult = _weaver.ProcessInput("delete(file.txt).tryrun()");
            var cancelRequestId = cancelResult.Feedback!.RequestId;
            var cancelFeedback = _weaver.ProcessInput("no");

            Assert.IsFalse(cancelFeedback.Success);
            Assert.AreEqual("Deletion cancelled by user.", cancelFeedback.Message);
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

        [TestMethod]
        public void DeleteCommand_InvokeExtension_Feedback_Cancel()
        {
            var result = _deleteCommand.InvokeExtension("feedback", "cancel");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Deletion cancelled by user.", result.Message);
        }
    }
}
