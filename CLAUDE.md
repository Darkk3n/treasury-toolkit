# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

TreasuryToolkit is a Windows desktop (WinForms) utility suite built on **.NET 10**. Its primary feature is a
guided workflow for treasury/accounting teams: scan a folder of bank-payment PDF receipts, extract structured
payment data (vendor, amount, currency, date, concept) via regex parsing of Spanish-language banking PDF text,
let the user review/edit the extracted data in a preview grid, then split/rename each PDF page into an
individually named file per payment. An Excel automation module (`UcExcelWorkflowAutomator`, using ClosedXML)
is in early development. See `README.md` for end-user-facing feature docs and rename token syntax.

The codebase and its `.exe` releases are proprietary (see `LICENSE.txt`); the repo is public for portfolio
purposes only, not for reuse or contribution.

## Solution structure

Three projects, referenced only in one direction (`App` → `Infra` → `Core`; `App` also references `Core`
directly). There is no test project in the solution.

- **`TreasuryToolkit.Core`** (`net10.0`, platform-agnostic) — contracts (`Contracts/ICompanyService`,
  `IFileScanner`, `IPdfProcessor`) and plain data models (`Models/CompanyModel`, `PaymentRowData`,
  `ScannedPaymentData`). No third-party dependencies. This is the only project that builds on non-Windows
  hosts.
- **`TreasuryToolkit.Infra`** (`net10.0`) — concrete implementations of the `Core` contracts:
  - `LocalFileScanner` (`IFileScanner`): opens PDFs with iText, extracts text per page, and pulls out payment
    fields using a set of hand-tuned regexes matching Spanish bank-statement labels (`Importe`, `Motivo`,
    `Titular de la cuenta`, `Divisa`, `Fecha de aplicación`, etc.), plus text-cleanup heuristics for OCR/layout
    artifacts (words split across whitespace, merged fields).
  - `TextPdfProcessor` (`IPdfProcessor`): takes the (possibly user-edited) rows plus the original files and
    writes one single-page PDF per row, named `{Date}-{Company}-{Vendor}-{Concept}-{Amount} {Currency}.pdf`.
    Some companies (currently `EMKA`, `KLEIBERIT`) have password/permission-restricted PDFs and require a
    page-to-canvas redraw workaround (`SliceSecuredPage`) instead of a direct page copy. Source files are
    deleted only once every page has been consumed. This project depends on `iText`.
  - `JsonCompanyService` (`ICompanyService`): loads the company list from the embedded resource
    `TreasuryToolkit.App/Data/companies.json` (found via `Assembly.GetManifestResourceNames()`, so it must stay
    an `EmbeddedResource` in the App project even though the service lives in `Infra`).
- **`TreasuryToolkit.App`** (`net10.0-windows`, WinForms, `OutputType=WinExe`) — the UI, composed with manual
  constructor DI (`Microsoft.Extensions.DependencyInjection`, wired in `Program.ConfigureServices`; no ASP.NET
  hosting). `MainForm` is a singleton shell with a sidebar; it swaps a single content panel between
  `UcFileRenamer` (the PDF tool), `UcExcelWorkflowAutomator`, and `UcAbout` user controls (`ShowView`).
  Dark/light theme is tracked as app state (`_isDarkMode`) and persisted to `appsettings.local.json` next to
  the executable (`LocalAppSettings`, plain JSON, not to be confused with `TreasuryToolkit.App.csproj`
  build config).
  - Unhandled exceptions on the UI thread route to `ExceptionHandlerForm`; unhandled exceptions on other
    threads currently just rethrow via `NotImplementedException` in `Program.CurrentDomain_UnhandledException`
    — this is a known gap, not an intentional design choice, if you're touching that code path.

### Data/control flow for the PDF tool

`UcFileRenamer` → `IFileScanner.ScanPdfFiles` (progress + per-row callbacks stream results into the preview
`DataGridView`) → user edits company/rows in the grid → `IPdfProcessor.ProcessPaymentBatch` consumes the
edited rows against the original files to produce the renamed/split output, with an undo path that restores
the previous state (see `UcFileRenamer.cs`).

## Build and run

The App and Infra projects require Windows (WinForms, `net10.0-windows`) and cannot be built or run on
macOS/Linux — only `TreasuryToolkit.Core` builds cross-platform. On Windows, from the repo root:

```
dotnet restore
dotnet build                              # builds the whole solution (TreasuryToolkit.slnx)
dotnet run --project TreasuryToolkit.App  # launch the WinForms app
```

There are no automated tests in this repository (no test project exists) and no separate lint step beyond
the compiler/analyzers.

## Release process

`.github/workflows/release.yml` runs on `windows-latest` when a `v*` tag is pushed (or manually via
`workflow_dispatch`):

1. `dotnet publish` a self-contained, non-single-file `win-x64` build into `./staging` (`Stage Unbundled
   Binaries`).
2. An Obfuscar-based obfuscation step exists in the workflow but is **currently commented out**
   (`Run Obfuscation`) — the subsequent "overwrite with obfuscated DLL" copy step still runs against
   `staging\obfuscated\...`, so re-enabling/disabling obfuscation must be done consistently across both
   steps or the copy will fail.
3. `dotnet publish` again with `/p:PublishSingleFile=true /p:PublishReadyToRun=true --no-build` to produce the
   final single-file `TreasuryToolkit.exe` in `./publish`.
4. The exe is code-signed (`dlemstra/code-sign-action`, using `BASE64_CODE_SIGNING_CERT` /
   `CODE_SIGNING_PASSWORD` secrets) and attached to a GitHub Release.

`TreasuryToolkit.App.csproj` also defines an `ObfuscateAssemblies` MSBuild target that runs Obfuscar
automatically `AfterTargets="Compile"` when `Configuration == Release` — this is a separate mechanism from the
workflow's (disabled) explicit obfuscation step, and both read the same `obfuscar.xml`.
