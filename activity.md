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

### 2026-02-04 – Iteration 2: Implement calculation engine with unit-tested formulas
- Task: Implement calculation engine with unit-tested formulas (Task #2)
- Reviewed existing engine — already complete from iteration 1
- Added 6 new unit tests for edge cases (combined reducer+barlow, binning 3x3/4x4, FOV invariance, f-ratio with reducer+barlow)
- Commands: dotnet build SamplingCalculator.slnx, dotnet test SamplingCalculator.slnx
- Result: Build succeeds, 20/20 tests pass
