# Weave

Weave is a lightweight C# command execution engine with support for namespaces, extensions, and user feedback handling. It provides a flexible way to register commands, manage execution, and handle interactive command flows.

---

## Features

- **Command Registration:** Register commands with optional namespace and parameter support. Overload commands based on parameter count.
- **Extensions:** Apply extensions globally or per command (e.g., `.help`, `.tryrun`).  
- **Feedback Handling:** Supports interactive user prompts and confirmation flows.  
- **Mediator Integration:** Tracks pending feedback for commands, ensuring safe resolution and cleanup.  
- **Namespace Support:** Commands and extensions can be organized per namespace for modularity.  

---

## Usage

## Weave Script Syntax

Weave supports a simple, consistent syntax for commands and optional extensions. Commands can take zero or more parameters, and extensions can be chained after the command.  

### Basic Syntax

- `CommandName` – The name of a registered command (required).  
- `param1 ... paramN` – Command parameters (optional, depending on the command).  
- `.ExtensionName(...)` – Optional extension for the command. Extensions modify or enhance command behavior.  

### Examples

```csharp
// Simple command without parameters
help()
// or with, some commands are global and shipped with the engine, the rest is user-defined
print("Hello, World!")

// Command with parameters, in this case script engine command to set a variable
setValue("counter", 1, Wint)

// Command with an extension, displays help information for the setValue command
setValue("counter", 1, Wint).help

// Multiple commands can be executed sequentially
setValue("score", 100, Wint)
// Retrieve the value of the "score" variable
getValue("score")
// Display all variables in memory
memory()
// Delete the "score" variable from memory
deleteValue("score")
// memory is now empty
memory()
```

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

### Handling Feedback

Some commands may require confirmation or additional input. Weave handles this automatically: the command execution will internally pause and repeatedly request input until the proper response is provided. You can simply call `ProcessInput` and handle the resulting message:  

```csharp
// Execute a command with optional namespace and extension
var result = weave.ProcessInput("namespace:myCommand(arg1, arg2).tryrun()");

// The engine ensures feedback is handled internally, repeating until resolved
Console.WriteLine(result.Message);
```

# Weaver Script Engine

Weaver Script Engine is a lightweight C# script execution engine built on top of the **Weave** command framework.  
It supports parsing structured scripts, variable management, conditional execution, loops, labels, and interactive feedback handling.

---

## Features

- **Script Parsing:** Supports labels, commands, assignments, `if` conditions, `do...while` loops, and `goto`.
- **Variable Management:** Built-in type-safe variable registry for `Wint`, `Wdouble`, `Wbool`, and `Wstring`.
- **Conditional Execution:** Simple expression evaluator for `if` statements and loop conditions.
- **Looping:** Supports `do { ... } while(condition)` with proper loop handling.
- **Goto Labels:** Jump between script labels for flexible execution flow.
- **Feedback Handling:** Scripts can pause for user input, confirmation, or interactive prompts.
- **Debugger-Friendly:** Includes a `DebugHelpers` utility to flatten nodes and inspect scripts.

---

## Installation

Add the Script Engine project to your solution and reference it in your application.

```csharp
using Weaver.ScriptEngine;
```

## Script Syntax Reference

| Syntax                     | Description |
|-----------------------------|-------------|
| `label <Name>;`             | Defines a label to jump to using `goto`. |
| `goto <Label>;`             | Jumps execution to the specified label. |
| `<Command>(arg1, arg2, ...);` | Executes a registered command with optional arguments. |
| `do { ... } while(<condition>);` | Executes a loop; runs the block at least once and repeats while condition evaluates to true. |
| `if (<condition>) { ... } else { ... }` | Conditional execution based on the evaluation of a simple expression. |
| `setValue(<key>, <value>, <type>);` | Assigns a value to a variable (`Wint`, `Wdouble`, `Wbool`, `Wstring`). |
| `getValue(<key>);`          | Retrieves a variable value. |
| `deleteValue(<key>);`       | Deletes a variable from the registry. |
| `memory();`                  | Lists all current variables in the registry (debug). |

**Notes:**  
- `<condition>` currently supports simple expressions like `score > 100`, `flag == true`, etc.  
- Commands must be registered in the `Weave` instance before use.  
- Loops and conditionals are evaluated with the `ExpressionEvaluator` and `VariableRegistry`.  

### Evaluate Command

The `evaluate` command is a versatile utility in **Weave** and the **Weaver Script Engine**.  
It can be used both as a simple calculator and as a registry-aware expression evaluator.

#### Features

- Evaluates **arithmetic expressions**: `1+2-3*4/2`  
- Evaluates **logical expressions**: `score1 > score2 && flag == true`  
- Supports **registry variables** when a registry is provided:

```csharp
setValue("score1", 10, Wint);
setValue("score2", 5, Wint);
evaluate("score1 + score2"); // returns 15
```

- Optionally stores the result in a registry variable:

```csharp
evaluate("1 + 2 + 3", "total"); // stores 6 in variable 'total'
```

- Supports boolean evaluation (true/false) and numeric evaluation (double).
- Fallback evaluation ensures arithmetic expressions are processed if logical evaluation fails.

### Usage Examples
```csharp
// Simple arithmetic calculation
evaluate("1 + 2 + 3");           // returns 6

// Store result in registry
evaluate("1 + 2 + 3", "score");  // stores 6 in 'score'

// Using registry variables
setValue("a", 4, Wint);
setValue("b", 6, Wint);
evaluate("a + b");                // returns 10

// Logical expressions
setValue("flag1", true, Wbool);
setValue("flag2", false, Wbool);
evaluate("flag1 && flag2");       // returns false

// Complex mixed expressions
setValue("score1", 10, Wint);
setValue("score2", 5, Wint);
evaluate("score1 > score2 && score2 > 0"); // returns true
```

## UML

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

