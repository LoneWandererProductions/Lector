/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        LicenseHeaderAnalyzer.cs
 * PURPOSE:     Analyzer to detect missing license headers in C# files.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using CoreBuilder.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Detects missing license headers at the start of C# files.
/// Implements ICommand for Weaver integration.
/// </summary>
public sealed class LicenseHeaderAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc />
    public string Name => "LicenseHeader";

    /// <inheritdoc />
    public string Description => "Simple analyzer that checks if a file starts with a license header.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        if (CoreHelper.ShouldIgnoreFile(filePath))
            yield break;

        // Trim leading whitespace
        var trimmed = fileContent.TrimStart();

        // Quick exit if there is no comment at all
        if (!(trimmed.StartsWith("/*") || trimmed.StartsWith("//")))
        {
            yield return new Diagnostic(
                Name,
                DiagnosticSeverity.Info,
                filePath,
                1,
                "Missing license header.");
            yield break;
        }

        // Extract the first comment block or line(s)
        string firstChunk;
        if (trimmed.StartsWith("/*"))
        {
            var endIdx = trimmed.IndexOf("*/", StringComparison.Ordinal);
            firstChunk = endIdx > 0 ? trimmed[..(endIdx + 2)] : trimmed;
        }
        else
        {
            var lines = trimmed
                .Split('\n')
                .TakeWhile(line => line.TrimStart().StartsWith("//"));
            firstChunk = string.Join("\n", lines);
        }

        // Normalize for simple keyword check
        var header = firstChunk.ToUpperInvariant();

        // Acceptable keywords to consider it a valid license header
        if (!(header.Contains("COPYRIGHT") || header.Contains("LICENSE")))
        {
            yield return new Diagnostic(
                Name,
                DiagnosticSeverity.Info,
                filePath,
                1,
                "Missing license header.");
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Executes the analyzer on a single file path.
    /// </summary>
    public CommandResult Execute(params string[] args)
    {
        return CoreHelper.Run(
            args,
            (filePath, content) => Analyze(filePath, content),
            Name
        );
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}