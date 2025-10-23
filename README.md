# Weaver Command System Roadmap

This document outlines a roadmap for improving and tightening the Weaver command and extension system, focusing on production readiness, maintainability, and flexibility.

---

## 1. Core Engine & Command Execution

### 1.1 Feedback Management Refactor
- **Issue:** `_pendingFeedbackCommand` and `_pendingFeedback` are fragile; bugs like “Unknown command 'yes'” happen.
- **Improvement:** Introduce a `FeedbackState` object or `IFeedbackProvider` interface to encapsulate feedback lifecycle. Could also support async and GUI prompts.
- **Priority:** ★★★★★
- **Benefit:** Safer handling of "yes/no/cancel," easier testing, reduces state bugs.

### 1.2 Extension Invocation Abstraction
- **Issue:** Currently extensions need to know the executor (`Func<string[], CommandResult>`) and sometimes call command methods directly.
- **Improvement:** Standardize extension lifecycle with hooks: `BeforeExecute`, `AfterExecute`, `AroundExecute`. Provide a unified interface for execution.
- **Priority:** ★★★★☆
- **Benefit:** Cleaner separation of concerns, extensions don’t touch command internals.

### 1.3 Consistent Error Handling & Logging
- **Issue:** Syntax errors, unknown commands, and extension errors are returned as messages only.
- **Improvement:** Introduce structured error codes, maybe `CommandErrorType` enum, and centralized logging.
- **Priority:** ★★★☆☆
- **Benefit:** Easier automation, better diagnostics in production.

---

## 2. Extensions System

### 2.1 Global vs. Command-Specific Extensions
- **Issue:** Some extensions are global (`help`), others require per-command references (`tryrun`).
- **Improvement:** Split into `IGlobalCommandExtension` and `ICommandBoundExtension`. Allow extensions to declare compatible commands.
- **Priority:** ★★★★☆
- **Benefit:** Cleaner design, easier to reason about which commands can use which extensions.

### 2.2 Extension Feedback Handling
- **Issue:** Some extensions can trigger feedback loops, which is confusing.
- **Improvement:** Make feedback optional and explicit. Possibly introduce `RequiresConfirmation` flag in the extension itself.
- **Priority:** ★★★★☆
- **Benefit:** Prevent unexpected feedback state pollution, safer extension development.

### 2.3 Extension Discovery / Registration
- **Issue:** Extensions are manually registered in `Weave`.
- **Improvement:** Add dynamic registration (reflection-based or configuration-based) for plugins/extensions.
- **Priority:** ★★★☆☆
- **Benefit:** Makes system more modular and plugin-ready.

---

## 3. Command Design

### 3.1 Optional `TryRun` and `Help`
- **Issue:** Every command might need to implement these; duplication risk.
- **Improvement:** Provide default base command class `CommandBase` with built-in `TryRun` and `Help` delegation to extensions.
- **Priority:** ★★★★☆
- **Benefit:** Reduces boilerplate, enforces consistent preview/feedback behavior.

### 3.2 Command Signature & Parameter Validation
- **Issue:** Argument counts are loosely enforced.
- **Improvement:** Enhance `CommandSignature` with parameter types, optional/default parameters, and validation logic.
- **Priority:** ★★★☆☆
- **Benefit:** Safer commands, better error messages, less runtime failures.

---

## 4. Parsing & Input Handling

### 4.1 Parser Robustness
- **Issue:** Only single extension supported; limited quoting support; brittle.
- **Improvement:** Extend `SimpleCommandParser` to support multiple chained extensions, quoted arguments, named parameters.
- **Priority:** ★★★☆☆
- **Benefit:** Makes the system closer to a production-ready CLI or scripting engine.

### 4.2 Namespacing Enforcement
- **Issue:** Ambiguity if multiple commands share names in different namespaces.
- **Improvement:** Introduce stricter namespace resolution or warnings for ambiguous commands.
- **Priority:** ★★☆☆☆
- **Benefit:** Avoids runtime confusion and unexpected command execution.

---

## 5. Testing & Tooling

### 5.1 MSTest / Unit Test Coverage
- **Issue:** Feedback loops and extensions have subtle bugs.
- **Improvement:** Write dedicated tests per command, per extension, and integration tests with `ProcessInput` flow.
- **Priority:** ★★★★★
- **Benefit:** Ensures stability, prevents regressions in complex interactions.

### 5.2 Diagnostics & Metrics
- **Issue:** Hard to see which commands are executed, how often, or which extensions are active.
- **Improvement:** Introduce optional telemetry/logging hooks. Could track execution time, success/failure, and feedback usage.
- **Priority:** ★★☆☆☆
- **Benefit:** Useful for debugging and understanding user interaction patterns.

---

## 6. Optional / Future Enhancements

- **Asynchronous Commands & Feedback:** Allow commands/extensions to return `Task<CommandResult>` for async workflows (web, GUI).  
- **Plugin System:** Auto-load commands/extensions from assemblies.  
- **Scripting Support:** Allow chaining commands with piping or variable substitution.  

---

### Priority Summary Table

| Area                         | Priority | Notes |
|-------------------------------|----------|-------|
| Feedback Management Refactor   | ★★★★★   | Critical for reliability |
| MSTest / Unit Test Coverage    | ★★★★★   | Prevent subtle feedback bugs |
| Extension Lifecycle Abstraction| ★★★★☆   | Cleaner separation of concerns |
| Global vs Command Extensions   | ★★★★☆   | Improves clarity and maintainability |
| Base Command with TryRun/Help  | ★★★★☆   | Reduces boilerplate |
| Structured Error Handling      | ★★★☆☆   | Needed for automation & diagnostics |
| Parser Improvements            | ★★★☆☆   | Needed for advanced input scenarios |
| Extension Discovery / Registration | ★★★☆☆ | Enables modularity and plugin support |
| Command Signature Validation   | ★★★☆☆   | Prevent runtime failures |
| Namespacing Enforcement        | ★★☆☆☆   | Helps avoid ambiguities |
| Diagnostics & Metrics          | ★★☆☆☆   | Useful for debugging/insights |

---

> This roadmap aims to turn Weaver from a functional prototype into a robust, maintainable, and production-ready command execution framework.
