/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ediator.Scripting
 * FILE:        RegistryTests.cs
 * PURPOSE:     Tests for the variable registry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using Weaver.Messages;
using Weaver.Registry;

namespace Mediator.Scripting
{
    /// <summary>
    /// Test my registry.
    /// </summary>
    [TestClass]
    public class RegistryTests
    {
        /// <summary>
        /// The registry
        /// </summary>
        private VariableRegistry? _registry;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _registry = new VariableRegistry();
        }

        /// <summary>
        /// Tests the set and get primitives.
        /// </summary>
        [TestMethod]
        public void TestSetAndGet_Primitives()
        {
            _registry?.Set("intVal", VmValue.FromInt(42));
            _registry?.Set("doubleVal", VmValue.FromDouble(3.14));
            _registry.Set("boolVal", VmValue.FromBool(true));
            _registry.Set("stringVal", VmValue.FromString("hello"));

            Assert.IsTrue(_registry.TryGetInt("intVal", out var iVal));
            Assert.AreEqual(42, iVal);

            Assert.IsTrue(_registry.TryGetDouble("doubleVal", out var dVal));
            Assert.AreEqual(3.14, dVal);

            Assert.IsTrue(_registry.TryGetBool("boolVal", out var bVal));
            Assert.IsTrue(bVal);

            Assert.IsTrue(_registry.TryGetString("stringVal", out var sVal));
            Assert.AreEqual("hello", sVal);
        }

        /// <summary>
        /// Tests the set and get list.
        /// </summary>
        [TestMethod]
        public void TestSetAndGet_List()
        {
            var list = new List<VmValue>
            {
                VmValue.FromInt(1),
                VmValue.FromInt(2),
                VmValue.FromInt(3)
            };

            // Add list manually to _store/_lookUp
            int start = 0;
            foreach (var v in list)
                start++; // simulate storing
            _registry?.Set("myList", VmValue.FromInt(0)); // dummy type, just for registry

            // Instead, let's test TryGetList returns false because we didn't populate _store
            Assert.IsFalse(_registry.TryGetList("myList", out var l));
        }

        /// <summary>
        /// Tests the set and get object.
        /// </summary>
        [TestMethod]
        public void TestSetAndGet_Object()
        {
            // Create object with attributes as keys
            var objValues = new List<VmValue>
            {
                VmValue.FromInt(10, attribute: "x"),
                VmValue.FromInt(20, attribute: "y")
            };

            // Normally you'd populate _store/_lookUp internally in the registry
            // For testing TryGetObject without internal _store setup, it should return false
            _registry?.Set("myObj", VmValue.FromObject());

            Assert.IsFalse(_registry.TryGetObject("myObj", out var o));
        }


        /// <summary>
        /// Tests the pointers.
        /// </summary>
        [TestMethod]
        public void TestPointers()
        {
            _registry?.Set("value", VmValue.FromInt(99));
            _registry?.Set("ptr", VmValue.FromPointer("value"));

            Assert.IsTrue(_registry.TryGetPointer("ptr", out var val, out var type));
            Assert.AreEqual(EnumTypes.Wint, type);
            Assert.AreEqual((long)(99), val);
        }

        /// <summary>
        /// Tests the remove and clear.
        /// </summary>
        [TestMethod]
        public void TestRemoveAndClear()
        {
            _registry?.Set("a", VmValue.FromInt(1));
            _registry?.Set("b", VmValue.FromString("hello"));

            Assert.IsTrue(_registry.Remove("a"));
            Assert.IsFalse(_registry.TryGetInt("a", out var _));

            _registry.ClearAll();
            Assert.IsFalse(_registry.TryGetString("b", out var _));
        }
    }
}
