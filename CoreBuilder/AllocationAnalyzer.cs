﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        AllocationAnalyzer.cs
 * PURPOSE:     Analyzer that detects allocations in hot paths.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Analyzer that detects allocations in hot paths.
/// </summary>
/// <seealso cref="T:CoreBuilder.Interface.ICodeAnalyzer" />
public sealed class AllocationAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc />
    public string Name => "Allocation";

    /// <inheritdoc />
    public string Description => "Analyzer that detects allocations in hot paths.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <summary>
    /// The aggregate stats
    /// </summary>
    private readonly Dictionary<string, (int Count, int TotalRisk, HashSet<string> Files)> _aggregateStats = new();

    /// <summary>
    /// The constant loop weight
    /// </summary>
    private const int ConstantLoopWeight = 10;

    /// <summary>
    /// The variable loop weight
    /// </summary>
    private const int VariableLoopWeight = 20;

    /// <summary>
    /// The nested loop weight
    /// </summary>
    private const int NestedLoopWeight = 50;

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetRoot();

        foreach (var alloc in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            // Ignore trivial .NET classes like string
            var typeName = alloc.Type.ToString();
            if (typeName == "string") continue;

            var loopContext = CoreHelper.GetLoopContext(alloc);
            if (loopContext == LoopContext.None) continue;

            int risk = loopContext switch
            {
                LoopContext.ConstantBounded => ConstantLoopWeight,
                LoopContext.VariableBounded => VariableLoopWeight,
                LoopContext.Nested => NestedLoopWeight,
                _ => 1
            };

            if (!_aggregateStats.TryGetValue(typeName, out var stats))
                stats = (0, 0, new HashSet<string>());

            stats.Count++;
            stats.TotalRisk += risk;
            stats.Files.Add(filePath);
            _aggregateStats[typeName] = stats;

            var line = alloc.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            yield return new Diagnostic(
                Name,
                DiagnosticSeverity.Info,
                filePath,
                line,
                $"Allocation of '{typeName}' inside loop. Called {stats.Count} times, current risk {risk}, total risk {stats.TotalRisk}.",
                DiagnosticImpact.CpuBound
            );
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
        => CoreHelper.Run(args, Analyze, Name);

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}
