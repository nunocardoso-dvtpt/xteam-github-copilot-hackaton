# Bookstore Legacy Application

This repository contains a legacy console-based bookstore management application.  
The purpose of this document is to provide operational context for project stakeholders and technical guidance for engineering maintenance.

---

## 1) Functional Overview (for Project Managers)

### Business purpose
The application supports basic catalog operations for books in a small operational context:
- register a new book
- list current books
- discontinue a specific book
- discontinue all books from a specific author

### Current scope
- **Interaction model:** command-line interface
- **Data lifecycle:** in-memory only (data resets on each execution)
- **Users:** internal operators / support staff

### Functional capabilities
- **Add book** with title, author, category, and description
- **Display catalog** with discontinued status
- **Discontinue by ID**
- **Discontinue by author**

### Functional limitations
- No persistent storage (no database/file save)
- No authentication or role control
- `add` command requires quoted arguments for multi-word values

### Operational risk register (easy fixes)
The following defects were identified and are now addressed in code.  
Detailed rationale is tracked in `MAINTENANCE_LOG.md`.

| ID | Severity | Summary | Impact | Status |
|---|---|---|---|
| BUG-01 | Medium | `show` command printed duplicate entries | Misleading operational output | Resolved |
| BUG-02 | High | `add` parsing broke for multi-word category/description | Failed or invalid book creation | Resolved |
| BUG-03 | Medium | Missing command arguments raised index errors | Runtime interruption | Resolved |
| BUG-04 | High | `discontinueBook` could throw null reference when ID does not exist | Runtime interruption | Resolved |
| BUG-05 | Medium | `discontinueBook` failed on non-numeric IDs | Runtime interruption | Resolved |
| BUG-06 | Medium | Ambiguous routing between discontinue-by-ID and discontinue-by-author | Unexpected behavior | Resolved |
| BUG-07 | Low | Author discontinue log printed literal `{currentBook.Id}` | Incorrect operational logs | Resolved |
| BUG-08 | Low | `help` text did not reflect actual commands | Operator confusion | Resolved |
| BUG-09 | Low | Error output bypassed console abstraction | Reduced testability/consistency | Resolved |

### Recommended next maintenance milestone
1. Add automated regression tests for command parsing and discontinue flows  
2. Split command parsing/dispatch into smaller focused classes  
3. Add persistent storage if data retention is required

---

## 2) Technical Reference (for Tech Leads)

### Technology baseline
- **Language:** C#
- **Target framework:** `.NET 10 (net10.0)`
- **Application type:** Console executable
- **Containerization:** Dockerfile + `compose.yaml`

### Solution structure
- `Bookstore\Program.cs`  
  Composition root and CLI command loop (presentation layer).
- `Bookstore\Book.cs`  
  Domain entity: `Id`, `Title`, `Author`, `CategoryId`, `Description`, `IsDiscontinued`.
- `Bookstore\Author.cs`  
  Domain entity: `Id`, `Name`, `BornDate`, `Awards`.
- `Bookstore\Domain\Repositories\*.cs`  
  Repository interfaces (data access contracts).
- `Bookstore\Application\*.cs` and `Bookstore\Application\Parsing\*.cs`  
  Application services and parser abstraction.
- `Bookstore\Infrastructure\InMemory\*.cs`  
  In-memory repository adapters.
- `Bookstore\IConsole.cs` / `Bookstore\RealConsole.cs`  
  Console abstraction layer.
- `Bookstore\Bookstore.csproj`  
  Build metadata and framework configuration.
- `Bookstore\Dockerfile`  
  Multi-stage build and runtime image entrypoint.
- `Bookstore.sln`  
  Root solution file.

### Runtime command set
- `show`
- `showAuthors`
- `add "<title>" <authorId> "<category>" "<description>"`
- `addAuthor <id> "<name>" <YYYY-MM-DD> "<award1,award2,...>"`
- `discontinueBook <id>`
- `discontinueAuthor <author>`
- `help`
- `quit`

Example author + book flow:

```powershell
addAuthor 10 "Frank Herbert" 1920-10-08 "Hugo,Nebula"
add "Dune" 10 "Science Fiction" "Classic novel"
show
showAuthors
```

### Architectural notes
- Application is stateful only for process lifetime (in-memory repositories).
- Presentation layer (`Program.cs`) delegates business rules to application services.
- Data access is abstracted via repository interfaces for easy implementation swap.
- Command parsing uses `ICommandParser` abstraction with quoted argument support.
- Runtime output/error handling uses the `IConsole` abstraction.

### Architecture layers
- Presentation: `Program.cs` (command loop + dependency wiring)
- Application: services and parser abstractions
- Domain: entities and repository interfaces
- Infrastructure: in-memory repository implementations

### Build and run
From repository root:

```powershell
dotnet run --project .\Bookstore\Bookstore.csproj
```

### Container run
From repository root:

```powershell
docker compose up --build
```

## Tests

Run all Bookstore unit tests:

```powershell
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj
```

Run tests with coverage output:

```powershell
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --settings .\Bookstore.Tests\Coverage\coverage.runsettings --collect:"XPlat Code Coverage"
```

### Technical maintenance priorities
- Introduce robust command parsing (quoted arguments or command model)
- Consolidate output/error handling behind `IConsole`
- Add guard clauses for all command argument and lookup operations
- Separate command dispatch concerns from domain mutation logic
- Add regression tests around command parsing and discontinue flows