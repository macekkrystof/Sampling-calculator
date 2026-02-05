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

### 2026-02-04 – Iteration 3: Add sampling classification logic
- Task: Add sampling classification logic (under/optimal/over) based on seeing (Task #3)
- Added recommendation fields to CalculatorResult model
- Implemented recommendation logic: status messages, binning suggestions, corrector suggestions, extreme warnings
- Updated Home.razor to display recommendations card
- Added 20 new unit tests (boundary conditions, recommendations, extreme warnings, Theory-based tests)
- Fixed locale issue (CultureInfo.InvariantCulture for decimal formatting)
- Fixed reducer recommendation logic for PRD's inverted formula
- Commands: dotnet build SamplingCalculator.slnx, dotnet test SamplingCalculator.slnx
- Result: Build succeeds, 40/40 tests pass

### 2026-02-04 – Iteration 4: Build input form components with validation and sensible defaults
- Task: Build input form components with validation and sensible defaults (Task #4)
- Created InputValidationService with validation for all fields (focal length, aperture, reducer, barlow, pixel size, sensor dimensions, seeing)
- Created reusable NumericInput.razor component (label, unit, tooltip, validation, aria attributes)
- Refactored Home.razor to use NumericInput, added validation-aware results panel
- Added CSS for validation errors, tooltips, active preset buttons
- Fixed InvariantCulture value rendering for number inputs
- Set up Playwright .NET test project, wrote 10 UI tests
- Added 43 unit tests for validation logic
- Commands: dotnet build SamplingCalculator.slnx, dotnet test SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Playwright, npx playwright screenshot
- Result: Build succeeds, 83/83 unit tests pass, 10/10 Playwright UI tests pass

### 2026-02-05 – Iteration 5: Create results UI with clear hierarchy and responsive layout
- Task: Create results UI with clear hierarchy and responsive layout (Task #5)
- Enhanced Results panel with visual hierarchy: primary metrics (pixel scale, FOV) displayed prominently with large font
- Added card headings (SAMPLING, FIELD OF VIEW, OPTICS) for clear organization
- SAMPLING card highlighted with accent border and subtle gradient background
- Improved responsive layout: sticky results panel on desktop, stacked on mobile
- Added accessibility: aria-label, aria-labelledby, role="status/alert", semantic ul/li
- Wrote 9 new Playwright UI tests for visual hierarchy, accessibility, and responsive behavior
- Commands: dotnet build SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Tests, dotnet test tests/SamplingCalculator.Playwright, npx playwright screenshot
- Result: Build succeeds, 83/83 unit tests pass, 19/19 Playwright UI tests pass
