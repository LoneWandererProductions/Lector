/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.TryRun
 * FILE:        TryrunTests.cs
 * PURPOSE:     Mostly tests for TryRun extension behavior.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Mediator.Core;
using Weaver;
using Weaver.Core.Extensions;
using Weaver.Messages;

namespace Mediator.TryRun
{
    [TestClass]
    public class TryrunTests
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