/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.TryRun
 * FILE:        ExtensionIntegrationTests.cs
 * PURPOSE:     Test Extension integration with commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver;

namespace Mediator.TryRun
{
    [TestClass]
    public class ExtensionIntegrationTests
    {
        /// <summary>
        /// The weaver
        /// </summary>
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

        /// <summary>
        /// Deletes the command with append extension works.
        /// </summary>
        [TestMethod]
        public void DeleteCommandWithAppendExtensionWorks()
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