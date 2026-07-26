/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mediator.Rules
 * FILE:        AnalyzerTestHelper.cs
 * PURPOSE:     Shared helpers for testing Core.Apps.Rules analyzers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Mediator.Rules
{
    /// <summary>
    /// Creates and cleans up temporary .cs files/directories so analyzer tests can call
    /// Analyze(filePath, fileContent) against real files on disk.
    /// </summary>
    internal static class AnalyzerTestHelper
    {
        /// <summary>
        /// Creates a fresh, uniquely named temp directory with no ancestor .csproj.
        /// Analyzers that walk up looking for a project root (e.g.
        /// DuplicateStringLiteralAnalyzer.FindProjectRoot) fall back to this directory
        /// itself instead of scanning unrelated files elsewhere on disk.
        /// </summary>
        public static string CreateTempDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), $"weave-rules-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Writes <paramref name="content"/> to a uniquely named .cs file inside
        /// <paramref name="directory"/> and returns the full path.
        /// </summary>
        public static string CreateTempCsFile(string content, string directory)
        {
            var path = Path.Combine(directory, $"{Guid.NewGuid():N}.cs");
            File.WriteAllText(path, content);
            return path;
        }

        /// <summary>
        /// Best-effort recursive delete; a leftover temp folder shouldn't fail the test run.
        /// </summary>
        public static void SafeDeleteDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
            catch
            {
                // ignored - best effort cleanup only
            }
        }
    }
}
