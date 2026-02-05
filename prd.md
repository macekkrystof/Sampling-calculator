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
    "title": "Run UI tests and fix issues",
    "description": "Pusť UI testy a oprav problémy, pokud nějaké neprojdou, případně oprav testy, pokud by chyba byla v nich. ",
    "passes": true
  },
]
```

<promise>COMPLETE</promise>
