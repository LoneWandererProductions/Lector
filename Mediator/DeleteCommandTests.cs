namespace Mediator
{
    [TestClass]
    public class DeleteCommandTests
    {
        private DeleteCommand _command = null!;

        [TestInitialize]
        public void Setup()
        {
            _command = new DeleteCommand();
        }

        [TestMethod]
        public void Execute_ShouldRequestConfirmation()
        {
            // act
            var result = _command.Execute("testFile");

            // assert
            Assert.IsTrue(result.RequiresConfirmation);
            Assert.IsNotNull(result.Feedback);
            Assert.AreEqual("Delete 'testFile'? (yes/no/cancel)", result.Feedback!.Prompt);
        }

        [TestMethod]
        public void InvokeExtension_ShouldHandleYes()
        {
            var result = _command.InvokeExtension("feedback", "yes");
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Resource deleted successfully.", result.Message);
        }

        [TestMethod]
        public void InvokeExtension_ShouldHandleNo()
        {
            var result = _command.InvokeExtension("feedback", "no");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Deletion cancelled by user.", result.Message);
        }

        [TestMethod]
        public void InvokeExtension_ShouldHandleCancel()
        {
            var result = _command.InvokeExtension("feedback", "cancel");
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Operation cancelled.", result.Message);
        }

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

        [TestMethod]
        public void InvokeExtension_ShouldHandleHelp()
        {
            var result = _command.InvokeExtension("help");
            Assert.IsTrue(result.Success);
            StringAssert.Contains(result.Message, "Usage: delete");
        }
    }
}
