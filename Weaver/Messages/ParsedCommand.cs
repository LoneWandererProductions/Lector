/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        ParsedCommand.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Messages
{
    /// <summary>
    /// Represents the result of parsing a raw command input.
    /// example: "delete(file.txt).saveTo(backupFolder)"
    /// returns ParsedCommand:
    /// Name = "delete", Args = ["file.txt"], Extension = "saveTo", ExtensionArgs = ["backupFolder"]
    /// </summary>
    public sealed class ParsedCommand
    {
        /// <summary>
        /// The optional namespace of the command (e.g. "system").
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// The command name (e.g. "echo").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The arguments for the main command.
        /// </summary>
        public string[] Args { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The optional extension name (e.g. "help").
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// The arguments for the extension, if any.
        /// </summary>
        public string[] ExtensionArgs { get; set; } = Array.Empty<string>();
    }

}
