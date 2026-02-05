# PRD – Kalkulátor samplingu (kamera × dalekohled) pro astrofotografii (Blazor WebAssembly)

## Overview

Cílem je vytvořit jednoduchou, rychlou a vizuálně přehlednou webovou aplikaci (bez backendu), která uživateli spočítá a srozumitelně vyhodnotí sampling jeho sestavy **kamera + dalekohled** podobně jako “CCD Suitability” kalkulátory. Aplikace pomůže rozhodnout, zda je sestava vůči typickému seeingu **podvzorkovaná / optimální / převzorkovaná**, a nabídne praktická doporučení (binning, reducer/barlow, cílový rozsah pixel scale).

Aplikace poběží jako čistě statická stránka (Blazor WebAssembly), bude responzivní, moderní a s „astronomickým“ vzhledem (dark-first, hvězdné pozadí, jemné animace, čistá typografie).

## Target Audience

* Amatérští astrofotografové (DSO i planety) a EAA uživatelé
* Uživatelé, kteří skládají novou sestavu a chtějí rychle ověřit vhodnost kamery pro dané ohnisko
* Pokročilí uživatelé, kteří chtějí porovnat více konfigurací (reducer/barlow, binning)

## Core Features

### 1) Vstupy (Setup)

**Telescope / Optics**

* Základní ohnisková vzdálenost (mm)
* Průměr apertury (mm) – volitelné (pro doplňkové metriky typu Dawes limit)
* Faktor reduceru (např. 0.7×) / faktor barlow (např. 2×) – volitelné
* Efektivní ohnisko se počítá automaticky:

  * `effectiveFocal = baseFocal * barlowFactor / reducerFactor`

**Camera / Sensor**

* Pixel size (µm)
* Rozlišení senzoru (px): šířka × výška
* Binning (1×1, 2×2, 3×3, 4×4)
* Volitelně: název kamery (pro uložení presetů)

**Seeing / Cílové použití**

* Seeing (arcsec) – default např. 2.0″ s rychlým přepínačem „Výborný / Průměr / Slabý“
* Režim: **DSO** (seeing-limited) / **Planetary** (často více závislé na optice a lucky imaging)

  * V1: stačí DSO režim jako primární; Planetary jen informativní poznámka (bez složitého modelu)

**UX požadavky na vstupy**

* Číselné inputy s validací, jednotkami, rozumnými defaulty a tooltipy
* Okamžitý přepočet (bez tlačítka “Calculate”)

---

### 2) Výstupy (Results)

**Hlavní metriky**

* **Pixel scale** (arcsec/pixel):

  * `pixelScale = 206.265 * (pixelSizeUm * binning) / effectiveFocalMm`
* **Field of View (FOV)**:

  * `fovWidthDeg = (pixelScale * sensorWidthPx) / 3600`
  * `fovHeightDeg = (pixelScale * sensorHeightPx) / 3600`
  * Zobrazit i v arcmin (deg → arcmin ×60)

**Hodnocení samplingu (DSO/seeing-limited)**

* Doporučené rozmezí pixel scale vůči seeingu (jednoduché, srozumitelné):

  * **Optimální**: přibližně `seeing/3 … seeing/2` (uživatelsky vysvětleno jako “dobrý kompromis detail vs. SNR”)
  * **Podvzorkované**: `pixelScale > seeing/2` (hvězdy „hranaté“, ztráta detailu)
  * **Převzorkované**: `pixelScale < seeing/3` (zbytečný šum, nároky na guiding, data)
* Výstup jako:

  * Barevný „status badge“: Under / Optimal / Over
  * Jednoznačná věta “Your setup is likely **oversampled** for 2.0″ seeing.”

**Doporučení**

* Navržený binning pro přiblížení k optimu (např. “Zvaž 2×2 binning pro ~0.9″/px”)
* Doporučený reducer/barlow faktor (jen informativně: “S reducerem 0.8× bys byl blíž optimu”)
* Varování na extrémy (např. pixel scale > 4″/px nebo < 0.2″/px)

**Volitelné doplňkové metriky (V1 – low effort)**

* Dawes limit (arcsec) pro informaci: `116 / apertureMm` (pouze pokud je zadaná apertura)
* f-ratio (pokud uživatel zadá i průměr a ohnisko): `effectiveFocal / aperture`

---

### 3) Porovnání konfigurací (V1 “lightweight”)

* Přepínač “Compare mode” → uživatel si naklonuje setup A → setup B (např. binning 1×1 vs 2×2, s reducerem)
* Vedle sebe: pixel scale, FOV, status

---

### 4) Presety & sdílení

* Lokální uložení presetů (LocalStorage):

  * „My telescope“, „My camera“, “Full rig”
* Sdílitelný URL stav (query string):

  * Otevření linku obnoví přesně stejný setup (bez serveru)

---

### 5) UI/Design požadavky

* Responzivní layout: mobil → 1 sloupec (Inputs nad Results), desktop → 2 sloupce
* Dark-first „astronomical“ design:

  * Jemné hvězdné pozadí (nenáročné na výkon), glassmorphism karty
  * Čistá typografie, výrazné číselné výstupy
  * Jemné animace při změně statusu (bez rušivosti)
* Přístupnost:

  * Kontrast, focus stavy, klávesová ovladatelnost
  * Nevyžadovat barvu jako jediný nosič informace (textové labely)

## Tech Stack

* **Blazor WebAssembly** (doporučeně .NET 8 nebo novější dle aktuálního LTS ve vašem repo)
* UI:

  * Varianta A: MudBlazor (rychlá produktivita, moderní komponenty)
  * Varianta B: Tailwind CSS + vlastní komponenty (větší kontrola „astro“ stylu)
* State:

  * Jednoduchý “CalculatorState” + URL query sync + LocalStorage
* Testy:

  * Unit testy pro výpočty (xUnit)
  * Volitelně bUnit pro základní UI chování (nenutné v první iteraci)

## Constraints & Assumptions

* Tasks must be small enough to finish in one iteration.
* Verification must be runnable locally (dotnet build/test or equivalent).
* Žádný backend (žádné účty, žádná DB); vše lokálně v prohlížeči.
* Výpočty jsou “praktické” (pro amatérské použití), ne fyzikálně vyčerpávající model.

## Success Criteria

* V aplikaci lze zadat parametry a okamžitě vidět pixel scale, FOV a jednoznačné hodnocení samplingu.
* Stav lze sdílet URL a uložit jako preset.
* UI je responzivní a působí moderně (dark astro).
* Vše jde spustit lokálně a projde `dotnet build` + `dotnet test`.
* All tasks have "passes": true
* The agent outputs: `<promise>COMPLETE</promise>`

---

## Task List (DO NOT EDIT EXCEPT passes)

```json
[
  {
    "id": 1,
    "title": "Scaffold Blazor WASM app + basic project structure",
    "description": "Create a Blazor WebAssembly app with a clean folder structure (Pages, Components, Services, Models). Add a minimal home page shell with Inputs and Results sections. Verify with dotnet build.",
    "passes": true
  },
  {
    "id": 2,
    "title": "Implement calculation engine with unit-tested formulas",
    "description": "Create a pure C# calculator library/class that computes effective focal length, pixel scale (arcsec/px), and FOV (deg + arcmin). Add xUnit tests for representative cases, including binning and reducer/barlow effects. Verify with dotnet test.",
    "passes": true
  },
  {
    "id": 3,
    "title": "Add sampling classification logic (under/optimal/over) based on seeing",
    "description": "Implement DSO seeing-based classification using thresholds around seeing/2 and seeing/3. Return status + recommended target range. Unit test boundary conditions.",
    "passes": true
  },
  {
    "id": 4,
    "title": "Build input form components with validation and sensible defaults",
    "description": "Create reusable input components (numeric with units, select for binning, toggles for reducer/barlow). Add validation (non-negative, non-zero focal length, reasonable ranges) and inline helper tooltips.",
    "passes": true
  },
  {
    "id": 5,
    "title": "Create results UI with clear hierarchy and responsive layout",
    "description": "Design a Results card showing pixel scale, FOV, effective focal length, and sampling badge. Ensure mobile/desktop responsive behavior and accessible labeling.",
    "passes": true
  },
  {
    "id": 6,
    "title": "Add comparison mode (Setup A vs Setup B)",
    "description": "Implement a lightweight compare mode that duplicates the state and renders two result panels side-by-side on desktop (stacked on mobile). Include a 'Copy A to B' action.",
    "passes": true
  },
  {
    "id": 7,
    "title": "Persist presets in LocalStorage",
    "description": "Add save/load/delete preset functionality for camera, telescope, and full rig. Use LocalStorage. Provide a simple presets UI and handle invalid/old data gracefully.",
    "passes": true
  },
  {
    "id": 8,
    "title": "Shareable URL state (query string sync)",
    "description": "Encode current calculator state into the URL query string and restore it on load. Ensure that invalid parameters fall back to defaults without crashing.",
    "passes": false
  },
  {
    "id": 9,
    "title": "Astronomical theme (dark-first) + accessibility pass",
    "description": "Implement a modern astro look (dark palette, subtle starfield background, glass cards, consistent spacing/typography). Ensure keyboard navigation, focus states, and contrast compliance for key elements.",
    "passes": false
  },
  {
    "id": 10,
    "title": "SEO a metadata",
    "description": "Přidat SEO metadata do wwwroot/index.html a komponent: title, description, og:tags pro social sharing, favicon (astronomický motiv), manifest.json pro PWA ready (volitelně), správné lang atributy.",
    "passes": false
  },
  {
    "id": 11,
    "title": "Error handling a edge cases",
    "description": "Implementovat robustní error handling: validace vstupů (žádné záporné hodnoty, rozumné rozsahy), graceful handling neplatných kombinací, user-friendly error messages, fallback hodnoty pro edge cases.",
    "passes": false
  },
  {
    "id": 12,
    "title": "Performance optimalizace",
    "description": "Optimalizovat performance: lazy loading komponent kde vhodné, debounce na input změny (aby se nepočítalo při každém stisku klávesy), minimalizace re-renderů, AOT compilation setup pro menší bundle size.",
    "passes": false
  },
  {
    "id": 13,
    "title": "Dokumentace a README",
    "description": "Vytvořit README.md v root projektu: popis projektu a jeho účelu, screenshot aplikace, instrukce pro build a spuštění (dotnet build, dotnet run), instrukce pro spuštění testů, technický stack a architektura, license (MIT).",
    "passes": false
  },
  {
    "id": 14,
    "title": "Finální QA a code cleanup",
    "description": "Provést finální kontrolu: odstranit unused code a komentáře, konzistentní formátování (dotnet format), zkontrolovat všechny TODO komentáře, ověřit že všechny testy prochází (dotnet test), ověřit produkční build (dotnet publish).",
    "passes": false
  }
]
```

<promise>COMPLETE</promise>
