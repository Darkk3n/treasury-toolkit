# TreasuryToolkit

## Overview
TreasuryToolkit is a high-performance Windows desktop utility suite built with **.NET 10** and **WinForms**. Originally designed to tackle high-speed batch file renaming with strict preview guardrails, it is engineered to serve as a centralized hub for automating tedious administrative workflows, file parsing, and data manipulation.

## Key Features
* **Batch File Renaming:** Rename hundreds of files or entire directories instantly.
* **Safe Preview Grid:** A dedicated data grid lets you preview exact structural changes before writing them to the disk.
* **Token-Based Patterns:** Leverage dynamic tokens like `{name}`, `{ext}`, `{counter}`, and custom `{date:format}` configurations.
* **Safety Net (Undo):** One-click undo action to instantly revert your last batch rename operation.
* **High-Performance Architecture:** Compiled as a single, standalone, self-contained executable (`.exe`) with a zero-bundle footprint—no external framework installations required.

## Upcoming Modules 🚀
* **Excel Automation Engine:** Loop-free, LINQ-powered spreadsheet manipulation utilizing `ClosedXML` to automate column filtering, row counting, and aggregate math workflows.

## How It Works
1. **Target:** Launch the application and select a workspace folder with the target files.
2. **Pattern:** Inputs the previously stablished formatting and data points needed.
3. **Verify:** Check the Preview Data Grid to ensure token expansions map perfectly.
4. **Execute:** Commit the change with absolute confidence.

## Common Rename Tokens
* `{name}` — Original filename excluding the extension.
* `{ext}` — The file extension (e.g., `.pdf`, `.xlsx`).
* `{counter}` — Sequential incrementor (supports custom padding, e.g., `001`, `002`).
* `{date:format}` — Injects file timestamps using native .NET date strings (e.g., `{date:yyyy-MM-dd}`).

## Core Dependencies
* **iText.Kernel** — Advanced PDF binary parsing and page-copy architecture.
* **ClosedXML** — (In Development) High-speed, fluid OpenXML Excel spreadsheet manipulation.

## Troubleshooting
* **Unexpected Previews:** Simplify your pattern expression back to basic tokens (e.g., `{name}_{counter}{ext}`) to isolate formatting anomalies.
* **Failed Operations:** Ensure targeted files are closed in external applications (e.g., Excel or Acrobat) and that your user account has administrative read/write permissions for the target directory.

## License & Intellectual Property
Copyright © 2026 Darkk3n. All Rights Reserved.

This software and its source code are strictly proprietary and confidential. Unauthorized copying, modification, distribution, or commercial reuse of this repository's assets via any medium is strictly prohibited. The source code is maintained publicly solely for architectural exhibition and portfolio validation.

## Author
Maintained and engineered by **Darkk3n**.
