# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

TreasuryToolkit is a Windows desktop (WPF) utility suite built on **.NET 10**. Its primary feature is a
guided workflow for treasury/accounting teams: scan a folder of bank-payment PDF receipts, extract structured
payment data (vendor, amount, currency, date, concept) via regex parsing of Spanish-language banking PDF text,
let the user review/edit the extracted data in a preview grid, then split/rename each PDF page into an
individually named file per payment. An Excel automation section exists but is on hold and hidden from the
sidebar (see below). See `README.md` for end-user-facing feature docs.

v2.0.0 replaced the original WinForms UI (`TreasuryToolkit.App`, v1) with the WPF project
`TreasuryToolkit.Wpf.App`. The v1 project has been removed from the repo; it is still in git history (any tag
before `v2.0.0`) if old behavior needs to be checked. Comments mentioning "v1" describe that original behavior
being kept or changed on purpose.

The codebase and its `.exe` releases are proprietary (see `LICENSE.txt`); the repo is public for portfolio
purposes only, not for reuse or contribution.

## Solution structure

Four projects (`TreasuryToolkit.slnx`), referenced only in one direction (`Wpf.App` → `Infra` → `Core`;
`Wpf.App` also references `Core` directly).

- **`TreasuryToolkit.Core`** (`net10.0`, platform-agnostic) — contracts (`Contracts/ICompanyService`,
  `IFileScanner`, `IPdfProcessor`) and plain data models (`Models/CompanyModel`, `PaymentRowData`,
  `ScannedPaymentData`). No third-party dependencies.
- **`TreasuryToolkit.Infra`** (`net10.0`) — concrete implementations of the `Core` contracts:
  - `LocalFileScanner` (`IFileScanner`): opens PDFs with iText, extracts text per page, and pulls out payment
    fields using a set of hand-tuned regexes matching Spanish bank-statement labels (`Importe`, `Motivo`,
    `Titular de la cuenta`, `Divisa`, `Fecha de aplicación`, etc.), plus text-cleanup heuristics for OCR/layout
    artifacts (words split across whitespace, merged fields).
  - `TextPdfProcessor` (`IPdfProcessor`): takes the (possibly user-edited) rows plus the original files and
    writes one single-page PDF per row, named `{Date}-{Company}-{Vendor}-{Concept}-{Amount} {Currency}.pdf`.
    Some companies have password/permission-restricted PDFs and require a
    page-to-canvas redraw workaround (`SliceSecuredPage`) instead of a direct page copy. Source files are
    deleted only once every page has been consumed. It throws `InvalidOperationException` up front if a file
    is open in another program. This project depends on `iText`.
  - `JsonCompanyService` (`ICompanyService`): loads the company list from the embedded resource
    `TreasuryToolkit.Wpf.App/Data/companies.json`, found through the entry assembly's
    `GetManifestResourceNames()`. It must stay an `EmbeddedResource` (not a WPF `Resource`) in the WPF
    project even though the service lives in `Infra`.
- **`TreasuryToolkit.Infra.Tests`** (`net10.0`, xUnit v3) — parser tests against text fixtures in
  `Fixtures/ReceiptText`. Fixtures must be anonymized receipt text with no real names or values.
  `global.json` opts `dotnet test` into Microsoft.Testing.Platform, which xUnit v3 needs on the .NET 10 SDK.
- **`TreasuryToolkit.Wpf.App`** (`net10.0-windows`, WPF, `OutputType=WinExe`, `AssemblyName=TreasuryToolkit`)
  — the UI, MVVM with CommunityToolkit.Mvvm. Composition is manual DI
  (`Microsoft.Extensions.DependencyInjection`, wired in `App.ConfigureServices`; no generic host).
  - `MainWindow` + `MainViewModel` are the shell: a sidebar and a content area bound to `CurrentViewModel`
    (`FileRenamerViewModel`, `ExcelWorkflowViewModel`, `AboutViewModel`, mapped to views by DataTemplates).
    ViewModels are singletons so each section keeps its state when navigating away and back.
  - `MainViewModel.IsExcelToolEnabled` is hard-coded to `false`: the Excel section's view, ViewModel and DI
    registration stay in place, and flipping it to `true` brings the sidebar entry back.
  - Theming: `Themes/Light.xaml` / `Themes/Dark.xaml` brush dictionaries plus `Themes/Controls.xaml` styles.
    `ThemeService` swaps the brush dictionary at runtime; every control must reference brushes through
    `DynamicResource` for that to work. It also darkens the native title bar.
  - Settings (`IsDarkMode`, sidebar state, window size/state, last folder) are persisted by `SettingsService`
    to `appsettings.local.json` next to the exe (`LocalAppSettings`). Property names match v1's, so an
    existing v1 settings file is picked up unchanged.
  - ViewModels never create windows: every dialog (info/warning/error, Yes/No, Retry/Cancel, folder picker,
    progress, the error report) goes through `IDialogService` / `DialogService`, using themed windows
    (`MessageWindow`, `ProgressWindow`, `ErrorWindow`). The folder picker is the native Windows dialog.
  - Unhandled exceptions (dispatcher, unobserved tasks, and background threads via `AppDomain`) are all
    routed to `ErrorWindow` in `App.xaml.cs`; `ErrorReportBuilder` assembles the stack trace, saved settings
    and environment (including the app version from `AppInfo`).
  - Bindings resolve ViewModel property names at runtime, so the WPF assembly must **not** be obfuscated.

### Data/control flow for the PDF tool

`FileRenamerViewModel.ScanFolderAsync` → `IFileScanner.ScanPdfFiles` on a background task (progress + per-row
callbacks stream results into the `DataGrid`) → user picks the company and edits rows (vendor forced upper
case, amount validated by `AmountValidationRule`) → `StartAsync`: `SourceBackupService` copies the source
folder to `<name>_Backup_yyyyMMdd`, `IPdfProcessor.ProcessPaymentBatch` runs (with a Retry/Cancel prompt while
a file is in use), the backup is deleted afterwards, and the results folder is opened. If processing fails
unexpectedly the backup is left behind so the originals can be restored by hand. The short pauses during
scan/process are intentional pacing so the status line can be followed.

## Build and run

`TreasuryToolkit.Wpf.App` requires Windows to run. On Windows, from the repo root:

```
dotnet restore
dotnet build                                  # builds the whole solution (TreasuryToolkit.slnx)
dotnet run --project TreasuryToolkit.Wpf.App  # launch the app
dotnet test --project TreasuryToolkit.Infra.Tests/TreasuryToolkit.Infra.Tests.csproj
```

Core, Infra and the tests are plain `net10.0` and build/run on macOS/Linux too. The WPF project can be
compile-checked (including XAML) off Windows with `-p:EnableWindowsTargeting=true`, but not run. There is no
separate lint step beyond the compiler/analyzers.

## Release process

`.github/workflows/release.yml` runs on `windows-latest` when a `v*` tag is pushed (or manually via
`workflow_dispatch`):

1. The version is derived from the tag (`v1.2.3` or `v1.2.3-suffix`); manual runs get `0.0.0-dev.<run>`.
   The csproj defaults to `0.0.0-dev` for local builds.
2. `dotnet test` runs the parser tests; a failure stops the release.
3. `dotnet publish` a self-contained, non-single-file `win-x64` build into `./staging`, then publish again
   with `PublishSingleFile=true`, `IncludeNativeLibrariesForSelfExtract=true` (required for WPF's native DLLs)
   and `--no-build` into `./publish`. `PublishReadyToRun=true` is passed to both steps on purpose (NETSDK1094
   otherwise). A check fails the run if anything besides `TreasuryToolkit.exe` is left in `./publish`.
4. The exe is code-signed (`dlemstra/code-sign-action`, using `BASE64_CODE_SIGNING_CERT` /
   `CODE_SIGNING_PASSWORD` secrets), zipped (avoids Edge's download warning for bare .exe files), and
   attached to a GitHub Release. Tags with a suffix (`v2.0.0-beta.1`) become prereleases; manual runs upload
   the zip as a build artifact instead.
