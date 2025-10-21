/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtParserInput.cs
 * PURPOSE:     Handle the Input for prompt and connect to the other modules
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Local

#nullable enable
using System.Diagnostics;

namespace Weaver.Parser;

/// <inheritdoc />
/// <summary>
///     Basic command Line Interpreter, bare bones for now
/// </summary>
internal sealed class IrtParserInput : IDisposable
{
    /// <summary>
    ///     Command Register
    /// </summary>
    private static Dictionary<int, InCommand>? _com;

    /// <summary>
    ///     Namespace of Commands
    /// </summary>
    private static string? _nameSpace;

    /// <summary>
    ///     Extension Command Register
    /// </summary>
    private static Dictionary<int, InCommand>? _extension;

    /// <summary>
    ///     My request identifier
    /// </summary>
    private readonly string _myRequestId;

    /// <summary>
    ///     The disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    ///     The original input string
    /// </summary>
    private string _inputString;

    /// <summary>
    ///     The IRT extension
    /// </summary>
    private IrtExtension _irtExtension;

    /// <summary>
    ///     Prevents a default instance of the <see cref="IrtParserInput" /> class from being created.
    /// </summary>
    private IrtParserInput()
    {
    }

    /// <inheritdoc />
    /// <summary>
    ///     Dispose of the resources used by the IrtPrompt.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Event to send selected command to the subscriber
    /// </summary>
    internal event EventHandler<string> SendInternalLog;

    /// <summary>
    ///     Get the engine running
    /// </summary>
    /// <param name="use">The command structure</param>
    internal void Initiate()
    {
        // Set the basic parameters

    }

    /// <summary>
    ///     Handles the input string and processes commands.
    /// </summary>
    /// <param name="inputString">Input string</param>
    internal void HandleInput(string inputString)
    {
        _inputString = inputString;

        if (string.IsNullOrWhiteSpace(inputString))
        {
            SetError(IrtConst.ErrorInvalidInput);
            return;
        }

        // Validate the input string
        if (!CleanAndCheckInputString(ref inputString))
        {
            return;
        }

        var dummy = new Dictionary<int, InCommand>();

        // Handle comment commands
        if (IsCommentCommand(inputString))
        {
            Trace.WriteLine(inputString);
            return;
        }

        // Check if the parentheses are correct
        if (!IrtKernel.SingleCheck(inputString))
        {
            SetErrorWithLog(IrtConst.ParenthesisError);
            return;
        }

        // Check for extensions in the internal namespace first, then in the external namespace if needed
        var extensionResult = _irtExtension.CheckForExtension(_inputString, IrtConst.InternalNameSpace,
            dummy);

        if (extensionResult.Status == IrtConst.Error)
        {
            extensionResult = _irtExtension.CheckForExtension(_inputString, _nameSpace, _extension);
        }

        // Process the extension result
        switch (extensionResult.Status)
        {
            case IrtConst.Error:
                return;

            case IrtConst.ParameterMismatch:
                return;

            case IrtConst.ExtensionFound:
                //TODO result is wrong.
                var command = ProcessInput(inputString, extensionResult.Extension);

                return;

            default:
                var com = ProcessInput(inputString);
                if (com != null)
                {
                }

                break;
        }
    }

    /// <summary>
    ///     Processes the input string.
    /// </summary>
    /// <param name="inputString">Input string</param>
    /// <param name="extension">Optional extension commands</param>
    internal OutCommand? ProcessInput(string inputString, ExtensionCommands? extension = null)
    {
        if (extension != null)
        {
            //remove the extension string, and handle only the base command.
            inputString = extension.BaseCommand;
        }
        var dummy = new Dictionary<int, InCommand>();

        var key = IrtKernel.CheckForKeyWord(inputString, dummy);

        key = IrtKernel.CheckForKeyWord(inputString, _com);
        if (key == IrtConst.Error)
        {
            SetErrorWithLog(IrtConst.KeyWordNotFoundError, _inputString);
            return null;
        }

        var (status, splitParameter) = IrtKernel.GetParameters(inputString, key, _com);
        var parameter = status == IrtConst.ParameterCommand
            ? IrtKernel.SplitParameter(splitParameter, IrtConst.Splitter)
            : new List<string> { splitParameter };

        var check = IrtKernel.CheckOverload(_com[key].Command, parameter.Count, _com);

        if (check != null)
        {
            return new OutCommand
            {
                Command = (int)check,
                Parameter = parameter,
                UsedNameSpace = _nameSpace,
                ExtensionCommand = extension
            };
        }

        SetErrorWithLog(IrtConst.SyntaxErrorParameterCount);

        return null;
    }

    /// <summary>
    ///     Cleans the input string.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>True if input string is cleaned and valid; otherwise false.</returns>
    private static bool CleanAndCheckInputString(ref string input)
    {
        input = IrtKernel.WellFormedParenthesis(input);
        var openParenthesis = new[] { IrtConst.BaseOpen, IrtConst.AdvancedOpen };
        var closeParenthesis = new[] { IrtConst.BaseClose, IrtConst.AdvancedClose };

        return IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);
    }

    /// <summary>
    ///     Determines whether the specified input is a comment command.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>
    ///     <c>true</c> if the specified input is a comment command; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsCommentCommand(string input)
    {
        return input.StartsWith(IrtConst.CommentCommand, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    ///     Determines whether the specified input is a help command.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>
    ///     <c>true</c> if the specified input is a help command; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsHelpCommand(string input)
    {
        input = input.Replace(IrtConst.BaseOpen.ToString(), string.Empty)
            .Replace(IrtConst.BaseClose.ToString(), string.Empty);
        return input.Equals(IrtConst.InternalCommandHelp, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    ///     Sets the error with log.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="additionalInfo">Additional information.</param>
    private void SetErrorWithLog(string errorMessage, string additionalInfo = "")
    {
    }

    /// <summary>
    ///     Sets the error status of the output command.
    /// </summary>
    /// <param name="error">The error message.</param>
    private void SetError(string error)
    {
        var com = new OutCommand
        {
            Command = IrtConst.Error,
            Parameter = null,
            UsedNameSpace = _nameSpace,
            ErrorMessage = error
        };
    }

    /// <summary>
    ///     Dispose the resources.
    /// </summary>
    /// <param name="disposing">
    ///     Indicates whether the method call comes from a Dispose method (true) or from a finalizer
    ///     (false).
    /// </param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        // Dispose unmanaged resources here if needed

        _disposed = true;
    }

    /// <summary>
    ///     Destructor to ensure the resources are released.
    /// </summary>
    ~IrtParserInput()
    {
        Dispose(false);
    }
}
