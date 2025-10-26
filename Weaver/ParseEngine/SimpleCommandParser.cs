/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ParseEngine
 * FILE:        SimpleCommandParser.cs
 * PURPOSE:     Parse simple commands with optional namespaces and extensions.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text.RegularExpressions;
using Weaver.Messages;

namespace Weaver.ParseEngine
{
    /// <summary>
    /// Provides parsing functionality for commands with optional namespaces and extensions.
    /// Syntax: [namespace:]command(arg1, arg2).extension(arg1, arg2)
    /// </summary>
    public static class SimpleCommandParser
    {
        /// <summary>
        /// Regex pattern for matching command blocks like cmd(arg1,arg2)
        /// </summary>
        private static readonly Regex CommandPattern = new(
            @"^(?<cmd>[a-zA-Z_][\w]*)\s*(\((?<args>[^()]*)\))?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Parses a raw command string into a <see cref="ParsedCommand" /> structure.
        /// </summary>
        /// <param name="raw">The raw input text.</param>
        /// <returns>
        /// The parsed command.
        /// </returns>
        /// <exception cref="System.FormatException">
        /// Empty input.
        /// or
        /// Missing command name.
        /// or
        /// Invalid command syntax near '{cmdPart}'.
        /// or
        /// Invalid extension syntax near '{segments[1]}'.
        /// </exception>
        /// <exception cref="FormatException">Thrown if the syntax is invalid.</exception>
        public static ParsedCommand Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new FormatException("Empty input.");

            // Split off namespace and command.ext structure
            raw = raw.Trim();

            // Split by '.' only if not inside parentheses
            var segments = SplitOutsideParentheses(raw, '.');

            if (segments.Count == 0)
                throw new FormatException("Missing command name.");

            // Handle namespace:command
            var mainPart = segments[0];
            string? ns = null;
            var cmdPart = mainPart;

            var nsSplit = mainPart.Split(':', 2);
            if (nsSplit.Length == 2)
            {
                ns = nsSplit[0].Trim();
                cmdPart = nsSplit[1].Trim();
            }

            // Parse main command
            var mainMatch = CommandPattern.Match(cmdPart);
            if (!mainMatch.Success)
                throw new FormatException($"Invalid command syntax near '{cmdPart}'.");

            var cmd = mainMatch.Groups["cmd"].Value;
            var argString = mainMatch.Groups["args"].Value;
            var args = ParseArgs(argString);

            // Parse optional extension
            var ext = string.Empty;
            var extArgs = Array.Empty<string>();

            if (segments.Count > 1)
            {
                var extMatch = CommandPattern.Match(segments[1]);
                if (!extMatch.Success)
                    throw new FormatException($"Invalid extension syntax near '{segments[1]}'.");

                ext = extMatch.Groups["cmd"].Value;
                extArgs = ParseArgs(extMatch.Groups["args"].Value);
            }

            return new ParsedCommand
            {
                Namespace = ns ?? string.Empty,
                Name = cmd,
                Args = args,
                Extension = ext,
                ExtensionArgs = extArgs
            };
        }

        /// <summary>
        /// Splits a string by a separator only when outside of parentheses.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Parse by Parenthese</returns>
        private static List<string> SplitOutsideParentheses(string input, char separator)
        {
            var parts = new List<string>();
            var depth = 0;
            var start = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if (c == separator && depth == 0)
                {
                    parts.Add(input[start..i].Trim());
                    start = i + 1;
                }
            }

            if (start < input.Length)
                parts.Add(input[start..].Trim());

            return parts;
        }

        /// <summary>
        /// Parses an argument list string into an array.
        /// Handles commas and quoted arguments.
        /// </summary>
        /// <param name="argString">The argument string.</param>
        /// <returns>Splits string into parts</returns>
        private static string[] ParseArgs(string argString)
        {
            if (string.IsNullOrWhiteSpace(argString))
                return Array.Empty<string>();

            // Split by commas outside quotes
            var args = Regex.Matches(argString, @"(?:""[^""]*""|'[^']*'|[^,]+)")
                .Select(m => m.Value.Trim().Trim('"', '\''))
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToArray();

            return args;
        }
    }
}