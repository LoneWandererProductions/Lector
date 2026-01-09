/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        RegistryCommandTests.cs
 * PURPOSE:     Some basic test cases for SetValueCommand and GetValueCommand.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Mediator
{
    [TestClass]
    public class RegistryCommandTests
    {
        private IVariableRegistry _registry = null!;
        private SetValueCommand _setValue = null!;
        private GetValueCommand _getValue = null!;

        [TestInitialize]
        public void Setup()
        {
            _registry = new VariableRegistry(); // or your concrete registry
            _setValue = new SetValueCommand(_registry);
            _getValue = new GetValueCommand(_registry);
        }

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

        [TestMethod]
        public void TestGetValue_NonExistingKey()
        {
            var getResult = _getValue.Execute("doesNotExist");
            Assert.IsFalse(getResult.Success);
            StringAssert.Contains(getResult.Message, "not found");
            Assert.AreEqual(EnumTypes.Wstring, getResult.Type); // default type for missing keys
            Assert.IsNull(getResult.Value);
        }

        [TestMethod]
        public void TestSetValue_InvalidInt()
        {
            var result = _setValue.Execute("score", "abc", "Wint");
            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "Invalid int value");
        }

        [TestMethod]
        public void TestSetValue_InvalidType()
        {
            var result = _setValue.Execute("x", "123", "UnknownType");
            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "Unknown type");
        }
    }

}
