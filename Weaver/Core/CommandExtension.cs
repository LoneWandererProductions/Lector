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
