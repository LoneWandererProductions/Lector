# Weaver Command System – Overview & Roadmap

Weaver is a **modular command execution framework** with **extensions** and a **feedback-driven confirmation system**. Commands implement `ICommand`, can optionally provide previews (`TryRun`), and return `FeedbackRequest` objects when user confirmation is required. Extensions (`ICommandExtension`) wrap or enhance commands (e.g., `.tryrun`, `.help`) but rely on commands for actual feedback handling.  

**Core idea:** The engine parses input → resolves command → executes → handles feedback via `IFeedback` → optionally invokes extensions.  

---

## How Weaver Works

1. **Command Definition**
   - Implements `ICommand`.
   - Returns `CommandResult` from `Execute`.
   - Can provide `TryRun` for previews.
   - Feedback (yes/no/cancel) is handled via `FeedbackRequest` objects returned from commands.

2. **Extensions**
   - Implement `ICommandExtension`.
   - Can hook **before**, **after**, or **around** command execution.
   - Extensions **delegate feedback** to commands; they don’t manage confirmation state themselves.

3. **Feedback**
   - `FeedbackRequest` centralizes prompts and valid options.
   - Engine automatically loops until the user provides valid input.
   - `onRespond` executes the appropriate command logic (e.g., delete a file, cancel, retry).

4. **ProcessInput**
   - Parses raw input, including optional extensions.
   - Resolves commands, executes `TryRun` or `Execute`.
   - Applies `IFeedback` loops if required.
   - Returns `CommandResult` with message, success flag, and optional `FeedbackRequest`.

---

## Short-Term Improvements / Roadmap

| Task | Description | Priority |
|------|------------|---------|
| **Base Command Class** | Provide `CommandBase` with default `TryRun` & `Help`. Reduces boilerplate. | ★★★★☆ |
| **Parameter Validation** | Validate argument counts and types at signature level. | ★★★☆☆ |
| **Extension Standardization** | Ensure all extensions follow `BeforeExecute` / `Invoke` / `AfterExecute` lifecycle, and delegate feedback to commands. | ★★★★☆ |
| **Unit Tests / Integration** | Full coverage for feedback loops and extensions, including `.tryrun` flows. | ★★★★★ |
| **Parser Improvements** | Support chained extensions, quoted/named parameters, and robust error messages. | ★★★☆☆ |

---

> This roadmap focuses on maintainability, clear separation of responsibilities, and leveraging `IFeedback` to eliminate fragile internal state while keeping commands extensible and testable.
