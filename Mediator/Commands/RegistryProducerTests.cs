/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Commands
 * FILE:        RegistryProducerTests.cs
 * PURPOSE:     Tests the IRegistryProducer contract, WhoAmI generation, and the Clean extension pipeline.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps;
using Core.Apps.Extensions;
using Weaver.Core.Extensions;
using Weaver.Messages;
using Weaver.Registry;

namespace Mediator.Commands
{
    [TestClass]
    public class RegistryProducerTests
    {
        /// <summary>
        /// The registry
        /// </summary>
        private VariableRegistry _registry;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Initialize a fresh, empty registry before every test
            _registry = new VariableRegistry();
        }

        /// <summary>
        /// WhoAmI execute populates registry and returns key.
        /// </summary>
        [TestMethod]
        public void WhoAmI_Execute_PopulatesRegistryAndReturnsKey()
        {
            // Arrange
            var command = new WhoAmI(_registry);

            // Act
            var result = command.Execute();

            // Assert: Check the CommandResult contract
            Assert.IsTrue(result.Success, "Command should succeed.");
            Assert.AreEqual(EnumTypes.Wobject, result.Type, "Should return Wobject type.");
            Assert.AreEqual("whoami", result.Value, "Value payload should be the registry key.");

            // Assert: Check the actual Registry (The Heap)
            Assert.IsTrue(_registry.TryGetObject("whoami", out var storedObject),
                "Registry should contain the 'whoami' object.");
            Assert.IsNotNull(storedObject, "Stored object should not be null.");

            // Verify specific expected fields exist in the Wobject
            Assert.IsTrue(storedObject.ContainsKey("hostname"), "Wobject should contain 'hostname'.");
            Assert.IsTrue(storedObject.ContainsKey("ip"), "Wobject should contain 'ip'.");
            Assert.IsTrue(storedObject.ContainsKey("os"), "Wobject should contain 'os'.");
        }

        /// <summary>
        /// Who extension invoke filters data and updates registry.
        /// </summary>
        [TestMethod]
        public void WhoAmIExtension_Invoke_FiltersDataAndUpdatesRegistry()
        {
            // Arrange
            var command = new WhoAmI(_registry);
            var extension = new WhoAmIExtension();

            // Dummy executor (represents the next step in the pipeline, though the extension bypasses it here)
            CommandResult DummyExecutor(string[] args) => CommandResult.Ok("dummy");

            // Act: Call whoami().who(ip, hostname)
            var extensionArgs = new[] { "ip", "hostname" };
            var result = extension.Invoke(command, extensionArgs, DummyExecutor, Array.Empty<string>());

            // Assert: Check the output message
            Assert.IsTrue(result.Success, "Extension should succeed.");
            Assert.IsTrue(result.Message.Contains("Hostname:"), "Message should contain Hostname.");
            Assert.IsTrue(result.Message.Contains("IP:"), "Message should contain IP.");
            Assert.IsFalse(result.Message.Contains("OS:"), "Message should NOT contain OS, as it was not requested.");

            // Assert: Check the Registry to ensure the Wobject was updated safely
            Assert.IsTrue(_registry.TryGetObject("whoami", out var storedObject),
                "Registry should still contain the 'whoami' object.");
            Assert.IsTrue(storedObject.ContainsKey("ip"), "Registry Wobject should contain the filtered 'ip'.");
        }

        /// <summary>
        /// Cleans the extension invoke wipes registry data successfully.
        /// </summary>
        [TestMethod]
        public void CleanExtension_Invoke_WipesRegistryDataSuccessfully()
        {
            // Arrange
            var command = new WhoAmI(_registry);
            var cleanExtension = new CleanExtension();

            CommandResult DummyExecutor(string[] args) => CommandResult.Ok("dummy");

            // Step 1: Execute WhoAmI to populate the registry
            command.Execute();

            // Sanity Check: Ensure it is actually in the registry before we try to clean it
            Assert.IsTrue(_registry.TryGetObject("whoami", out _),
                "Setup failed: Registry should contain data before cleaning.");

            // Act: Invoke the .clean() extension
            var result = cleanExtension.Invoke(command, Array.Empty<string>(), DummyExecutor, Array.Empty<string>());

            // Assert: Verify the CleanExtension contract
            Assert.IsTrue(result.Success, "Clean extension should succeed.");
            Assert.AreEqual("whoami", result.Value, "Clean extension should return the key that was wiped.");

            // Assert: Verify the Registry (The Heap) is actually empty
            var exists = _registry.TryGetObject("whoami", out _);
            Assert.IsFalse(exists,
                "The 'whoami' key MUST be completely removed from the registry after .clean() is called.");
        }

        /// <summary>
        /// Cleans the extension invoke on non producer fails gracefully.
        /// </summary>
        [TestMethod]
        public void CleanExtension_Invoke_OnNonProducer_FailsGracefully()
        {
            // Arrange
            // We use IncCommand because we earlier decided it is NOT an IRegistryProducer
            var nonProducerCommand = new Weaver.Core.Commands.IncCommand(_registry);
            var cleanExtension = new CleanExtension();

            CommandResult DummyExecutor(string[] args) => CommandResult.Ok("dummy");

            // Act
            var result = cleanExtension.Invoke(nonProducerCommand, Array.Empty<string>(), DummyExecutor,
                Array.Empty<string>());

            // Assert
            Assert.IsFalse(result.Success, "Clean extension should fail if the command is not an IRegistryProducer.");
            Assert.IsTrue(result.Message.Contains("not supported") || result.Message.Contains("not implement"),
                "Should return a helpful error message explaining why it failed.");
        }
    }
}