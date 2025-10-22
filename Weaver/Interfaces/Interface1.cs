/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        Interface1.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Interfaces
{
    /// <summary>
    /// Represents a generic input/output device for scripts.
    /// </summary>
    public interface IScriptIO
    {
        string ReadInput(string prompt);
        void WriteOutput(string message);
    }
}
