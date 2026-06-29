# Bookstore Maintenance Log

## Detected Bugs and Improvement Hints

| ID | Type | Finding | Hint |
|---|---|---|---|
| BUG-01 | Bug | `show` printed duplicate entries due to nested loops. | Keep a single pass over the collection when rendering list views. |
| BUG-02 | Bug | `add` split by spaces, breaking multi-word fields and causing index errors. | Use tokenization that supports quoted arguments and validate argument count. |
| BUG-03 | Bug | Missing command arguments could trigger out-of-range access. | Guard command arguments before use and return usage guidance. |
| BUG-04 | Bug | `discontinueBook` could throw on non-numeric IDs and missing book IDs. | Use `TryParse` and null checks before state mutation. |
| BUG-05 | Bug | `discontinueBook` and `discontinueAuthor` were routed through the same ambiguous method. | Route each command to its explicit handler for deterministic behavior. |
| BUG-06 | Bug | Author discontinue log printed `{currentBook.Id}` literally. | Use structured formatting/interpolation correctly. |
| BUG-07 | Bug | Console output mixed direct `Console` calls and abstraction usage. | Route output through `IConsole` for consistency and testability. |
| OPT-01 | Optimization | Category mapping used repetitive `if` checks. | Use a case-insensitive dictionary lookup (`TryGetValue`) for clarity and O(1) lookup. |
| OPT-02 | Optimization | Title duplicate detection was case-sensitive. | Use case-insensitive comparisons for user-facing identifiers. |
| OPT-03 | Optimization | Help text did not reflect real commands. | Keep command docs aligned with parser behavior. |

## Changes Implemented and Reasons

### 1) Safer command parsing and execution
- Added empty-command handling in `Execute`.
- Added safe split for command and argument (`Split(' ', 2, RemoveEmptyEntries)`).
- Reason: prevents runtime exceptions and improves UX with explicit feedback.

### 2) Explicit command routing
- `discontinueBook` now calls `DiscontinueBook`.
- `discontinueAuthor` now calls `DiscontinueByAuthor`.
- Reason: removes ambiguous behavior and ensures predictable command intent.

### 3) Robust `add` parsing with quoted arguments
- Added `ParseArguments` using regex to support:
  - `add "<title>" "<author>" "<category>" "<description>"`
- Added argument count validation and usage message.
- Reason: fixes multi-word input bugs and avoids index-based crashes.

### 4) Category validation refactor
- Replaced chained `if` mapping with `CategoryNameToId` dictionary (`StringComparer.OrdinalIgnoreCase`).
- Added validation message listing valid categories.
- Reason: reduces branching complexity and improves readability/performance.

### 5) Discontinue guards and feedback
- Added `int.TryParse` for ID validation.
- Added missing-book guard.
- Added confirmation output when discontinuing a book.
- Added empty-author guard for `discontinueAuthor`.
- Reason: eliminates null/format crash paths and improves operator feedback.

### 6) Display loop and output consistency
- Fixed `show` to iterate once and print each book once.
- Added empty-state output (`No books registered.`).
- Routed runtime error output through `IConsole`.
- Reason: correct functional output and align with abstraction for maintainability.

### 7) Logging and help text corrections
- Fixed broken author discontinue message format.
- Updated `help` command to match actual supported commands and quoting format.
- Reason: improves operational clarity and reduces command misuse.

## Recommended Next Improvements (Not Yet Implemented)

1. Add automated tests for parser and discontinue flows (regression coverage).
2. Extract command handlers from `Program.cs` into dedicated services to reduce class size.
3. Persist data storage (file or DB) if production usage requires state across restarts.
