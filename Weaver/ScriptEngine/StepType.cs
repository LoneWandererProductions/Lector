/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptExecutor.cs
 * PURPOSE:     Executes parsed script statements using Weave
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    internal enum StepType
    {
        Input = 0,
        Execute = 1,
        Output = 2,
        Internal = 3
    }
}