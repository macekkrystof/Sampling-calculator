# Sampling Calculator for Astrophotography

A modern, fast, and responsive web application built with **Blazor WebAssembly** to help astrophotographers determine the suitability of their camera and telescope combination.

![Application Screenshot](docs/screenshot.png)

## 🔭 Overview

This application calculates the **sampling (pixel scale)** of your astrophotography rig and evaluates it against typical seeing conditions. It helps you decide if your setup is **Undersampled**, **Optimal**, or **Oversampled**, providing actionable recommendations for binning, focal reducers, or barlow lenses.

## ✨ Features

*   **Instant Calculation**: Real-time feedback on Pixel Scale (arcsec/px) and Field of View (FOV).
*   **Sampling Analysis**: Visual status badge (Under / Optimal / Over) based on your local seeing conditions.
*   **Smart Recommendations**: Suggestions for binning or optical correctors to reach optimal sampling.
*   **Comparison Mode**: Side-by-side comparison of two different setups.
*   **Presets**: Save and load your telescope and camera profiles via LocalStorage.
*   **Shareable Links**: Current configuration is synced to the URL for easy sharing.
*   **Astronomical Theme**: Dark-mode first design with starfield background and glassmorphism UI.
*   **PWA Ready**: Installable as a Progressive Web App.
*   **Privacy Focused**: Runs entirely client-side; no data is sent to a server.

## 🛠️ Tech Stack

*   **Framework**: Blazor WebAssembly (.NET 10)
*   **Language**: C#
*   **Styling**: Vanilla CSS (Custom variables, Flexbox/Grid, Glassmorphism)
*   **State Management**: In-memory state + URL Query Strings + LocalStorage
*   **Testing**: xUnit (Logic), Playwright (End-to-End UI)

## 🚀 Getting Started

### Prerequisites
*   [.NET 10 SDK](https://dotnet.microsoft.com/download) (or latest compatible version)

### Build
To build the solution:
```bash
dotnet build
```

### Run Locally
To start the development server:
```bash
dotnet run --project src/SamplingCalculator
```
Then open your browser at the address shown in the terminal (usually `http://localhost:5036` or similar).

### Run Tests
To run unit tests (xUnit):
```bash
dotnet test tests/SamplingCalculator.Tests
```

To run UI tests (Playwright):
*Note: The app must be running for UI tests to execute if they aren't configured to start it automatically, though the provided tests use Playwright's web server orchestration.*
```bash
dotnet test tests/SamplingCalculator.Playwright
```

## 📂 Project Structure

*   `src/SamplingCalculator`: Main Blazor WebAssembly application.
    *   `Components/`: Reusable UI components (NumericInput, InputPanel, ResultsPanel).
    *   `Models/`: Data models (CalculatorInput, CalculatorResult, Presets).
    *   `Pages/`: Application pages (Home.razor).
    *   `Services/`: Logic for calculations, validation, URL sync, and preset management.
    *   `wwwroot/`: Static assets (CSS, icons, index.html).
*   `tests/SamplingCalculator.Tests`: Unit tests for business logic and services.
*   `tests/SamplingCalculator.Playwright`: End-to-end UI tests.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
