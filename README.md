TreasuryToolkit
===========

Overview
--------
TreasuryToolkit is a small Windows desktop utility (WinForms) written for .NET 10 that helps with batch renaming files. It focuses on quick, predictable renaming with a preview step and simple token-based patterns so you can safely rename large numbers of files.

Key features
------------
- Batch rename files and whole folders
- Preview changes before applying them
- Token-based name patterns (counters, original name, extension, date)
- Filter by extension and simple search patterns
- Undo the last rename operation
- Lightweight single-window UI for fast workflows

How it works (usage summary)
---------------------------
1. Launch the application.
2. Add files or select a folder to populate the file list (UI).
3. Enter a rename pattern using tokens (examples below).
4. Use the Preview Data Grid feature to verify results.
5. Apply the rename operation when satisfied.

Common tokens
-------------
- {name} — original filename without extension
- {ext} — file extension
- {counter} — sequential number (configurable start and padding)
- {date:format} — file timestamp formatted with .NET date format strings (e.g., {date:yyyyMMdd})

Dependencies
------------
- iText.Kernel for PDF parsing and page copying. Check the project file for the exact package/version.

Troubleshooting
---------------
- If preview shows unexpected results, try a simpler pattern (e.g., {name}{ext}) and verify tokens are used correctly.
- Check file permissions if rename operations fail on some files.

Author
------
Created by the repository owner.
