# Weave

Weave is a lightweight C# command execution engine with support for namespaces, extensions, and user feedback handling. It provides a flexible way to register commands, manage execution, and handle interactive command flows.

---

## Features

- **Command Registration:** Register commands with optional namespace and parameter support.  
- **Extensions:** Apply extensions globally or per command (e.g., `.help`, `.tryrun`).  
- **Feedback Handling:** Supports interactive user prompts and confirmation flows.  
- **Mediator Integration:** Tracks pending feedback for commands, ensuring safe resolution and cleanup.  
- **Namespace Support:** Commands and extensions can be organized per namespace for modularity.  

---

## Usage

### Registering Commands

```csharp
var weave = new Weave();

var myCommand = new MyCommand();
weave.Register(myCommand);

```
## Processing Input

```csharp
var result = weave.ProcessInput("namespace:myCommand(arg1, arg2).help");
Console.WriteLine(result.Message);
```

## Handling Feedback

If a command requires confirmation or additional input, Weave will handle it automatically:

```csharp
if (result.RequiresConfirmation)
{
    // Next user input is routed automatically to the pending feedback
    var followUp = Console.ReadLine();
    var followUpResult = weave.ProcessInput(followUp);
}

```

##UML

```mermaid
classDiagram
    direction LR

    class Weave {
        - Dictionary<(string ns, string name, int paramCount), ICommand> _commands
        - Dictionary<(string ns, string name, int paramCount), Dictionary<string, int>> _commandExtensions
        - static Dictionary<string, CommandExtension> GlobalExtensions
        - List<ICommandExtension> _extensions
        - FeedbackRequest? _pendingFeedback
        - MessageMediator _mediator
        + Register(ICommand command)
        + RegisterExtension(ICommandExtension extension)
        + ProcessInput(string raw) CommandResult
        + FindCommand(string name, int argCount, string? ns)
        + FindExtension(ICommand command, string extensionName, int argCount)
        + Reset()
    }

    class ICommand {
        <<interface>>
        + string Namespace
        + string Name
        + int ParameterCount
        + Dictionary<string, int> Extensions
        + CommandResult Execute(string[] args)
        + CommandResult InvokeExtension(string extensionName, string[] args)
        + CommandResult? TryRun(string[] args)
    }

    class ICommandExtension {
        <<interface>>
        + string Name
        + string? Namespace
        + int ExtensionParameterCount
        + CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> next)
        + void BeforeExecute(ICommand command, string[]? args = null)
        + void AfterExecute(ICommand command, CommandResult result)
    }

    class CommandExtension {
        + string Name
        + int ParameterCount
        + bool IsInternal
        + bool IsPreview
    }

    class CommandResult {
        + bool Success
        + bool RequiresConfirmation
        + FeedbackRequest? Feedback
        + string Message
        + static CommandResult Fail(string message)
    }

    class FeedbackRequest {
        + Guid RequestId
        + bool IsPending
        + bool RequiresConfirmation
        + CommandResult Respond(string input)
    }

    class MessageMediator {
        + void Register(ICommand cmd, FeedbackRequest feedback)
        + ICommand? Resolve(Guid requestId)
        + void Clear(Guid requestId)
        + void ClearAll()
    }

    %% Relationships
    Weave --> ICommand : manages
    Weave --> ICommandExtension : loads & invokes
    Weave --> MessageMediator : mediates feedback
    Weave --> FeedbackRequest : tracks pending

    ICommand --> CommandResult : returns
    ICommandExtension --> CommandResult : returns
    FeedbackRequest --> CommandResult : produces

    %% Execution flow
    Weave ..> ICommandExtension : delegates execution
    ICommandExtension ..> ICommand : may invoke via executor
    ICommandExtension ..> FeedbackRequest : can request confirmation
    FeedbackRequest ..> ICommandExtension : resumes execution after user input
```

License

This project is licensed under the MIT License

