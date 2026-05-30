FileRenamer
===========

Overview
--------
FileRenamer is a small Windows desktop utility (WinForms) written for .NET 10 that helps with batch renaming files. It focuses on quick, predictable renaming with a preview step and simple token-based patterns so you can safely rename large numbers of files.

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
4. Use the Preview feature to verify results.
5. Apply the rename operation when satisfied.

Common tokens
-------------
- {name} — original filename without extension
- {ext} — file extension
- {counter} — sequential number (configurable start and padding)
- {date:format} — file timestamp formatted with .NET date format strings (e.g., {date:yyyyMMdd})

Pattern examples
----------------
- "{name}_{counter:000}" -> adds a zero-padded counter, e.g. photo_001.jpg
- "{date:yyyyMMdd}_{name}{ext}" -> prepends the date to the existing name

Troubleshooting
---------------
- If preview shows unexpected results, try a simpler pattern (e.g., {name}{ext}) and verify tokens are used correctly.
- Check file permissions if rename operations fail on some files.

Author
------
Created by the repository owner. For questions or issues, check the repository issue tracker.
