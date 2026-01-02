/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        ExtensionIntegrationTests.cs
 * PURPOSE:     Test Extension integration with commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver;
using Weaver.Core;
using Weaver.Messages;

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
        /// Tries the run extension behavior with and without try run.
        /// </summary>
        [TestMethod]
        public void TryRunExtensionBehaviorWithAndWithoutTryRun()
        {
            var extension = new TryRunExtension();

            var cmdWithTry = new CommandWithTry();
            var cmdWithoutTry = new CommandWithoutTry();

            // executor simply calls Execute on the command
            CommandResult ExecutorWithTry(string[] args) => cmdWithTry.Execute(args);

            CommandResult ExecutorWithoutTry(string[] args) => cmdWithoutTry.Execute(args);

            // ---------------------------------------------
            // CASE 1: Command IMPLEMENTS TryRun()
            // ---------------------------------------------
            var resultTry = extension.Invoke(
                cmdWithTry,
                new[] { "fileA" },       // extensionArgs
                ExecutorWithTry,         // executor
                new[] { "fileA" }        // commandArgs
            );

            Assert.IsTrue(resultTry.RequiresConfirmation);
            Assert.IsNotNull(resultTry.Feedback);
            Assert.IsTrue(resultTry.Message.Contains("[Preview-WithTry]"));

            // simulate user saying "yes"
            var confirmed1 = resultTry.Feedback!.Respond("yes");
            Assert.AreEqual("EXEC fileA", confirmed1.Message);

            // -----------------------------
            // CASE 2: Command without TryRun
            // -----------------------------
            var resultNoTry = extension.Invoke(
                cmdWithoutTry,
                new[] { "fileB" },
                ExecutorWithoutTry,
                new[] { "fileB" }        // commandArgs
            );

            Assert.IsTrue(resultNoTry.RequiresConfirmation);
            Assert.IsNotNull(resultNoTry.Feedback);
            Assert.IsTrue(resultNoTry.Message.Contains("[Preview-Fallback]"));

            // simulate user saying "no" (note: use resultNoTry.Feedback, not resultTry.Feedback)
            var confirmed2 = resultNoTry.Feedback!.Respond("no");

            Assert.IsFalse(confirmed2.Success);
            Assert.AreEqual("Execution cancelled by user.", confirmed2.Message);
        }
    }
}