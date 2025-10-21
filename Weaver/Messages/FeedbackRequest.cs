namespace Weaver.Messages
{
    public sealed class FeedbackRequest
    {
        public string RequestId { get; init; } = Guid.NewGuid().ToString();
        public string Prompt { get; init; } = "";
        public string[] Options { get; init; } = Array.Empty<string>();
        public bool ExpectExactMatch { get; init; } = true; // optional hint for frontend
    }
}
