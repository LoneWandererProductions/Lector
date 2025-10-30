/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        UnusedLocalVariableAnalyzer.cs
 * PURPOSE:     Unused local variable Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using DiagnosticSeverity = CoreBuilder.Enums.DiagnosticSeverity;

namespace CoreBuilder;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Analyzer that finds unused local variables.
/// </summary>
public sealed class UnusedLocalVariableAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc />
    public string Name => "UnusedLocalVariable";

    /// <inheritdoc />
    public string Description => "Unused local variable Analyzer.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // 🔹 Ignore generated code and compiler artifacts
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var compilation = CSharpCompilation.Create("Analysis")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var model = compilation.GetSemanticModel(tree);
        var root = tree.GetRoot();

        foreach (var localDecl in root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            foreach (var variable in localDecl.Declaration.Variables)
            {
                if (variable.Identifier.Text == "_")
                    continue; // discard, don’t flag

                var symbol = model.GetDeclaredSymbol(variable);

                if (symbol is not ILocalSymbol localSymbol) continue;

                var references = root.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(id =>
                        SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, localSymbol));

                if (references.Any()) continue;

                var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, line,
                    $"Unused local variable '{variable.Identifier.Text}'.");
            }
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Fail("Missing argument: <path>\nUsage: unusedlocal <folder>");

        var path = args[0];
        if (!Directory.Exists(path))
            return CommandResult.Fail($"Directory not found: {path}");

        var files = Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)
            .Where(f => !CoreHelper.ShouldIgnoreFile(f))
            .ToList();

        if (files.Count == 0)
            return CommandResult.Ok("No C# files found to analyze.");

        var diagnostics = new List<Diagnostic>();

        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                diagnostics.AddRange(Analyze(file, content));
            }
            catch
            {
                // unreadable file - ignore silently
            }
        }

        if (diagnostics.Count == 0)
            return CommandResult.Ok("✅ No unused local variables found.");

        var msg = string.Join(Environment.NewLine,
            diagnostics.Select(d => $"{d.FilePath}:{d.LineNumber} -> {d.Message}")
        );

        return CommandResult.Ok(msg);
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}