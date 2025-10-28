/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        DoubleNewlineAnalyzer.cs
 * PURPOSE:     Simple Double Newline Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using CoreBuilder.Interface;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder;

/// <inheritdoc />
/// <summary>
///     Finds double line breaks.
/// </summary>
/// <seealso cref="T:CoreBuilder.ICodeAnalyzer" />
public sealed class DoubleNewlineAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc />
    public string Name => "DoubleNewline";

    /// <inheritdoc />
    public string Description => "Simple Double Newline Analyzer.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new CommandSignature(Namespace, Name, ParameterCount);

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // Skip ignored files
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        var lines = fileContent.Split('\n');
        for (var i = 1; i < lines.Length - 1; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]) && string.IsNullOrWhiteSpace(lines[i - 1]))
            {
                yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, i + 1,
                    "Multiple blank lines in a row.");
            }
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Fail("Usage: DoubleNewline <filePath>");

        var filePath = args[0];
        if (!File.Exists(filePath))
            return CommandResult.Fail($"File not found: {filePath}");

        var content = File.ReadAllText(filePath);
        var results = Analyze(filePath, content).ToList();

        return FormatResult(results, Path.GetFileName(filePath));
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }

    /// <summary>
    /// Formats diagnostic results into a CommandResult.
    /// </summary>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="target">The target.</param>
    /// <returns>Return result of our Input</returns>
    private static CommandResult FormatResult(List<Diagnostic> diagnostics, string target)
    {
        if (diagnostics.Count == 0)
            return CommandResult.Ok($"No issues found in {target}.");

        // Your Diagnostic already has a perfect ToString(), so we just join lines.
        var output = new StringBuilder()
            .AppendLine($"Found {diagnostics.Count} issue(s) in {target}:")
            .AppendJoin('\n', diagnostics.Select(d => d.ToString()))
            .ToString();

        return CommandResult.Ok(output);
    }
}
