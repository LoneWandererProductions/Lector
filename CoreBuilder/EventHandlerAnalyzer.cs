﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        EventHandlerAnalyzer.cs
 * PURPOSE:     Analyzer that detects potential event handler leaks.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Enums;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DiagnosticSeverity = CoreBuilder.Enums.DiagnosticSeverity;

namespace CoreBuilder;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Check if Event is unsubscribed.
/// </summary>
/// <seealso cref="CoreBuilder.Interface.ICodeAnalyzer" />
public sealed class EventHandlerAnalyzer : ICodeAnalyzer

{
    /// <inheritdoc />
    public string Name => "EventHandler";

    /// <inheritdoc />
    public string Description => "Analyzer that detects potential event handler leaks.";

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
                DiagnosticSeverity.Warning,
                filePath,
                line,
                $"Event '{key}' subscribed {stats.Count} times so far. Check for corresponding unsubscribes.",
                DiagnosticImpact.IoBound
            );
        }
    }
}