/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        MessageMediator.cs
 * PURPOSE:     Mediates messages and feedback requests between commands and the script executor.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;

namespace Weaver.Messages
{
    public sealed class MessageMediator
    {
        private readonly Dictionary<string, ICommand> _pending = new(StringComparer.OrdinalIgnoreCase);

        public void Register(ICommand command, FeedbackRequest feedback)
        {
            if (feedback == null)
                throw new ArgumentNullException(nameof(feedback));

            _pending[feedback.RequestId] = command;
        }

        public ICommand? Resolve(string requestId)
        {
            return _pending.TryGetValue(requestId, out var cmd) ? cmd : null;
        }

        public void Clear(string requestId)
        {
            _pending.Remove(requestId);
        }

        public void ClearAll()
        {
            _pending.Clear();
        }
    }
}

