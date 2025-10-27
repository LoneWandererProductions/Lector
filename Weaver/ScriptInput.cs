/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        ScriptInput.cs
 * PURPOSE:     Script input handler with feedback support.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;

namespace Weaver
{
    /// <summary>
    /// Provides a simple console-based IO device for scripts.
    /// </summary>
    public sealed class ConsoleScriptIo : IScriptIo
    {
        public string ReadInput(string prompt)
        {
            Console.Write(prompt + " ");
            return Console.ReadLine() ?? string.Empty;
        }

        public void WriteOutput(string message)
        {
            Console.WriteLine(message);
        }
    }
}