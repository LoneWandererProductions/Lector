/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Helper
 * FILE:        CoreHelper.cs
 * PURPOSE:     Runner that executes analyzers over directories or single files, is used in the command Interface.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreBuilder.Helper
{
    /// <summary>
    /// Provides helper methods to execute analyzers over directories or single files.
    /// </summary>
    internal static class RunAnalyze
    {
        /// <summary>
        /// Runs the given analyzer against all C# source files in the specified directory.
        /// </summary>
        /// <param name="directory">The directory path to scan recursively.</param>
        /// <param name="analyzer">The analyzer instance implementing <see cref="ICodeAnalyzer"/>.</param>
        /// <returns>A collection of all diagnostics produced by the analyzer.</returns>
        internal static IEnumerable<Diagnostic> RunAnalyzer(string directory, ICodeAnalyzer analyzer)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(directory) || analyzer == null)
                return Enumerable.Empty<Diagnostic>();

            if (!Directory.Exists(directory))
                return Enumerable.Empty<Diagnostic>();

            var diagnostics = new List<Diagnostic>();

            // Search recursively for .cs files
            var files = Directory.GetFiles(directory, CoreResources.ResourceCsExtension, SearchOption.AllDirectories);

            foreach (var file in SafeEnumerateFiles(directory, CoreResources.ResourceCsExtension))
            {
                if (CoreHelper.ShouldIgnoreFile(file))
                    continue;

                var content = File.ReadAllText(file);
                diagnostics.AddRange(analyzer.Analyze(file, content));
            }

            return diagnostics;
        }

        /// <summary>
        /// Runs the analyzer on a single file, if it exists.
        /// </summary>
        /// <param name="filePath">The full path of the file to analyze.</param>
        /// <param name="analyzer">The analyzer instance implementing <see cref="ICodeAnalyzer"/>.</param>
        /// <returns>A collection of diagnostics for that file.</returns>
        internal static IEnumerable<Diagnostic> RunAnalyzerForFile(string filePath, ICodeAnalyzer analyzer)
        {
            if (string.IsNullOrWhiteSpace(filePath) || analyzer == null)
                return Enumerable.Empty<Diagnostic>();

            if (!File.Exists(filePath))
                return Enumerable.Empty<Diagnostic>();

            if (CoreHelper.ShouldIgnoreFile(filePath))
                return Enumerable.Empty<Diagnostic>();

            var content = File.ReadAllText(filePath);
            return analyzer.Analyze(filePath, content);
        }

        /// <summary>
        /// Recursively enumerates files while skipping inaccessible folders.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns>All Folders.</returns>
        private static IEnumerable<string> SafeEnumerateFiles(string root, string pattern)
        {
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                string current = stack.Pop();

                string[] files = Array.Empty<string>();
                try { files = Directory.GetFiles(current, pattern); }
                catch { }

                foreach (var f in files)
                    yield return f;

                string[] dirs = Array.Empty<string>();
                try { dirs = Directory.GetDirectories(current); }
                catch { }

                foreach (var d in dirs)
                    stack.Push(d);
            }
        }
    }
}