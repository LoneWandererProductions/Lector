/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        ConResources.cs
 * PURPOSE:     Namespaces and Commands for my command line Tool
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder;

/// <summary>
///     Resource File
/// </summary>
internal static class ConResources
{
    /// <summary>
    ///     The user space fore code
    /// </summary>
    internal const string UserSpaceCode = "CodeUtilities";

    /// <summary>
    ///     The header
    /// </summary>
    internal const int Header = 0;

    /// <summary>
    ///     The resxtract
    /// </summary>
    internal const int Resxtract = 1;

    /// <summary>
    ///     The resxtract overload
    /// </summary>
    internal const int ResXtractOverload = 2;

    /// <summary>
    ///     The analyzer
    /// </summary>
    internal const int Analyzer = 3;

    /// <summary>
    ///     The dir analyzer
    /// </summary>
    internal const int DirAnalyzer = 4;

    /// <summary>
    ///     The resource1
    /// </summary>
    internal const string ResourceHeader = "header";

    /// <summary>
    /// The resource event processing completed.
    /// </summary>
    internal const string ResourceEventProcessing = "Event processing completed.";

    /// <summary>
    /// The resource cs extension
    /// </summary>
    internal const string ResourceCsExtension = "*.cs";

    /// <summary>
    /// The list command
    /// </summary>
    internal const string ResourceListCmd = "list";

    internal const string ResourceInput = "Enter something: ";

    internal const string ResourceResxtract = "resxtract";

    internal const string ResourceResxtractOutput = "Resxtract operation completed successfully: {0} created.{1}";

    internal const string ResourceEventWait = "Event is processing. Please wait...";

    /// <summary>
    /// The using command
    /// </summary>
    internal const string ResourceUsingCmd = "using";

    internal const string ErrorDirectory = "Error: Directory path '{0}' does not exist.";

    internal const string ErrorProjectPath = "Error: The project path '{0}' does not exist.";

    internal const string ErrorDirectoryOutput =
        "Error: The directory for output resource file '{0}' does not exist.";

    internal const string ErrorAccessFile = "Error accessing output resource file: {0}";

    internal const string MessageClose = "The application will close after a short delay.";

    internal const string ErrorCommandNotFound = "Error: Command not found.";

    internal const string InformationDirectoryMissing = "Directory path is required.";

    internal const string ErrorProjectPathMissing = "Error: Project path is required.";

    internal const string ResxtractFinished = "Resxtract operation completed: No string literals found to extract.";

    internal const string MessageSeparator = "  - ";

    internal const string Quotes = "\"";

    internal const string MessageError = "Error";

    internal const string MessageFilesIgnored = "Loaded {0} files to ignore.";

    internal const string ErrorRegexpattern = "Error loading regex pattern: {0}. Exception: {1}";

    internal const string MessageOutputIgnore = "Loaded {0} ignore patterns.";

    internal const string MessageChangedFiles = "Changed files:{0}  - {1}";

    internal const string InformationInvalidArgument = "Invalid arguments or operation.";

    internal const string MessageInfo = "Core Console Application";

    internal const string HeaderTryrunNoChanges = "Header try run, no results.";

    internal const string HeaderTryrunWouldAffect = "Header try run would affect: ";

    internal const string ResxtractTryrunWouldAffect = "Resxtract try run, would affect: ";
}
