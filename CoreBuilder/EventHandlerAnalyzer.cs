/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        EventHandlerAnalyzer.cs
 * PURPOSE:     Analyzer that detects potential event handler leaks.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
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

namespace CoreBuilder;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Check if Event is unsubscribed.
/// </summary>
/// <seealso cref="CoreBuilder.Interface.ICodeAnalyzer" />
public sealed class EventHandlerAnalyzer : ICodeAnalyzer, ICommand

{
    /// <inheritdoc />
    public string Name => "EventHandler";

    /// <inheritdoc />
    public string Description => "Analyzer that detects potential event handler leaks.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <summary>
    /// The event stats
    /// </summary>
    private readonly Dictionary<string, (int Count, HashSet<string> Files)> _eventStats = new();

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

        // Find event subscriptions (+=)
        var subscriptions = root.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(a => a.IsKind(SyntaxKind.AddAssignmentExpression));

        foreach (var sub in subscriptions)
        {
            if (sub.Left is not IdentifierNameSyntax eventName) continue;

            var key = eventName.Identifier.Text;
            if (!_eventStats.TryGetValue(key, out var stats))
                stats = (0, new HashSet<string>());

            stats.Count++;
            stats.Files.Add(filePath);
            _eventStats[key] = stats;

            var line = sub.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            yield return new Diagnostic(
                Name,
                Enums.DiagnosticSeverity.Warning,
                filePath,
                line,
                $"Event '{key}' subscribed {stats.Count} times so far. Check for corresponding unsubscribes.",
                DiagnosticImpact.IoBound
            );
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length < ParameterCount)
            return CommandResult.Fail($"Usage: {Namespace}.{Name} <path>");

        var path = args[0];
        if (!File.Exists(path))
            return CommandResult.Fail($"File not found: {path}");

        var content = File.ReadAllText(path);
        var diagnostics = Analyze(path, content).ToList();

        if (diagnostics.Count == 0)
            return CommandResult.Ok("No potential event handler leaks detected.");

        var message = string.Join("\n", diagnostics.Select(d => d.ToString()));
        return CommandResult.Ok(message, diagnostics);
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}