/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        AnalyzerFactory.cs
 * PURPOSE:     Return all available code analyzers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Interface;
using CoreBuilder.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Weaver.Interfaces;

namespace CoreBuilder
{
    public static class AnalyzerFactory
    {
        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <returns>All commands.</returns>
        public static IReadOnlyList<ICommand> GetCommands()
        {
            ICommand[] modules =
            {
                new DirectorySizeAnalyzer(),
                new HeaderExtractor(),
                new ResXtract(),
                new AllocationAnalyzer(),
                new DisposableAnalyzer(),
                new DoubleNewlineAnalyzer(),
                new DuplicateStringLiteralAnalyzer(),
                new EventHandlerAnalyzer(),
                new HotPathAnalyzer(),
                new LicenseHeaderAnalyzer(),
                new UnusedClassAnalyzer(),
                new UnusedConstantAnalyzer(),
                new UnusedLocalVariableAnalyzer(),
                new UnusedParameterAnalyzer(),
                new UnusedPrivateFieldAnalyzer(),
                new DocCommentCoverageCommand(),
                new DeadReferenceAnalyzer(),
                new ApiExplorerCommand()
            };

            return modules; 
        }

        /// <summary>
        /// Gets all analyzers.
        /// </summary>
        /// <returns>All Code Analyzers</returns>
        public static IReadOnlyList<ICodeAnalyzer> GetAllAnalyzers()
        {
            ICodeAnalyzer[] modules =
            {
                new AllocationAnalyzer(),
                new DisposableAnalyzer(),
                new DoubleNewlineAnalyzer(),
                new DuplicateStringLiteralAnalyzer(),
                new EventHandlerAnalyzer(),
                new HotPathAnalyzer(),
                new LicenseHeaderAnalyzer(),
                new UnusedClassAnalyzer(),
                new UnusedConstantAnalyzer(),
                new UnusedLocalVariableAnalyzer(),
                new UnusedParameterAnalyzer(),
                new UnusedPrivateFieldAnalyzer(),
                new DocCommentCoverageCommand(),
                new DeadReferenceAnalyzer()
            };

            return modules;
        }

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <param name="userspace">The userspace.</param>
        /// <returns>All commands by Namespace.</returns>
        public static IReadOnlyList<ICommand> GetCommands(string userspace)
        {
            ICommand[] modules =
            {
        new DirectorySizeAnalyzer(),
        new HeaderExtractor(),
        new ResXtract(),
        new AllocationAnalyzer(),
        new DisposableAnalyzer(),
        new DoubleNewlineAnalyzer(),
        new DuplicateStringLiteralAnalyzer(),
        new EventHandlerAnalyzer(),
        new HotPathAnalyzer(),
        new LicenseHeaderAnalyzer(),
        new UnusedClassAnalyzer(),
        new UnusedConstantAnalyzer(),
        new UnusedLocalVariableAnalyzer(),
        new UnusedParameterAnalyzer(),
        new UnusedPrivateFieldAnalyzer(),
        new DocCommentCoverageCommand(),
        new DeadReferenceAnalyzer(),
        new ApiExplorerCommand()
    };

            // Filter by Namespace
            return modules
                .Where(m => string.Equals(m.Namespace, userspace, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}