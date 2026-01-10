/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Commands
 * FILE:        RegistryCommandTests.cs
 * PURPOSE:     Some basic test cases for SetValueCommand and GetValueCommand.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core.Commands;
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
            _setValue = new SetValueCommand(_registry);
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
            Assert.AreEqual(42, getResult.Value);
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
        /// Tests the set value invalid int.
        /// </summary>
        [TestMethod]
        public void TestSetValue_InvalidInt()
        {
            var result = _setValue.Execute("score", "abc", "Wint");
            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "Invalid int value");
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
    }

}
