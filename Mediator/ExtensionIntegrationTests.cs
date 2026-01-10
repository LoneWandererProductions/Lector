/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        ExtensionIntegrationTests.cs
 * PURPOSE:     Test Extension integration with commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver;

namespace Mediator
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

        /// <summary>
        /// Help global extension works for DeleteCommand.
        /// </summary>
        [TestMethod]
        public void DeleteCommandHelpExtensionWorks()
        {
            // Execute command with .help extension
            var result = _weaver.ProcessInput("delete(file.txt).help()");

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Success, "Help extension always succeeds");
            Assert.IsTrue(result.Message.Contains("Deletes a resource"), "Help message should include command description");
            Assert.IsNull(result.Feedback, "Help should not require feedback");

            result = _weaver.ProcessInput("help(delete)");
            Assert.IsTrue(result.Message.Contains("Deletes a resource by name"));
        }

        /// <summary>
        /// Help global extension works with unknown command gracefully.
        /// </summary>
        [TestMethod]
        public void HelpExtensionHandlesUnknownCommand()
        {
            var result = _weaver.ProcessInput("nonexistent().help()");

            Assert.IsNotNull(result, "Result should not be null");

        }
    }
}