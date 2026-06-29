# Layered Architecture Refactor Design

## Summary

Refactor the current monolithic `Program.cs` into pragmatic maintainable layers while preserving CLI behavior and tests.

The new structure introduces:
- Application services for business workflows
- Repository interfaces for data access abstraction
- In-memory repository implementations (current runtime storage)
- Thin presentation/composition in `Program.cs`

This enables future persistence changes (e.g., database) by swapping repository implementations, not rewriting command/business logic.

## Goals

1. Split responsibilities currently concentrated in `Program.cs`.
2. Introduce interfaces to decouple use-cases from data access details.
3. Keep command behavior and output stable.
4. Preserve and expand tests to protect regressions.

## Non-Goals

1. No database integration in this change.
2. No major CLI UX redesign.
3. No persistence beyond in-memory storage.

## Target Layering

## 1) Presentation / Composition

Primary file:
- `Bookstore\Program.cs`

Responsibilities:
- command loop
- command dispatch
- dependency wiring (services + repositories + parser)
- rendering output via `IConsole`

Must not contain business rules or storage logic.

## 2) Application

Files:
- `Bookstore\Application\IBookCatalogService.cs`
- `Bookstore\Application\BookCatalogService.cs`
- `Bookstore\Application\IAuthorService.cs`
- `Bookstore\Application\AuthorService.cs`
- `Bookstore\Application\Parsing\ICommandParser.cs`
- `Bookstore\Application\Parsing\CommandParser.cs`

Responsibilities:
- add/show/discontinue books
- add/show authors
- command argument interpretation and validation orchestration

Dependencies:
- depends on repository interfaces and parser interface only
- no direct dependency on concrete in-memory repository classes

## 3) Domain

Files:
- `Bookstore\Book.cs`
- `Bookstore\Author.cs`
- `Bookstore\Domain\Repositories\IBookRepository.cs`
- `Bookstore\Domain\Repositories\IAuthorRepository.cs`

Responsibilities:
- entities and contracts
- no CLI or infrastructure concerns

## 4) Infrastructure

Files:
- `Bookstore\Infrastructure\InMemory\InMemoryBookRepository.cs`
- `Bookstore\Infrastructure\InMemory\InMemoryAuthorRepository.cs`

Responsibilities:
- in-memory storage and retrieval
- guarantee Unknown Author seed (`Id=0`) in author repository

## Dependency Direction

- Presentation -> Application interfaces
- Application -> Domain repository interfaces + parser interface
- Infrastructure -> Domain interfaces (implements contracts)

No upward dependency from domain/application into presentation/infrastructure.

## Behavioral Requirements (must remain true)

1. Existing commands continue to work with current signatures.
2. `addAuthor` and `showAuthors` continue to work as currently tested.
3. `add` resolves author by `authorId`, fallback to Unknown Author when missing.
4. `show` includes author name.
5. All user-facing messages continue through `IConsole`.

## Error Handling Rules

1. Validation remains explicit and deterministic.
2. No broad silent catches in services/repositories.
3. Program-level loop catch remains as top-level safety boundary.

## Testing Strategy

## Keep and adapt existing tests

- Command-level tests in `Bookstore.Tests` remain as integration safety net.
- Update existing tests to reference new file/class locations only where needed.

## Add new tests

1. `BookCatalogService` tests:
   - add success
   - duplicate title rejection
   - category validation
   - discontinue by id/author
2. `AuthorService` tests:
   - add success
   - duplicate id rejection
   - reserved id 0 rejection
   - invalid date rejection
   - show list shape
3. `CommandParser` tests:
   - quoted multi-word arguments
   - mixed quoted/unquoted tokens
   - empty input handling
4. `InMemoryAuthorRepository` tests:
   - Unknown Author seeded once
   - lookup by id
5. `InMemoryBookRepository` tests:
   - add/find/list/discontinue behavior

All tests follow AAA and TDD red-green sequence.

## Migration Plan (implementation order)

1. Introduce interfaces and in-memory repository implementations.
2. Move business logic from `Program.cs` into services.
3. Replace direct list access with repositories in services.
4. Reduce `Program.cs` to composition + command dispatch + output.
5. Update tests and add service/repository/parser tests.
6. Run full build/test/coverage verification.

## Future Extension Path

To add database persistence later:
1. Implement `IBookRepository` and `IAuthorRepository` in a DB project/folder.
2. Replace composition wiring in `Program.cs` from in-memory to DB implementation.
3. Keep services/tests mostly unchanged.
