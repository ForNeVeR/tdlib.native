<!--
SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: BSL-1.0
-->

Contributor Guide
=================

GitHub Actions
--------------
If you want to update the GitHub Actions used in the project, edit the file that generated them: `github-actions.fsx`.

Then run the following shell command:
```console
$ dotnet fsi github-actions.fsx
```

(You should have [.NET SDK][dotnet-sdk] version 8 or later to run it.)

File Encoding Changes
---------------------
If the automation asks you to update the file encoding (line endings or UTF-8 BOM) in certain files, run the following PowerShell script ([PowerShell Core][powershell] is recommended to run this script):
```console
$ pwsh -c "Install-Module VerifyEncoding -Repository PSGallery -RequiredVersion 2.2.1 -Force && Test-Encoding -AutoFix"
```

The `-AutoFix` switch will automatically fix the encoding issues, and you'll only need to commit and push the changes.

[dotnet-sdk]: https://dotnet.microsoft.com/en-us/download
