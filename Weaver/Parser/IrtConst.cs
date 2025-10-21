/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Interpreter.Resources
* FILE:        Interpreter.Resources/IrtConst.cs
* PURPOSE:     The Command Constants that are delivered with the dll
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

namespace Weaver.Parser;

/// <summary>
///     IrtConst contains all strings class.
/// </summary>
internal static class IrtConst
{
    /// <summary>
    ///     Regex Pattern, Find periods that are not inside {} or (), (const). Value: @"\.(?![^{}]*\})(?![^(]*\))".
    /// </summary>
    internal const string RegexParenthesisOutsidePattern = @"\.(?![^{}]*\})(?![^(]*\))";

    /// <summary>
    ///     Regex Pattern to remove whitespace before '(' and '{', (const). Value: @"\s*([(){}])\s*".
    /// </summary>
    internal const string RegexParenthesisWellFormedPatternLeft = @"\s+(?=[({])";

    /// <summary>
    ///     Regex  Pattern to remove whitespace after ')' and '}', (const). Value:  @"(?&lt;=[)}])\s+".
    /// </summary>
    internal const string RegexParenthesisWellFormedPatternRight = @"(?<=[)}])\s+";

    /// <summary>
    ///     Regex  Pattern to remove all whitespace, (const). Value:  @"\s+".
    /// </summary>
    internal const string RegexRemoveWhiteSpace = @"\s+";

    /// <summary>
    ///     Separator (const). Value: ", ".
    /// </summary>
    internal const string Separator = " , ";

    /// <summary>
    ///     Empty Parameter (const). Value:  "()".
    /// </summary>
    internal const string EmptyParameter = "()";

    /// <summary>
    ///     The internal command container (const). Value: "CONTAINER".
    /// </summary>
    private const string InternalContainer = "CONTAINER";

    /// <summary>
    ///     The internal command batch execute (const). Value: "BATCHEXECUTE".
    /// </summary>
    private const string InternalBatchExecute = "BATCHEXECUTE";

    /// <summary>
    ///     The internal command print (const). Value: "PRINT".
    /// </summary>
    private const string InternalPrint = "PRINT";

    /// <summary>
    ///     The internal command confirm (const). Value: "CONFIRM".
    /// </summary>
    private const string InternalConfirm = "CONFIRM";

    /// <summary>
    ///     The internal command help (const). Value: "HELP".
    /// </summary>
    internal const string InternalCommandHelp = "HELP";

    /// <summary>
    ///     The internal command list (const). Value: "LIST".
    /// </summary>
    private const string InternalCommandList = "LIST";

    /// <summary>
    ///     The internal Namespace (const). Value: "INTERNAL".
    /// </summary>
    internal const string InternalNameSpace = "INTERNAL";

    /// <summary>
    ///     The internal command using (const). Value: "USING".
    /// </summary>
    private const string InternalUsing = "USING";

    /// <summary>
    ///     The internal extension command use (const). Value: "Use".
    /// </summary>
    private const string InternalExtensionUse = "USE";

    /// <summary>
    ///     The internal extension command help (const). Value: "HELP".
    /// </summary>
    private const string InternalHelpExtension = "HELP";

    /// <summary>
    ///     The internal command use (const). Value: "USE".
    /// </summary>
    private const string InternalUse = "USE";

    /// <summary>
    ///     The internal command Log (const). Value: "LOG".
    /// </summary>
    private const string InternalErrorLog = "LOG";

    /// <summary>
    ///     The internal command Log info (const). Value: "LOGINFO".
    /// </summary>
    private const string InternalLogInfo = "LOGINFO";

    /// <summary>
    ///     The internal command Log full (const). Value: "LOGFULL".
    /// </summary>
    private const string InternalLogFull = "LOGFULL";

    /// <summary>
    ///     The internal command if statement (const). Value: "IF".
    /// </summary>
    internal const string InternalIf = "IF";

    /// <summary>
    ///     The internal command else, followed after if (const). Value: "ELSE".
    /// </summary>
    internal const string InternalElse = "ELSE";

    /// <summary>
    ///     The internal command Goto (const). Value: "GOTO".
    /// </summary>
    private const string InternalGoto = "GOTO";

    /// <summary>
    ///     The internal command label, used by goto (const). Value: "LABEL".
    /// </summary>
    internal const string InternalLabel = "LABEL";

    /// <summary>
    ///     The internal command await feedback, used for user feedback (const). Value: "AWAITFEEDBACK".
    /// </summary>
    private const string InternalAwaitFeedback = "AWAITFEEDBACK";

    /// <summary>
    ///     The error UserSpace not Found (const). Value: "Error UserSpace not found".
    /// </summary>
    internal const string ErrorUserSpaceNotFound = "Error UserSpace not found";

    /// <summary>
    ///     The error not initialized (const). Value: "Error please initiate the Prompt first.".
    /// </summary>
    internal const string ErrorNotInitialized = "Error please initiate the Prompt first.";

    /// <summary>
    ///     The error File not found (const). Value: "Error please initiate the Prompt first.".
    /// </summary>
    internal const string ErrorFileNotFound = "Error please initiate the Prompt first.";

    /// <summary>
    ///     The error for Extensions (const). Value: "Extension provided produced Errors: ".
    /// </summary>
    internal const string ErrorExtensions = "Extension provided produced Errors: ";

    /// <summary>
    ///     The error for Invalid Input (const). Value: "Input was null or empty.".
    /// </summary>
    internal const string ErrorInvalidInput = "Input was null or empty.";

    /// <summary>
    ///     The error feedback for wrong input (const). Value: "Input was not valid.".
    /// </summary>
    internal const string ErrorFeedbackOptions = "Input was not valid.";

    /// <summary>
    /// The error feedback missing (const). Value: "Error, Feedback data is missing.".
    /// </summary>
    internal const string ErrorFeedbackMissing = "Error, Feedback data is missing.";

    /// <summary>
    ///     The error no feedback options (const). Value: "No Feedback Options were provided."
    /// </summary>
    internal const string ErrorNoFeedbackOptions = "No Feedback Options were provided.";

    /// <summary>
    ///     The error feedback Option not allowed (const). Value: "Option not allowed.".
    /// </summary>
    internal const string ErrorFeedbackOptionNotAllowed = "Option not allowed.";

    /// <summary>
    ///     The error Internal Extension not found (const). Value: "Unknown Internal extension command.".
    /// </summary>
    internal const string ErrorInternalExtensionNotFound = "Unknown Internal extension command.";

    /// <summary>
    ///     The feedback Message (const). Value: "You selected: ".
    /// </summary>
    internal const string FeedbackMessage = "You selected: ";

    /// <summary>
    ///     The feedback for cancel Command (const). Value:"Operation was cancelled. You can proceed."
    /// </summary>
    internal const string FeedbackCancelOperation = "Operation was cancelled. You can proceed.";

    /// <summary>
    ///     The feedback for yes Command (const). Value: "Operation was executed with yes."
    /// </summary>
    internal const string FeedbackOperationExecutedYes = "Operation was executed with yes.";

    /// <summary>
    ///     The feedback for yes Command (const). Value: "Operation was executed with no."
    /// </summary>
    internal const string FeedbackOperationExecutedNo = "Operation was executed with no.";

    /// <summary>
    ///     The parenthesis error (const). Value: "Wrong parenthesis".
    /// </summary>
    internal const string ParenthesisError = "Wrong parenthesis";

    /// <summary>
    ///     The key word not found error (const). Value: "error KeyWord not Found: ".
    /// </summary>
    internal const string KeyWordNotFoundError = "error KeyWord not Found: ";

    /// <summary>
    ///     The key word not found error (const). Value: "error KeyWord not Found: ".
    /// </summary>
    internal const string JumpLabelNotFoundError = "error jump label not found: ";

    /// <summary>
    ///     The syntax error (const). Value: "Error in the Syntax".
    /// </summary>
    internal const string SyntaxError = "Error in the Syntax: ";

    /// <summary>
    ///     The syntax error parameter count (const). Value: "Problems with the provided parameter count the and the expected
    ///     amount.".
    /// </summary>
    internal const string SyntaxErrorParameterCount =
        "Problems with the provided parameter count the and the expected amount.";

    /// <summary>
    ///     The message info (const). Value: "Information: ".
    /// </summary>
    internal const string MessageInfo = "Information: ";

    /// <summary>
    ///     The message error (const). Value: "Error: ".
    /// </summary>
    internal const string MessageError = "Error: ";

    /// <summary>
    ///     The message warning (const). Value: "Warning: ".
    /// </summary>
    internal const string MessageWarning = "Warning: ";

    /// <summary>
    ///     The message error count (const). Value: "Error Count: ".
    /// </summary>
    internal const string MessageErrorCount = "Error Count: ";

    /// <summary>
    ///     The message log Count (const). Value: "Log Count: ".
    /// </summary>
    internal const string MessageLogCount = "Log Count: ";

    /// <summary>
    ///     The message log statistics (const). Value: "General Information about the Log.".
    /// </summary>
    internal const string MessageLogStatistics = "General Information about the Log.";

    /// <summary>
    ///     The end (const). Value: ";".
    /// </summary>
    internal const string End = ";";

    /// <summary>
    ///     The Active (const). Value: "Active Using: ".
    /// </summary>
    internal const string Active = "Active Using: ";

    /// <summary>
    ///     The information startup (const). Value: "Interpreter started up".
    /// </summary>
    internal const string InformationStartup = "Interpreter started up";

    /// <summary>
    ///     The information Namespace switch (const). Value: "Namespace switched to: ".
    /// </summary>
    internal const string InformationNamespaceSwitch = "Namespace switched to: ";

    /// <summary>
    ///     The format description (const). Value: " Description: ".
    /// </summary>
    internal const string FormatDescription = " Description: ";

    /// <summary>
    ///     The format count (const). Value: " Parameter Count: ".
    /// </summary>
    internal const string FormatCount = " Parameter Count: ";

    /// <summary>
    ///     The open Clause, Standard is'('
    /// </summary>
    internal const char BaseOpen = '(';

    /// <summary>
    ///     The advanced open Clause, Standard is'{'
    /// </summary>
    internal const char AdvancedOpen = '{';

    /// <summary>
    ///     The close Clause, Standard is')'
    /// </summary>
    internal const char BaseClose = ')';

    /// <summary>
    ///     The advanced close Clause, Standard is'}'
    /// </summary>
    internal const char AdvancedClose = '}';

    /// <summary>
    ///     The Splitter Clause, Standard is','
    /// </summary>
    internal const char Splitter = ',';

    /// <summary>
    ///     The Splitter for a new Command, Standard is';'
    /// </summary>
    internal const char NewCommand = ';';

    /// <summary>
    ///     Indicator for comment, mostly used for batch files, (const). Value: "--".
    /// </summary>
    internal const string CommentCommand = "--";

    /// <summary>
    ///     The error. (const). Value: "-1".
    /// </summary>
    internal const int Error = -1;

    /// <summary>
    ///     The error used for input, that is not available for this command. (const). Value: "-2".
    /// </summary>
    internal const int ErrorOptionNotAvailable = -2;

    /// <summary>
    ///     The no split occurred. (const). Value: "2".
    /// </summary>
    internal const int NoSplitOccurred = 0;

    /// <summary>
    ///     The extension has a Parameter mismatch. (const). Value: "1".
    /// </summary>
    internal const int ParameterMismatch = 1;

    /// <summary>
    ///     The extension has a Parameter mismatch. (const). Value: "2".
    /// </summary>
    internal const int ParenthesisMismatch = 2;

    /// <summary>
    ///     The Internal extension found. (const). Value: "3".
    /// </summary>
    internal const int ExtensionFound = 3;

    /// <summary>
    ///     If Command  is Batch expression. (const). Value: "0".
    /// </summary>
    internal const int BatchCommand = 0;

    /// <summary>
    ///     If Command has Parameter. (const). Value: "1".
    /// </summary>
    internal const int ParameterCommand = 1;

    /// <summary>
    ///     Help with Parameter Id. (const). Value: "1".
    /// </summary>
    internal const int InternalHelpWithParameter = 1;

    /// <summary>
    ///     Important Command Id for Container. (const). Value: "8".
    /// </summary>
    internal const int InternalContainerId = 8;

    /// <summary>
    ///     Important Command Id for batch files. (const). Value: "9".
    /// </summary>
    internal const int InternalBatchId = 9;

    /// <summary>
    ///     The internal check, if Parameter is empty, since the brackets are expected. (const). Value: "()".
    /// </summary>
    internal static readonly string InternalEmptyParameter = string.Concat(BaseOpen, BaseClose);
}
