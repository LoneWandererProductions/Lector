/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        HeaderExtractor.cs
 * PURPOSE:     Inserts or detects license headers in C# source files.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Interface;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder;

/// <inheritdoc />
/// <summary>
///     Tool for detecting and inserting standardized license headers
///     into C# source files. Integrates with Weaver’s command and
///     extension infrastructure.
/// </summary>
public sealed class HeaderExtractor : IHeaderExtractor, ICommand
{
    /// <summary>
    ///     Defines the header template with placeholders for file info.
    /// </summary>
    private const string HeaderTemplate = @"/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     {0}
 * FILE:        {1}
 * PURPOSE:     {2}
 * PROGRAMMER:  {3}
 */
";

    /// <inheritdoc />
    public string Name => "HeaderExtractor";

    /// <inheritdoc />
    public string Description => "Detects and inserts standardized license headers into C# files.";

    /// <inheritdoc />
    public string Namespace => "Formatter";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <inheritdoc />
    /// <summary>
    ///     Scans a directory and inserts headers where missing.
    /// </summary>
    public string ProcessFiles(string? directoryPath, bool includeSubdirectories)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            return "Invalid or missing directory path.";

        var log = new StringBuilder();
        var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var file in Directory.GetFiles(directoryPath, "*.cs", searchOption))
        {
            try
            {
                if (CoreHelper.ShouldIgnoreFile(file))
                {
                    log.AppendLine($"Skipping {file} (auto-generated or excluded).");
                    continue;
                }

                var fileContent = File.ReadAllText(file);
                if (ContainsHeader(fileContent))
                {
                    log.AppendLine($"Header already exists in {file}.");
                    continue;
                }

                var updatedContent = InsertHeader(fileContent, Path.GetFileName(file), "Your file purpose here",
                    "Peter Geinitz (Wayfarer)");
                File.WriteAllText(file, updatedContent);

                log.AppendLine($"Header inserted in {file}");
            }
            catch (Exception ex)
            {
                log.AppendLine($"Failed to process {file}: {ex.Message}");
            }
        }

        return log.ToString();
    }

    /// <inheritdoc />
    public string DetectFilesNeedingHeaders(string? directoryPath, bool includeSubdirectories)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            return string.Empty;

        var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var csFiles = Directory.GetFiles(directoryPath, "*.cs", searchOption);

        var needingHeaders =
            (from file in csFiles
                where !CoreHelper.ShouldIgnoreFile(file)
                let content = File.ReadAllText(file)
                where !ContainsHeader(content)
                select Path.GetFullPath(file)).ToList();

        return needingHeaders.Count > 0
            ? string.Join(Environment.NewLine, needingHeaders)
            : "All files already contain headers.";
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Fail("Missing argument: directory path.");

        var directoryPath = args[0];
        var includeSubdirs = args.Length > 1 && bool.TryParse(args[1], out var result) && result;

        var resultMessage = ProcessFiles(directoryPath, includeSubdirs);
        return CommandResult.Ok(resultMessage);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Provides a preview execution mode when invoked via "tryrun".
    ///     This allows users to see which files would be modified before confirming.
    /// </summary>
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        if (!string.Equals(extensionName, "tryrun", StringComparison.OrdinalIgnoreCase))
            return CommandResult.Fail($"Extension '{extensionName}' not supported by '{Name}'.");

        if (args.Length == 0)
            return CommandResult.Fail("Missing argument: directory path.");

        var directoryPath = args[0];
        var includeSubdirs = args.Length > 1 && bool.TryParse(args[1], out var result) && result;

        var previewList = DetectFilesNeedingHeaders(directoryPath, includeSubdirs);
        if (string.IsNullOrWhiteSpace(previewList))
            return CommandResult.Ok("All files already contain headers. Nothing to insert.");

        // Create feedback request for confirmation
        FeedbackRequest? feedback = null;
        var cache = feedback;

        feedback = new FeedbackRequest(
            prompt:
            $"The following files are missing headers:\n\n{previewList}\n\nProceed with header insertion? (yes/no)",
            options: new[] { "yes", "no" },
            onRespond: input =>
            {
                input = input.Trim().ToLowerInvariant();
                return input switch
                {
                    "yes" => Execute(args), // Run the actual insertion
                    "no" => CommandResult.Fail("Operation cancelled by user."),
                    _ => new CommandResult
                    {
                        Message = "Please answer yes/no.",
                        RequiresConfirmation = true,
                        Feedback = cache // Reuse feedback for retry
                    }
                };
            });

        return new CommandResult
        {
            Message = "Preview of header insertion complete. Awaiting user confirmation.",
            Feedback = feedback,
            RequiresConfirmation = true,
            Success = false
        };
    }

    /// <summary>
    /// Determines whether the specified content contains header.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>
    ///   <c>true</c> if the specified content contains header; otherwise, <c>false</c>.
    /// </returns>
    private static bool ContainsHeader(string content)
    {
        return content.Split('\n').Select(line => line.Trim().ToLowerInvariant()).Any(trimmed =>
            trimmed.Contains("copyright", StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Extracts the namespace.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The namepsace in use.</returns>
    private static string ExtractNamespace(string content)
    {
        foreach (var parts in from line in content.Split('\n')
                 select line.Trim()
                 into trimmed
                 where trimmed.StartsWith("namespace ", StringComparison.InvariantCultureIgnoreCase)
                 select trimmed.Split(new[] { ' ', '{' }, StringSplitOptions.RemoveEmptyEntries))
        {
            return parts.Length > 1 ? parts[1] : "UnknownNamespace";
        }

        return "UnknownNamespace";
    }

    /// <summary>
    /// Inserts the header.
    /// </summary>
    /// <param name="fileContent">Content of the file.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="purpose">The purpose.</param>
    /// <param name="programmerName">Name of the programmer.</param>
    /// <returns>Add the new Header to the file</returns>
    private static string InsertHeader(string fileContent, string fileName, string purpose, string programmerName)
    {
        var namespaceName = ExtractNamespace(fileContent);
        var header = string.Format(HeaderTemplate, namespaceName, fileName, purpose, programmerName);
        return string.Concat(header, Environment.NewLine, fileContent);
    }
}