/*
 * PROJECT:     Mediator.Rules
 * FILE:        DeadReferenceAnalyzerTests.cs
 * PURPOSE:     Tests for DeadReferenceAnalyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Reflection;
using Core.Apps.Rules;

namespace Mediator.Rules
{
    /// <summary>
    /// Dead Reference Tests
    /// </summary>
    [TestClass]
    public class DeadReferenceAnalyzerTests
    {
        private string _tempDir = null!;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _tempDir = AnalyzerTestHelper.CreateTempDirectory();
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            AnalyzerTestHelper.SafeDeleteDirectory(_tempDir);
        }

        /// <summary>
        /// Analyzes the via public API successfully processes csproj file.
        /// </summary>
        [TestMethod]
        public void Analyze_ViaPublicApi_SuccessfullyProcessesCsprojFile()
        {
            const string csprojXml = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <ProjectReference Include=""..\Weaver\Weaver.csproj"" />
    <PackageReference Include=""Microsoft.CodeAnalysis.CSharp"" Version=""4.14.0"" />
  </ItemGroup>
</Project>";
            var path = Path.Combine(_tempDir, "Sample.csproj");
            File.WriteAllText(path, csprojXml);

            var diagnostics = new DeadReferenceAnalyzer().Analyze(path, csprojXml).ToList();

            // Extension bug is fixed; analyzer now correctly parses the file and surfaces references via ProjectReferenceInfo
            Assert.AreEqual(2, diagnostics.Count);
            StringAssert.Contains(diagnostics[0].Message, @"..\Weaver\Weaver.csproj");
            StringAssert.Contains(diagnostics[1].Message, "Microsoft.CodeAnalysis.CSharp");
        }

        [TestMethod]
        public void GetUnusedReferences_ViaReflection_AlwaysReturnsEveryParsedReference()
        {
            const string csprojXml = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <ProjectReference Include=""..\Weaver\Weaver.csproj"" />
    <PackageReference Include=""Microsoft.CodeAnalysis.CSharp"" Version=""4.14.0"" />
  </ItemGroup>
</Project>";

            var unused = GetUnusedReferencesViaReflection(csprojXml).ToList();

            Assert.AreEqual(2, unused.Count);
            CollectionAssert.Contains(unused, @"..\Weaver\Weaver.csproj");
            CollectionAssert.Contains(unused, "Microsoft.CodeAnalysis.CSharp");
        }

        /// <summary>
        /// Gets the unused references via reflection.
        /// </summary>
        /// <param name="xmlContent">Content of the XML.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// ProjectReferenceInfo type not found - has it moved or been renamed?
        /// or
        /// GetUnusedReferences method not found - has it moved or been renamed?
        /// </exception>
        private static List<string> GetUnusedReferencesViaReflection(string xmlContent)
        {
            var type = typeof(DeadReferenceAnalyzer).Assembly
                           .GetType("Core.Apps.Helper.ProjectReferenceInfo")
                       ?? throw new InvalidOperationException(
                           "ProjectReferenceInfo type not found - has it moved or been renamed?");

            var instance = Activator.CreateInstance(
                type,
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                args: new object[] { xmlContent },
                culture: null);

            var method = type.GetMethod("GetUnusedReferences", BindingFlags.NonPublic | BindingFlags.Instance)
                         ?? throw new InvalidOperationException(
                             "GetUnusedReferences method not found - has it moved or been renamed?");

            var result = (IEnumerable<string>)method.Invoke(instance, null)!;
            return result.ToList();
        }
    }
}