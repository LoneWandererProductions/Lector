/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        IScriptIo.cs
 * PURPOSE:     Interface declaration for script input/output devices.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Interfaces
{
    /// <summary>
    /// Represents a generic input/output device for scripts.
    /// </summary>
    public interface IScriptIo
    {
        /// <summary>
        /// Reads the input.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <returns>Get user input</returns>
        string ReadInput(string prompt);

        /// <summary>
        /// Writes the output.
        /// </summary>
        /// <param name="message">The message.</param>
        void WriteOutput(string message);
    }
}