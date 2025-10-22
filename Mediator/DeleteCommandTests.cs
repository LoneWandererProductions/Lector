/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        DeleteCommandTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Mediator
{
    [TestClass]
    public class DeleteCommandTests
    {
        /// <summary>
        /// The command
        /// </summary>
        private DeleteCommand _command = null!;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _command = new DeleteCommand();
        }

        /// <summary>
        /// Invokes the extension should handle yes.
        /// </summary>
        [TestMethod]
        public void InvokeExtension_ShouldHandleYes()
        {
            var result = _command.InvokeExtension("feedback", "yes");
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Resource deleted successfully.", result.Message);
        }

        /// <summary>
        /// Invokes the extension should handle no.
        /// </summary>
        [TestMethod]
        public void InvokeExtension_ShouldHandleNo()
        {
            var result = _command.InvokeExtension("feedback", "no");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Deletion cancelled by user.", result.Message);
        }

        /// <summary>
        /// Invokes the extension should handle cancel.
        /// </summary>
        [TestMethod]
        public void InvokeExtension_ShouldHandleCancel()
        {
            var result = _command.InvokeExtension("feedback", "cancel");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Deletion cancelled by user.", result.Message);
        }

        /// <summary>
        /// Invokes the extension should re prompt on unknown input.
        /// </summary>
        [TestMethod]
        public void InvokeExtension_ShouldRePromptOnUnknownInput()
        {
            var result = _command.InvokeExtension("feedback", "maybe");

            Assert.IsNotNull(result.Feedback);
            Assert.IsTrue(result.Message.Contains("Unrecognized"));
            Assert.IsTrue(result.Feedback!.Prompt.Contains("Please answer"));
            CollectionAssert.AreEquivalent(
                new[] { "yes", "no", "cancel" },
                result.Feedback.Options);
        }
    }
}