/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator
 * FILE:        SimpleCommandParserTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.ParseEngine;

namespace Mediator
{
    [TestClass]
    public class SimpleCommandParserTests
    {
        [TestMethod]
        public void Parse_SimpleCommand_NoArgs_NoExtension()
        {
            const string input = "echo()";
            var result = SimpleCommandParser.Parse(input);

            Assert.AreEqual(string.Empty, result.Namespace);
            Assert.AreEqual("echo", result.Name);
            CollectionAssert.AreEqual(Array.Empty<string>(), result.Args);
            Assert.AreEqual(string.Empty, result.Extension);
            CollectionAssert.AreEqual(Array.Empty<string>(), result.ExtensionArgs);
        }

        [TestMethod]
        public void Parse_CommandWithArgs()
        {
            const string input = "copy(file1.txt, file2.txt)";
            var result = SimpleCommandParser.Parse(input);

            Assert.AreEqual("copy", result.Name);
            CollectionAssert.AreEqual(new[] { "file1.txt", "file2.txt" }, result.Args);
        }

        [TestMethod]
        public void Parse_NamespacedCommandWithExtension()
        {
            const string input = "system:delete(file.txt).log('backup')";
            var result = SimpleCommandParser.Parse(input);

            Assert.AreEqual("system", result.Namespace);
            Assert.AreEqual("delete", result.Name);
            CollectionAssert.AreEqual(new[] { "file.txt" }, result.Args);
            Assert.AreEqual("log", result.Extension);
            CollectionAssert.AreEqual(new[] { "backup" }, result.ExtensionArgs);
        }

        [TestMethod]
        public void Parse_ArgumentsWithQuotes()
        {
            const string input = "rename(\"old.txt\", \"new.txt\")";
            var result = SimpleCommandParser.Parse(input);

            CollectionAssert.AreEqual(new[] { "old.txt", "new.txt" }, result.Args);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parse_InvalidSyntax_Throws()
        {
            SimpleCommandParser.Parse("invalid(");
        }

        [TestMethod]
        public void Parse_CommandWithEmptyExtensionArgs()
        {
            const string input = "build().run()";
            var result = SimpleCommandParser.Parse(input);

            Assert.AreEqual("build", result.Name);
            Assert.AreEqual("run", result.Extension);
            CollectionAssert.AreEqual(Array.Empty<string>(), result.ExtensionArgs);
        }
    }
}