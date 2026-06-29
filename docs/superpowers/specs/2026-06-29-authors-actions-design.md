# Author Actions and Book Author Resolution Design

## Summary

This design adds two new actions to the Bookstore CLI:
- `addAuthor` to register authors
- `showAuthors` to list registered authors

It also changes book author resolution to use author IDs at add-time, with a mandatory fallback author:
- `Id = 0`, `Name = "Unknown Author"`

The existing `show` command continues to print author names, now resolved through author ID lookup during `add`.

## Goals

1. Support author management as first-class operations.
2. Keep current CLI style and in-memory storage model.
3. Ensure books always have an author name, even when author ID is unknown.
4. Keep implementation low-risk by preserving current `Book.Author` string field.

## Non-Goals

1. No persistence layer (DB/file) in this change.
2. No refactor to fully normalized `Book.AuthorId` storage in `Book`.
3. No changes to category model or discontinue flow behavior.

## Command Contracts

## 1) Add Author

Command:

`addAuthor <id> "<name>" <bornDate YYYY-MM-DD> "<award1,award2,...>"`

Examples:

- `addAuthor 1 "Frank Herbert" 1920-10-08 "Hugo,Nebula"`
- `addAuthor 2 "Ursula Le Guin" 1929-10-21 "Hugo,Locus"`

Behavior:
- Adds a new author with the provided fields.
- Rejects duplicate IDs.
- Rejects `id = 0` for manual creation (reserved).
- Parses `bornDate` strictly as `YYYY-MM-DD`.
- Splits awards by comma, trims whitespace, ignores empty entries.

## 2) Show Authors

Command:

`showAuthors`

Behavior:
- Prints all authors including `Unknown Author (ID 0)`.
- For each author, prints: ID, Name, Born Date, Awards list.

## 3) Add Book (updated contract)

Command:

`add "<title>" <authorId> "<category>" "<description>"`

Behavior:
- Parses `authorId` as integer.
- Resolves author by ID from in-memory author list.
- If ID does not exist, resolves to ID 0 (`Unknown Author`).
- Stores resolved author name in `Book.Author`.

## Domain Model

## Author

- `Id: long`
- `Name: string`
- `BornDate: DateOnly`
- `Awards: List<string>`

## Book (unchanged shape)

- Keep existing fields.
- `Author` remains string and stores resolved author name.

## Runtime Data

Inside `Bookstore`:
- `List<Book> books` (existing)
- `List<Author> authors` (new), initialized with:
  - `Author { Id = 0, Name = "Unknown Author", BornDate = default, Awards = [] }`

## Data Flow

### addAuthor flow

1. Parse command tokens (quoted-aware parser already used in app).
2. Validate argument count and formats.
3. Validate uniqueness and reserved ID rule.
4. Create and append author.
5. Write confirmation/error message via `IConsole`.

### add flow

1. Parse command arguments including numeric `authorId`.
2. Validate title/category/description inputs.
3. Resolve author:
   - found author by ID -> use that name
   - not found -> use Unknown Author
4. Create `Book` with resolved `Author` name.

### show flow

1. Iterate books.
2. Print current output format including author name (already included).

## Validation and Error Handling

- `addAuthor`:
  - invalid argument count -> usage message
  - invalid/non-numeric ID -> validation message
  - duplicate ID -> validation message
  - reserved ID 0 -> validation message
  - invalid date format -> validation message
- `add`:
  - invalid/missing `authorId` -> validation message
  - unknown `authorId` -> no failure; fallback to Unknown Author
- All messages are emitted via `IConsole`.

## Testing Strategy (TDD + AAA)

Add tests in `Bookstore.Tests`:

1. `addAuthor` success with awards parsing.
2. `addAuthor` duplicate ID rejection.
3. `addAuthor` reserved ID 0 rejection.
4. `addAuthor` invalid date rejection.
5. `showAuthors` includes Unknown Author and created authors.
6. `add` with known author ID uses that author name in `show`.
7. `add` with unknown author ID uses `Unknown Author` in `show`.

All tests must use AAA structure and be implemented RED -> GREEN.

## Documentation Updates

Update README command section to include:
- `addAuthor`
- `showAuthors`
- updated `add` signature with `<authorId>`

Include examples with quoted multi-word values.
