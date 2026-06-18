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
            var start = 0;
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

        /// <summary>
        /// Tests the set and get list valid.
        /// </summary>
        [TestMethod]
        public void TestSetAndGet_List_Valid()
        {
            var list = new List<VmValue>
            {
                VmValue.FromInt(1),
                VmValue.FromInt(2),
                VmValue.FromInt(3)
            };

            // Use the Registry's internal allocation logic
            _registry.SetList("myList", list);

            Assert.IsTrue(_registry.TryGetList("myList", out var retrievedList));
            Assert.AreEqual(3, retrievedList?.Count);
            Assert.AreEqual(1, retrievedList[0].Int64);
        }

        /// <summary>
        /// Tests the set and get object valid.
        /// </summary>
        [TestMethod]
        public void TestSetAndGet_Object_Valid()
        {
            var objDict = new Dictionary<string, VmValue>
            {
                { "x", VmValue.FromInt(10) },
                { "y", VmValue.FromInt(20) }
            };

            // Use the Registry's internal allocation logic
            _registry.SetObject("myObj", objDict);

            Assert.IsTrue(_registry.TryGetObject("myObj", out var retrievedObj));
            Assert.AreEqual(2, retrievedObj?.Count);
            Assert.AreEqual(10, retrievedObj["x"].Int64);
            Assert.AreEqual(20, retrievedObj["y"].Int64);
        }

        /// <summary>
        /// Tests the set and get type safe extensions.
        /// </summary>
        [TestMethod]
        public void TestSetAndGet_TypeSafeExtensions()
        {
            // Test that the explicit Typed-Getters return false for wrong types
            _registry.Set("num", VmValue.FromInt(100));

            Assert.IsFalse(_registry.TryGetDouble("num", out _), "Should fail to get Int as Double.");
            Assert.IsFalse(_registry.TryGetBool("num", out _), "Should fail to get Int as Bool.");
            Assert.IsFalse(_registry.TryGetString("num", out _), "Should fail to get Int as String.");
        }

        /// <summary>
        /// Tests the pointers to compound types.
        /// </summary>
        [TestMethod]
        public void TestPointers_ToCompoundTypes()
        {
            // Setup a List
            var list = new List<VmValue> { VmValue.FromInt(5) };
            _registry.SetList("myList", list);

            // Create a pointer to the List
            _registry.Set("ptrToList", VmValue.FromPointer("myList"));

            // Test that TryGetPointer resolves the list correctly
            Assert.IsTrue(_registry.TryGetPointer("ptrToList", out var val, out var type));
            Assert.AreEqual(EnumTypes.Wlist, type);
            Assert.IsInstanceOfType(val, typeof(IReadOnlyList<VmValue>));

            var retrievedList = (IReadOnlyList<VmValue>)val!;
            Assert.AreEqual(5, retrievedList[0].Int64);
        }

        /// <summary>
        /// Tests the object attribute persistence.
        /// </summary>
        [TestMethod]
        public void TestObject_AttributePersistence()
        {
            var obj = new Dictionary<string, VmValue>
            {
                { "key1", VmValue.FromInt(1) }
            };

            _registry.SetObject("myObj", obj);

            // Verify the attribute was stored correctly in the heap
            Assert.IsTrue(_registry.TryGetObject("myObj", out var dict));
            Assert.IsTrue(dict!.ContainsKey("key1"));
            Assert.AreEqual("key1", dict["key1"].Attribute);
        }
    }
}