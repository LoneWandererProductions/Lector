namespace Weaver.Messages
{
    /// <summary>
    /// example: "delete(file.txt).saveTo(backupFolder)"
    /// returns ParsedCommand:
    /// Name = "delete", Args = ["file.txt"], Extension = "saveTo", ExtensionArgs = ["backupFolder"]
    /// </summary>
    public class ParsedCommand
    {
        public string Namespace { get; init; } = "global";
        public string Name { get; init; } = "";
        public string[] Args { get; init; } = Array.Empty<string>();
        public string? Extension { get; init; }
        public string[] ExtensionArgs { get; init; } = Array.Empty<string>();
    }
}
