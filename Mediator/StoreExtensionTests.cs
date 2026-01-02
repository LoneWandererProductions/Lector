/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        StoreExtensionTests.cs
 * PURPOSE:     Tests for the .store() global extension.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weaver;
using Weaver.Messages;

namespace Mediator
{
    /// <summary>
    /// Tests for the StoreExtension global extension.
    /// </summary>
    [TestClass]
    public sealed class StoreExtensionTests
    {
        private Weave? _weave;

        [TestInitialize]
        public void Setup()
        {
            _weave = new Weave();
        }

        /// <summary>
        /// evaluate(2+3).store(total) stores result under provided key.
        /// </summary>
        [TestMethod]
        public void StoreWithCustomKeyStoresValueAndTypeCorrectly()
        {
            var result = _weave!.ProcessInput("evaluate(2+3).store(total)");

            Assert.IsTrue(result.Success);

            Assert.IsTrue(_weave.Runtime.Variables.TryGet("total", out var value, out var type));
            Assert.AreEqual(5.0, value);
            Assert.AreEqual(EnumTypes.Wdouble, type);
        }

        /// <summary>
        /// evaluate(7*3).store() stores under default key 'result'.
        /// </summary>
        [TestMethod]
        public void StoreWithoutKeyUsesDefaultResultKey()
        {
            var result = _weave!.ProcessInput("evaluate(7 * 3).store()");

            Assert.IsTrue(result.Success);

            Assert.IsTrue(_weave.Runtime.Variables.TryGet("result", out var value, out var type));
            Assert.AreEqual(21.0, value);
            Assert.AreEqual(EnumTypes.Wdouble, type);
        }

        /// <summary>
        /// Store preserves boolean type.
        /// </summary>
        [TestMethod]
        public void StorePreservesBooleanType()
        {
            var result = _weave!.ProcessInput("evaluate(5 >3).store(flag)");

            Assert.IsTrue(result.Success);

            Assert.IsTrue(_weave.Runtime.Variables.TryGet("flag", out var value, out var type));
            Assert.AreEqual(true, value);
            Assert.AreEqual(EnumTypes.Wbool, type);
        }
    }
}
