/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        CommandResult.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Messages
{
    public sealed class CommandResult
    {
        public string Message { get; init; } = "";
        public bool Success { get; set; }
        public bool RequiresConfirmation { get; init; }
        public string[]? Suggestions { get; init; }

        public static CommandResult Ok(string msg) => new() { Success = true, Message = msg };
        public static CommandResult Fail(string msg) => new() { Success = false, Message = msg };

        public bool RequiresFeedback => Feedback != null;
        public FeedbackRequest? Feedback { get; init; }
    }
}