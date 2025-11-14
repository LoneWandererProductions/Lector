/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        DocCommentCoverageCommand.cs
 * PURPOSE:     Checks code for XML documentation comment coverage.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Command that checks code for XML documentation comment coverage.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class DocCommentCoverageCommand : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Namespace => "analysis";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "doccoverage";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Reports the percentage of public members with XML doc comments.";

        /// <inheritdoc />
        public int ParameterCount => 1; // e.g., path to assembly or source folder

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var symbol = ReflectionHelper.GetSymbol(typeDecl); // pseudo: Syntax → TypeInfo
                if (!ReflectionHelper.HasXmlDoc(symbol))
                {
                    var line = typeDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"Type '{typeDecl.Identifier.Text}' is missing XML documentation.",
                        DiagnosticImpact.Readability
                    );
                }

                foreach (var member in typeDecl.Members)
                {
                    var memberSymbol = ReflectionHelper.GetSymbol(member);
                    if (!ReflectionHelper.HasXmlDoc(memberSymbol))
                    {
                        var line = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Info,
                            filePath,
                            line,
                            $"Member '{member.GetName()}' is missing XML documentation.",
                            DiagnosticImpact.Readability
                        );
                    }
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Please provide a folder path.");

            var folder = args[0];
            if (!Directory.Exists(folder))
                return CommandResult.Fail($"Folder '{folder}' does not exist.");

            int total = 0, documented = 0;

            foreach (var file in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                if (CoreHelper.ShouldIgnoreFile(file))
                    continue;

                var content = File.ReadAllText(file);
                var diagnostics = Analyze(file, content);

                // Count total public types/members
                var tree = CSharpSyntaxTree.ParseText(content);
                var root = tree.GetRoot();
                foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
                {
                    total++; // count type
                    if (!ReflectionHelper.HasXmlDoc(ReflectionHelper.GetSymbol(typeDecl)))
                        continue; // not documented
                    documented++;

                    foreach (var member in typeDecl.Members)
                    {
                        total++; // count member
                        if (ReflectionHelper.HasXmlDoc(ReflectionHelper.GetSymbol(member)))
                            documented++;
                    }
                }
            }

            double percent = total == 0 ? 0 : (documented * 100.0 / total);
            return CommandResult.Ok($"Doc comment coverage: {percent:F1}% ({documented}/{total})");
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail($"'{Name}' has no extensions.");
    }

}
