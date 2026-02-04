# Activity Log

## Format
- Date/time
- Task description
- What changed
- Commands run
- Result / issues

---

## Entries

### 2026-02-04 – Iteration 1: Scaffold Blazor WASM app
- Task: Scaffold Blazor WASM app + basic project structure (Task #1)
- Created solution, WASM project, test project with folder structure
- Added CalculatorInput/CalculatorResult models, SamplingCalculatorService
- Built Home.razor with input form and results display
- Added dark-theme CSS, simplified layout
- Wrote 14 unit tests for calculation engine
- Commands: dotnet new sln, dotnet new blazorwasm, dotnet new xunit, dotnet build, dotnet test
- Result: Build succeeds, 14/14 tests pass
