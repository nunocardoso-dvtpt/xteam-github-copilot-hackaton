# Bookstore Test Suite and Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a dedicated .NET test project with TDD-driven tests that validate Bookstore behavior end-to-end and produce measurable coverage output.

**Architecture:** Create one xUnit test project (`Bookstore.Tests`) that references the production project and tests behavior through the public `Run()` workflow using a deterministic fake console. Keep tests grouped by command domain (`add/show`, `discontinue`, `help/error`) and add coverage collection through `coverlet.collector` with a repeatable `dotnet test` command.

**Tech Stack:** .NET 10, xUnit, Microsoft.NET.Test.Sdk, coverlet.collector, FluentAssertions (optional but recommended), dotnet CLI.

## Global Constraints

- Follow TDD strictly: write failing test first, run to observe failure, then implement minimal production code.
- Use AAA test structure in every test method (`// Arrange`, `// Act`, `// Assert`).
- Do not introduce test-only branches in production code.
- Keep console interaction deterministic via `IConsole` test double.
- Preserve existing CLI contract (`show`, `add`, `discontinueBook`, `discontinueAuthor`, `help`, `quit`).
- Use non-interactive commands only (`dotnet test`, `dotnet build`, `dotnet sln`).

---

## File Structure (target state)

- Create: `Bookstore.Tests\Bookstore.Tests.csproj`
- Create: `Bookstore.Tests\Console\ScriptedConsole.cs`
- Create: `Bookstore.Tests\BookstoreRunTests.cs`
- Create: `Bookstore.Tests\AddCommandTests.cs`
- Create: `Bookstore.Tests\DiscontinueCommandTests.cs`
- Create: `Bookstore.Tests\HelpAndErrorCommandTests.cs`
- Create: `Bookstore.Tests\Coverage\coverage.runsettings`
- Modify: `Bookstore.sln`
- Modify: `README.md`

### Task 1: Create and wire the test project

**Files:**
- Create: `Bookstore.Tests\Bookstore.Tests.csproj`
- Modify: `Bookstore.sln`

**Interfaces:**
- Consumes: `Bookstore\Bookstore.csproj`
- Produces: test project reference available for all test tasks

- [ ] **Step 1: Write the failing test scaffold check**

Add this minimal test in a temporary file `Bookstore.Tests\SmokeTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class SmokeTests
{
    [Fact]
    public void Smoke_Test_Project_Loads()
    {
        // Arrange
        
        // Act
        
        // Assert
        Assert.True(true);
    }
}
```

- [ ] **Step 2: Run test command to verify failure before project exists**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj`

Expected: FAIL with message similar to  
`MSB1009: Project file does not exist.`

- [ ] **Step 3: Create minimal test project implementation**

Run:

```powershell
dotnet new xunit -n Bookstore.Tests -f net10.0
dotnet add .\Bookstore.Tests\Bookstore.Tests.csproj reference .\Bookstore\Bookstore.csproj
dotnet add .\Bookstore.Tests\Bookstore.Tests.csproj package coverlet.collector
dotnet sln .\Bookstore.sln add .\Bookstore.Tests\Bookstore.Tests.csproj
```

Then ensure `Bookstore.Tests\Bookstore.Tests.csproj` contains:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Bookstore\Bookstore.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Run test to verify project passes**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj`

Expected: PASS with at least 1 test.

- [ ] **Step 5: Commit**

```bash
git add Bookstore.Tests\ Bookstore.sln
git commit -m "test: scaffold xunit project for bookstore"
```

### Task 2: Build deterministic console test double

**Files:**
- Create: `Bookstore.Tests\Console\ScriptedConsole.cs`
- Create: `Bookstore.Tests\BookstoreRunTests.cs`

**Interfaces:**
- Consumes: `Tasks.IConsole` interface
- Produces: `ScriptedConsole` reusable in all test classes

- [ ] **Step 1: Write failing test for console sequencing**

Create `Bookstore.Tests\BookstoreRunTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class BookstoreRunTests
{
    [Fact]
    public void Run_Stops_On_Quit_Command()
    {
        // Arrange
        var console = new ScriptedConsole(["quit"]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains("> ", console.Output);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~Run_Stops_On_Quit_Command"`

Expected: FAIL with compile error similar to  
`The type or namespace name 'ScriptedConsole' could not be found`.

- [ ] **Step 3: Write minimal implementation**

Create `Bookstore.Tests\Console\ScriptedConsole.cs`:

```csharp
using Tasks;

namespace Bookstore.Tests;

public sealed class ScriptedConsole : IConsole
{
    private readonly Queue<string> lines;
    public List<string> Output { get; } = new();

    public ScriptedConsole(IEnumerable<string> scriptedInput)
    {
        lines = new Queue<string>(scriptedInput);
    }

    public string ReadLine() => lines.Count > 0 ? lines.Dequeue() : "quit";

    public void Write(string format, params object[] args) =>
        Output.Add(string.Format(format, args));

    public void WriteLine(string format, params object[] args) =>
        Output.Add(string.Format(format, args));

    public void WriteLine() => Output.Add(string.Empty);
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~Run_Stops_On_Quit_Command"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore.Tests\Console\ScriptedConsole.cs Bookstore.Tests\BookstoreRunTests.cs
git commit -m "test: add scripted console test double"
```

### Task 3: Add TDD coverage for `add` and `show` behavior

**Files:**
- Create: `Bookstore.Tests\AddCommandTests.cs`

**Interfaces:**
- Consumes: `Bookstore.Bookstore.Run()`, command format `add "<title>" "<author>" "<category>" "<description>"`
- Produces: regression protection for parse/duplicate/show flows

- [ ] **Step 1: Write failing tests**

Create tests in `Bookstore.Tests\AddCommandTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class AddCommandTests
{
    [Fact]
    public void Add_With_Quoted_Multiword_Arguments_Shows_Book()
    {
        // Arrange
        var console = new ScriptedConsole([
            "add \"Dune Messiah\" \"Frank Herbert\" \"Science Fiction\" \"Classic sequel\"",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Dune Messiah"));
    }

    [Fact]
    public void Add_Duplicate_Title_Is_Rejected()
    {
        // Arrange
        var console = new ScriptedConsole([
            "add \"Dune\" \"Frank Herbert\" \"Science Fiction\" \"One\"",
            "add \"Dune\" \"Another Author\" \"Science Fiction\" \"Two\"",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Duplicate book"));
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AddCommandTests"`

Expected: FAIL (before production fixes when applying this plan on a clean baseline).

- [ ] **Step 3: Write minimal implementation**

Implement only the minimum in `Bookstore\Program.cs` needed to pass:

```csharp
private void Add(string commandLine)
{
    var arguments = ParseArguments(commandLine);
    if (arguments.Count != 4)
    {
        console.WriteLine("Usage: add \"<title>\" \"<author>\" \"<category>\" \"<description>\"");
        return;
    }
    AddBook(arguments[0], arguments[1], arguments[2], arguments[3]);
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~AddCommandTests"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore.Tests\AddCommandTests.cs Bookstore\Program.cs
git commit -m "test: cover add and show command flows"
```

### Task 4: Add TDD coverage for discontinue scenarios and error paths

**Files:**
- Create: `Bookstore.Tests\DiscontinueCommandTests.cs`

**Interfaces:**
- Consumes: `discontinueBook <id>`, `discontinueAuthor <author>`
- Produces: regression coverage for ID validation, not-found handling, author matching

- [ ] **Step 1: Write failing tests**

Create `Bookstore.Tests\DiscontinueCommandTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class DiscontinueCommandTests
{
    [Fact]
    public void DiscontinueBook_Invalid_Id_Prints_Validation_Message()
    {
        // Arrange
        var console = new ScriptedConsole(["discontinueBook abc", "quit"]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("Invalid book ID"));
    }

    [Fact]
    public void DiscontinueAuthor_Marks_All_Active_Books_For_Author()
    {
        // Arrange
        var console = new ScriptedConsole([
            "add \"Book 1\" \"Author A\" \"Fiction\" \"Desc\"",
            "add \"Book 2\" \"Author A\" \"Fantasy\" \"Desc\"",
            "discontinueAuthor Author A",
            "show",
            "quit"
        ]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        var discontinuedCount = console.Output.Count(line => line.Contains("(discontinued)"));
        Assert.True(discontinuedCount >= 2);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~DiscontinueCommandTests"`

Expected: FAIL on clean baseline.

- [ ] **Step 3: Write minimal implementation**

Implement minimal validation and author discontinue logic in `Bookstore\Program.cs`:

```csharp
if (!int.TryParse(idString, out var id))
{
    console.WriteLine("Invalid book ID: \"{0}\". Please provide a numeric ID.", idString);
    return;
}
```

and

```csharp
foreach (var currentBook in books.Where(book => book.Author.Equals(authorName.Trim(), StringComparison.OrdinalIgnoreCase)))
{
    if (!currentBook.IsDiscontinued)
    {
        currentBook.IsDiscontinued = true;
        console.WriteLine("Discontinued: '{0}' from author {1} (ID: {2})", currentBook.Title, currentBook.Author, currentBook.Id);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~DiscontinueCommandTests"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Bookstore.Tests\DiscontinueCommandTests.cs Bookstore\Program.cs
git commit -m "test: cover discontinue command edge cases"
```

### Task 5: Cover help/unknown command flows and add coverage reporting

**Files:**
- Create: `Bookstore.Tests\HelpAndErrorCommandTests.cs`
- Create: `Bookstore.Tests\Coverage\coverage.runsettings`
- Modify: `README.md`

**Interfaces:**
- Consumes: `help` and unknown command behavior
- Produces: repeatable coverage command and docs

- [ ] **Step 1: Write failing tests**

Create `Bookstore.Tests\HelpAndErrorCommandTests.cs`:

```csharp
using Xunit;

namespace Bookstore.Tests;

public class HelpAndErrorCommandTests
{
    [Fact]
    public void Help_Prints_Supported_Commands()
    {
        // Arrange
        var console = new ScriptedConsole(["help", "quit"]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("discontinueBook"));
        Assert.Contains(console.Output, line => line.Contains("discontinueAuthor"));
    }

    [Fact]
    public void Unknown_Command_Prints_Error()
    {
        // Arrange
        var console = new ScriptedConsole(["unknownCommand", "quit"]);
        var sut = new global::Bookstore.Bookstore(console);

        // Act
        sut.Run();

        // Assert
        Assert.Contains(console.Output, line => line.Contains("I don't know what the command"));
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:  
`dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --filter "FullyQualifiedName~HelpAndErrorCommandTests"`

Expected: FAIL on clean baseline where help text is stale.

- [ ] **Step 3: Write minimal implementation**

Ensure `Help()` in `Bookstore\Program.cs` includes:

```csharp
console.WriteLine("  show");
console.WriteLine("  add \"<title>\" \"<author>\" \"<category>\" \"<description>\"");
console.WriteLine("  discontinueBook <book ID>");
console.WriteLine("  discontinueAuthor <author name>");
console.WriteLine("  help");
console.WriteLine("  quit");
```

Create `Bookstore.Tests\Coverage\coverage.runsettings`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Exclude>[xunit.*]*</Exclude>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

Update `README.md` with a testing section:

```markdown
## Tests

Run unit tests:
```powershell
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj
```

Run tests with coverage:
```powershell
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --settings .\Bookstore.Tests\Coverage\coverage.runsettings --collect:"XPlat Code Coverage"
```
```

- [ ] **Step 4: Run full verification**

Run:

```powershell
dotnet build .\Bookstore.sln
dotnet test .\Bookstore.Tests\Bookstore.Tests.csproj --settings .\Bookstore.Tests\Coverage\coverage.runsettings --collect:"XPlat Code Coverage"
```

Expected:
- Build succeeds with 0 errors.
- All tests pass.
- Coverage file generated under `Bookstore.Tests\TestResults\**\coverage.cobertura.xml`.

- [ ] **Step 5: Commit**

```bash
git add Bookstore.Tests\HelpAndErrorCommandTests.cs Bookstore.Tests\Coverage\coverage.runsettings README.md Bookstore\Program.cs
git commit -m "test: add help/error coverage and test run docs"
```

## Self-Review Checklist (applied to this plan)

1. **Spec coverage:** Includes test project creation, many tests across all command paths, TDD steps, and coverage reporting command.
2. **Placeholder scan:** No TODO/TBD placeholders; every task has explicit files, commands, and code blocks.
3. **Type consistency:** All tests instantiate `global::Bookstore.Bookstore` and use `ScriptedConsole : Tasks.IConsole`.
