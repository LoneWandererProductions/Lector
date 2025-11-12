/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        UnusedParameterAnalyzer.cs
 * PURPOSE:     Unused parameter Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using DiagnosticSeverity = CoreBuilder.Enums.DiagnosticSeverity;

namespace CoreBuilder.Rules;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Analyzer that finds unused method parameters.
/// </summary>
public sealed class UnusedParameterAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Name => "UnusedParameter";

    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Description => "Analyzer that finds unused method parameters.";

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
            yield break;

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var compilation = CSharpCompilation.Create("Analysis")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var model = compilation.GetSemanticModel(tree);
        var root = tree.GetRoot();

        foreach (var methodDecl in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            // Skip abstract/interface methods (no body to analyze)
            if (methodDecl.Body == null && methodDecl.ExpressionBody == null)
                continue;

            foreach (var parameter in methodDecl.ParameterList.Parameters)
            {
                var symbol = model.GetDeclaredSymbol(parameter);
                if (symbol is not { } paramSymbol)
                    continue;

                var references = methodDecl.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(id =>
                        SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, paramSymbol));

                if (references.Any())
                    continue;

                var line = parameter.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                yield return new Diagnostic(Name, DiagnosticSeverity.Warning, filePath, line,
                    $"Unused parameter '{parameter.Identifier.Text}' in method '{methodDecl.Identifier.Text}'.");
            }
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length < 1)
            return CommandResult.Fail("Missing argument: path");

        var path = args[0];

        // If a single file was passed, analyze that file only
        IEnumerable<Diagnostic> diagnosticsEnumerable;
        if (File.Exists(path))
        {
            diagnosticsEnumerable = RunAnalyze.RunAnalyzerForFile(path, this);
        }
        else if (Directory.Exists(path))
        {
            // Analyze all .cs files under the directory (RunAnalyzer handles ignore rules)
            diagnosticsEnumerable = RunAnalyze.RunAnalyzer(path, this);
        }
        else
        {
            return CommandResult.Fail($"Path not found: {path}");
        }

        var diagnostics = diagnosticsEnumerable.ToList();

        if (diagnostics.Count == 0)
            return CommandResult.Ok("No unused parameters found.");

        var output = string.Join("\n", diagnostics.Select(d =>
                         $"{d.FilePath}({d.LineNumber}): {d.Message}")) +
                     $"\nTotal: {diagnostics.Count} unused parameters.";

        return CommandResult.Ok(output);
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}