using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weaver;
using Weaver.Core;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Mediator
{
    [TestClass]
    public class WeaveFeedbackTests
    {
        [TestMethod]
        public void DeleteCommand_TriggersFeedback_AndExecutesOnConfirmation()
        {
            // Arrange
            var weave = new Weave();
            weave.Register(new DeleteCommand());

            // Act 1: initial command triggers confirmation
            var result = weave.Process("delete myfile.txt");

            // Assert 1: result requests feedback
            Assert.IsNotNull(result.Feedback, "Expected a feedback request from delete command.");
            Assert.IsTrue(result.Message.Contains("Are you sure"), "Expected delete confirmation message.");
            Assert.IsTrue(result.Feedback.Prompt.Contains("Delete 'myfile.txt'"), "Expected prompt to mention file.");
            Assert.AreEqual("yes", result.Feedback.Options[0], "Feedback options should include 'yes'.");

            var requestId = result.Feedback.RequestId;
            Assert.IsFalse(string.IsNullOrWhiteSpace(requestId), "Feedback must include a valid RequestId.");

            // Act 2: simulate user confirmation
            var followUp = weave.ContinueFeedback(requestId, "yes");

            // Assert 2: execution result confirms deletion
            Assert.IsTrue(followUp.Success, "Follow-up should succeed on 'yes'.");
            Assert.IsTrue(followUp.Message.Contains("deleted"), "Expected success message confirming deletion.");
        }

        [TestMethod]
        public void DeleteCommand_CancelsOnUserInput()
        {
            // Arrange
            var weave = new Weave();
            weave.Register(new DeleteCommand());

            // Step 1: trigger feedback
            var result = weave.Process("delete secret.doc");
            var reqId = result.Feedback?.RequestId ?? throw new AssertFailedException("Missing feedback request.");

            // Step 2: simulate user saying "no"
            var followUp = weave.ContinueFeedback(reqId, "no");

            // Step 3: verify
            Assert.IsFalse(followUp.Success, "Expected fail result when user cancels.");
            Assert.IsTrue(followUp.Message.Contains("cancelled"), "Expected cancellation message.");
        }

        [TestMethod]
        public void DeleteCommand_InvalidResponse_RePromptsUser()
        {
            // Arrange
            var weave = new Weave();
            weave.Register(new DeleteCommand());

            var result = weave.Process("delete temp.log");
            var reqId = result.Feedback?.RequestId ?? throw new AssertFailedException("Missing feedback request.");

            // Step 1: simulate bad input
            var followUp = weave.ContinueFeedback(reqId, "maybe");

            // Step 2: verify that it asks again
            Assert.IsFalse(followUp.Success, "Should not succeed on invalid input.");
            Assert.IsNotNull(followUp.Feedback, "Should re-prompt for input.");
            Assert.IsTrue(followUp.Message.Contains("Unrecognized"), "Should indicate invalid response.");
        }
    }
}
