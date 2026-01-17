/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Commands
 * FILE:        RegistryCommandTests.cs
 * PURPOSE:     Some basic test cases for SetValueCommand and GetValueCommand.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core.Commands;
using Weaver.Evaluate;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Mediator.Commands
{
    /// <summary>
    /// Check evaluate command tests.
    /// </summary>
    [TestClass]
    public class RegistryCommandTests
    {
        /// <summary>
        /// The registry
        /// </summary>
        private IVariableRegistry _registry = null!;

        /// <summary>
        /// The evaluator
        /// </summary>
        private ExpressionEvaluator _evaluator;

        /// <summary>
        /// The set value
        /// </summary>
        private SetValueCommand _setValue = null!;

        /// <summary>
        /// The get value
        /// </summary>
        private GetValueCommand _getValue = null!;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _registry = new VariableRegistry(); // or your concrete registry
            _evaluator = new ExpressionEvaluator(_registry);
            _setValue = new SetValueCommand(_registry, _evaluator);
            _getValue = new GetValueCommand(_registry);
        }

        /// <summary>
        /// Tests the set value and get value int.
        /// </summary>
        [TestMethod]
        public void TestSetValueAndGetValue_Int()
        {
            var setResult = _setValue.Execute("counter", "42", "Wint");
            Assert.IsTrue(setResult.Success);
            StringAssert.Contains(setResult.Message, "Registered");

            var getResult = _getValue.Execute("counter");
            Assert.IsTrue(getResult.Success);
            Assert.AreEqual((long)42, getResult.Value);
            Assert.AreEqual(EnumTypes.Wint, getResult.Type);
        }

        /// <summary>
        /// Tests the set value and get value string.
        /// </summary>
        [TestMethod]
        public void TestSetValueAndGetValue_String()
        {
            var setResult = _setValue.Execute("name", "Alice", "Wstring");
            Assert.IsTrue(setResult.Success);

            var getResult = _getValue.Execute("name");
            Assert.IsTrue(getResult.Success);
            Assert.AreEqual("Alice", getResult.Value);
            Assert.AreEqual(EnumTypes.Wstring, getResult.Type);
        }

        /// <summary>
        /// Tests the get value non existing key.
        /// </summary>
        [TestMethod]
        public void TestGetValue_NonExistingKey()
        {
            var getResult = _getValue.Execute("doesNotExist");
            Assert.IsFalse(getResult.Success);
            StringAssert.Contains(getResult.Message, "not found");
            Assert.AreEqual(EnumTypes.Wstring, getResult.Type); // default type for missing keys
            Assert.IsNull(getResult.Value);
        }

        /// <summary>
        /// Tests the type of the set value invalid.
        /// </summary>
        [TestMethod]
        public void TestSetValue_InvalidType()
        {
            var result = _setValue.Execute("x", "123", "UnknownType");
            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "Unknown type");
        }

        /// <summary>
        /// Tests incrementing an existing integer value.
        /// </summary>
        [TestMethod]
        public void TestIncrement_IntValue()
        {
            // Set initial value
            var setResult = _setValue.Execute("counter", "10", "Wint");
            Assert.IsTrue(setResult.Success);

            // Get current value
            var getResult = _getValue.Execute("counter");
            Assert.IsTrue(getResult.Success);
            Assert.AreEqual((long)10, getResult.Value);

            // Increment value
            var newValue = (long)getResult.Value + 1;
            var incrementResult = _setValue.Execute("counter", newValue.ToString(), "Wint");
            Assert.IsTrue(incrementResult.Success);

            // Verify increment
            var finalResult = _getValue.Execute("counter");
            Assert.IsTrue(finalResult.Success);
            Assert.AreEqual((long)11, finalResult.Value);
            Assert.AreEqual(EnumTypes.Wint, finalResult.Type);
        }
    }
}