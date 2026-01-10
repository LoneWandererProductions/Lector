/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        WeaveFeedbackTests.cs
 * PURPOSE:     Test feedback loop of Weaver
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Mediator.Core;
using Weaver;

namespace Mediator
{
    [TestClass]
    public class WeaveFeedbackTests
    {
        /// <summary>
        /// Deletes the command triggers feedback and executes on confirmation.
        /// </summary>
        [TestMethod]
        public void DeleteCommand_TriggersFeedback_AndExecutesOnConfirmation()
        {
            var weave = new Weave();
            weave.Register(new DeleteCommand());

            // Initial command triggers feedback
            var result = weave.ProcessInput("delete(\"myfile.txt\")");

            Assert.IsNotNull(result.Feedback, "Expected a feedback request from delete command.");
            Assert.IsTrue(result.Message.Contains("Are you sure"), "Expected delete confirmation message.");
            Assert.IsTrue(result.Feedback.Prompt.Contains("Delete 'myfile.txt'"), "Expected prompt to mention file.");
            Assert.AreEqual("yes", result.Feedback.Options[0], "Feedback options should include 'yes'.");

            // Provide user input directly to resolve feedback
            var followUp = weave.ProcessInput("yes");

            Assert.IsTrue(followUp.Success, "Follow-up should succeed on 'yes'.");
            Assert.IsTrue(followUp.Message.Contains("deleted"), "Expected success message confirming deletion.");
        }

        /// <summary>
        /// Deletes the command cancels on user input.
        /// </summary>
        [TestMethod]
        public void DeleteCommand_CancelsOnUserInput()
        {
            var weave = new Weave();
            weave.Register(new DeleteCommand());

            var result = weave.ProcessInput("delete(\"secret.doc\")");
            Assert.IsNotNull(result.Feedback, "Expected a feedback request from delete command.");

            var followUp = weave.ProcessInput("no");

            Assert.IsFalse(followUp.Success, "Expected fail result when user cancels.");
            Assert.IsTrue(followUp.Message.Contains("cancelled"), "Expected cancellation message.");
        }

        /// <summary>
        /// Deletes the command invalid response re prompts user.
        /// </summary>
        [TestMethod]
        public void DeleteCommand_InvalidResponse_RePromptsUser()
        {
            var weave = new Weave();
            weave.Register(new DeleteCommand());

            var result = weave.ProcessInput("delete(\"temp.log\")");
            Assert.IsNotNull(result.Feedback, "Expected a feedback request from delete command.");

            // Invalid input, feedback should remain
            var followUp = weave.ProcessInput("maybe");

            Assert.IsFalse(followUp.Success, "Should not succeed on invalid input.");
            Assert.IsNotNull(followUp.Feedback, "Should re-prompt for input.");
            Assert.IsTrue(followUp.Message.Contains("Unrecognized"), "Should indicate invalid response.");
        }
    }
}