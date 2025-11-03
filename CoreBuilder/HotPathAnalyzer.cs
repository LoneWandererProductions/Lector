/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        HotPathAnalyzer.cs
 * PURPOSE:     Analyzer that detects frequently called methods and flags hot paths.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Analyzer that detects method calls in hot paths (loops) and aggregates statistics.
/// </summary>
public sealed class HotPathAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc />
    public string Name => "HotPath";

    /// <inheritdoc />
    public string Description => "Analyzer that detects frequently called methods and flags hot paths.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <summary>
    /// Project-wide aggregation: method FQN -> (call count, total risk, files seen)
    /// </summary>
    private readonly Dictionary<string, (int Count, int TotalRisk, HashSet<string> Files)> _aggregateStats = new();

    // Thresholds / weights
    private const int ConstantLoopWeight = 10;
    private const int VariableLoopWeight = 20;
    private const int NestedLoopWeight = 50;

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

        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

        foreach (var invocation in invocations)
        {
            var symbolName = invocation.Expression.ToString();

            // Ignore framework / system calls
            if (symbolName.StartsWith("System.") ||
                symbolName.StartsWith("Microsoft.") ||
                symbolName.StartsWith("Path.") ||
                symbolName.StartsWith("File.") ||
                symbolName.StartsWith("Directory."))
                continue;

            var loopContext = CoreHelper.GetLoopContext(invocation);
            if (loopContext == LoopContext.None)
                continue;

            int risk = loopContext switch
            {
                LoopContext.ConstantBounded => ConstantLoopWeight,
                LoopContext.VariableBounded => VariableLoopWeight,
                LoopContext.Nested => NestedLoopWeight,
                _ => 1
            };

            if (!_aggregateStats.TryGetValue(symbolName, out var stats))
                stats = (0, 0, new HashSet<string>());

            stats.Count++;
            stats.TotalRisk += risk;
            stats.Files.Add(filePath);
            _aggregateStats[symbolName] = stats;

            var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            DiagnosticImpact impact = loopContext switch
            {
                LoopContext.ConstantBounded => DiagnosticImpact.CpuBound,
                LoopContext.VariableBounded => DiagnosticImpact.CpuBound,
                LoopContext.Nested => DiagnosticImpact.CpuBound,
                _ => DiagnosticImpact.Other
            };

            yield return new Diagnostic(
                Name,
                DiagnosticSeverity.Info,
                filePath,
                line,
                $"{BuildMessage(symbolName, loopContext)} Called {stats.Count} times so far, current risk {risk}, total risk {stats.TotalRisk}.",
                impact
            );
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (_aggregateStats.Count == 0)
            return CommandResult.Fail("No hot paths detected. Run analysis first.");

        var sb = new StringBuilder();
        sb.AppendLine("🔥 Hot Path Summary:");
        sb.AppendLine(new string('-', 50));

        foreach (var kvp in _aggregateStats.OrderByDescending(k => k.Value.TotalRisk))
        {
            var (method, data) = (kvp.Key, kvp.Value);
            sb.AppendLine(
                $"{method}: {data.Count} calls, total risk {data.TotalRisk}, files [{string.Join(", ", data.Files)}]");
        }

        return CommandResult.Ok(
            message: $"Hot path analysis complete. {_aggregateStats.Count} unique methods detected.",
            value: sb.ToString(),
            type: EnumTypes.Wstring
        );
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }

    /// <summary>
    /// Builds the message.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="ctx">The CTX.</param>
    /// <returns>Concated message.</returns>
    private static string BuildMessage(string method, LoopContext ctx)
    {
        return ctx switch
        {
            LoopContext.ConstantBounded => $"Method '{method}' inside constant-bounded loop.",
            LoopContext.VariableBounded => $"Method '{method}' inside variable-bounded loop.",
            LoopContext.Nested => $"Method '{method}' inside nested loops.",
            _ => $"Method '{method}' inside loop."
        };
    }
}