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
