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

(You should have [.NET SDK][dotnet-sdk] version 9 or later to run it.)

License Automation
------------------
<!-- REUSE-IgnoreStart -->

If the CI asks you to update the file licenses, follow one of these:
1. Update the headers manually (look at the existing files), something like this:
   ```fsharp
   // SPDX-FileCopyrightText: %year% %your name% <%your contact info, e.g. email%>
   //
   // SPDX-License-Identifier: BSL-1.0
   ```
   (accommodate to the file's comment style if required).
2. Alternately, use the [REUSE][reuse] tool:
   ```console
   $ reuse annotate --license BSL-1.0 --copyright '%your name% <%your contact info, e.g. email%>' %file names to annotate%
   ```

(Feel free to attribute the changes to "tdlib.native contributors <https://github.com/ForNeVeR/tdlib.native>" instead of your name in a multi-author file, or if you don't want your name to be mentioned in the project's source: this doesn't mean you'll lose the copyright.)

<!-- REUSE-IgnoreEnd -->

File Encoding Changes
---------------------
If the automation asks you to update the file encoding (line endings or UTF-8 BOM) in certain files, run the following PowerShell script ([PowerShell Core][powershell] is recommended to run this script):
```console
$ pwsh -c "Install-Module VerifyEncoding -Repository PSGallery -RequiredVersion 2.2.1 -Force && Test-Encoding -AutoFix"
```

The `-AutoFix` switch will automatically fix the encoding issues, and you'll only need to commit and push the changes.

[dotnet-sdk]: https://dotnet.microsoft.com/en-us/download
[reuse]: https://reuse.software/
