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

[dotnet-sdk]: https://dotnet.microsoft.com/en-us/download
