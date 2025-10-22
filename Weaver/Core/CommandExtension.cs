/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        CommandExtension.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Core
{
    public sealed class CommandExtension
    {
        public string Name { get; init; } = "";
        public int ParameterCount { get; init; }
        public bool IsInternal { get; init; }
        public bool IsPreview { get; init; } // for tryrun
    }
}