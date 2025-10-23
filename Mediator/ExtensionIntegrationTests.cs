using Weaver;

namespace Mediator
{
    [TestClass]
    public class ExtensionIntegrationTests
    {
        private Weave _weaver = null!;

        [TestInitialize]
        public void Init()
        {
            _weaver = new Weave();

            // Register DeleteCommand
            _weaver.Register(new DeleteCommand());

            // Register external extension
            _weaver.RegisterExtension(new AppendExtension());
        }

        [TestMethod]
        public void DeleteCommand_WithAppendExtension_Works()
        {
            // Execute command with extension
            var result = _weaver.ProcessInput("delete(file.txt).append()");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Message.Contains("[EXT]"), "Extension did not modify message");
            Assert.IsFalse(result.Success); // because DeleteCommand returns RequiresConfirmation feedback initially
            Assert.IsNotNull(result.Feedback, "Feedback should exist");
        }
    }
}