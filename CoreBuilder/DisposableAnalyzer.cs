/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        DisposableAnalyzer.cs
 * PURPOSE:     Analyzer that detects undisposed IDisposable objects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Analyzer that detects undisposed IDisposable objects.
/// </summary>
/// <seealso cref="CoreBuilder.Interface.ICodeAnalyzer" />
public sealed class DisposableAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Name => "DisposableLeak";

    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Description => "Analyzer that detects undisposed IDisposable objects.";

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
        var root = tree.GetRoot();

        var declarations = root.DescendantNodes()
            .OfType<VariableDeclarationSyntax>()
            .Where(v => v.Type is IdentifierNameSyntax id && ImplementsIDisposable(id.Identifier.Text));

        foreach (var decl in declarations)
        {
            foreach (var v in decl.Variables)
            {
                // check if inside using statement
                if (!IsDisposed(v, root))
                {
                    var line = v.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    yield return new Diagnostic(
                        Name,
                        Enums.DiagnosticSeverity.Warning,
                        filePath,
                        line,
                        $"'{v.Identifier.Text}' implements IDisposable but is not disposed. Risk of resource leak.",
                        DiagnosticImpact.IoBound
                    );
                }
            }
        }
    }

    //
    /// <summary>
    ///  Dummy check for demonstration; could be extended with semantic model
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>
    ///   <c>true</c> if the specified resource is disposed; otherwise, <c>false</c>.
    /// </returns>
    private static bool ImplementsIDisposable(string typeName)
    {
        return typeName.EndsWith("Stream") || typeName.EndsWith("Reader") || typeName.EndsWith("Writer");
    }

    /// <summary>
    /// Determines whether the specified variable is disposed.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <param name="root">The root.</param>
    /// <returns>
    ///   <c>true</c> if the specified variable is disposed; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsDisposed(VariableDeclaratorSyntax variable, SyntaxNode root)
    {
        // Simplified: check for using block
        var usingStatements = root.DescendantNodes().OfType<UsingStatementSyntax>();
        foreach (var u in usingStatements)
        {
            if (u.Declaration?.Variables.Any(v => v.Identifier.Text == variable.Identifier.Text) ?? false)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length < 1)
            return CommandResult.Fail("Missing argument: file path.");

        var filePath = args[0];
        if (!File.Exists(filePath))
            return CommandResult.Fail($"File not found: {filePath}");

        try
        {
            var fileContent = File.ReadAllText(filePath);
            var diagnostics = Analyze(filePath, fileContent).ToList();

            if (!diagnostics.Any())
                return CommandResult.Ok("No disposable leaks detected.");

            var sb = new StringBuilder();
            sb.AppendLine($"Disposable leak analysis for: {filePath}");
            sb.AppendLine(new string('-', 80));
            foreach (var diag in diagnostics)
                sb.AppendLine(diag.ToString());
            sb.AppendLine(new string('-', 80));
            sb.AppendLine($"{diagnostics.Count} potential disposable leaks found.");

            return CommandResult.Ok(sb.ToString(), diagnostics);
        }
        catch (Exception ex)
        {
            return CommandResult.Fail($"Analyzer execution failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}