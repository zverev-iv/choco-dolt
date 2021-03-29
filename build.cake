#addin nuget:?package=Cake.FileHelpers&version=4.0.1
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Publish");
var packageVersion = Argument("packageVersion", "0.24.0");
var url = Argument("url", String.Empty);
var url64bit = Argument("url64bit", "https://github.com/dolthub/dolt/releases/download/v0.24.0/dolt-windows-amd64.zip");
var binDir = Argument("binDir", "bin");
var tempDir = Argument("tempDir", "temp");

ChocolateyPackSettings packageInfo = null;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   Information("Running tasks...");
});

Teardown(ctx =>
{
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    DeleteFiles("./**/*.nupkg");
    DeleteFiles("./**/*.nuspec");
    DeleteFiles(System.IO.Path.Combine(binDir, "*"));
    if (DirectoryExists(binDir))
    {
        DeleteDirectory(binDir, new DeleteDirectorySettings {
        Force = true
        });
    }
    DeleteFiles(System.IO.Path.Combine(tempDir, "*"));
    if (DirectoryExists(tempDir))
    {
        DeleteDirectory(tempDir, new DeleteDirectorySettings {
        Force = true
        });
    }
});

Task(".gitignore clean")
    .Does(() =>
{
    var regexes = FileReadLines("./.gitignore");
    foreach(var regex in regexes)
    {
        DeleteFiles(regex);
    }
});

Task("Set package info")
    .Does(() =>
{
    packageInfo = new ChocolateyPackSettings
    {
        //PACKAGE SPECIFIC SECTION
        Id = "dolt",
        Version = packageVersion,
        PackageSourceUrl = new Uri("https://github.com/zverev-iv/choco-dolt"),
        Owners = new[] { "zverev-iv" },
        //SOFTWARE SPECIFIC SECTION
        Title = "Dolt",
        Authors = new[] {
            "dolthub"
            },
        Copyright = "2021, dolthub",
        ProjectUrl = new Uri("https://dolthub.com"),
        ProjectSourceUrl = new Uri("https://github.com/dolthub/dolt"),
        DocsUrl = new Uri("https://docs.dolthub.com"),
        BugTrackerUrl = new Uri("https://github.com/dolthub/dolt/issues"),
        LicenseUrl = new Uri("https://github.com/dolthub/dolt/blob/master/LICENSE"),
        RequireLicenseAcceptance = false,
        Summary = "Version Controlled Database",
        Description = @"Dolt is the true Git for data experience in a SQL database, providing version control for schema and cell-wise for data, all optimized for collaboration.",
        ReleaseNotes = new[] { "https://github.com/dolthub/dolt/releases" },
        Files = new[] {
            new ChocolateyNuSpecContent {Source = System.IO.Path.Combine(binDir, "**"), Target = "tools"}
            },
        Tags = new[] {
            "dolt",
            "git",
            "database",
            "db",
            "version",
            "control"
            }
    };

});

Task("Copy src to bin")
    .Does(() =>
{
    if (!DirectoryExists(binDir))
    {
        CreateDirectory(binDir);
    }
    CopyFiles("src/*", binDir);
});

Task("Set package args")
    .IsDependentOn("Copy src to bin")
    .Does(() =>
{
    string hash  = null;
    string hash64 = null;
    if (!DirectoryExists(tempDir))
    {
        CreateDirectory(tempDir);
    }
    if(!string.IsNullOrWhiteSpace(url))
    {
        Information("Download x86 binary");
        var uri = new Uri(url);
        var fullFileName = System.IO.Path.Combine(tempDir, System.IO.Path.GetFileName(uri.LocalPath));
        DownloadFile(url, fullFileName);
        Information("Calculate sha256 for x86 binary");
        hash = CalculateFileHash(fullFileName).ToHex();
        Information("Write x86 data in sources");
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${url}", url);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksum}", hash);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksumType}", "sha256");
    }
    if(url64bit == url && hash != null)
    {
        Information("x86 and x64 uri are the same");
        Information("Write x64 data in sources");
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${url64bit}", url);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksum64}", hash);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksumType64}", "sha256");
    }
    else if(!string.IsNullOrWhiteSpace(url64bit))
    {
        Information("Download x64 binary");
        var uri = new Uri(url64bit);
        var fullFileName = System.IO.Path.Combine(tempDir, System.IO.Path.GetFileName(uri.LocalPath));
        DownloadFile(url64bit, fullFileName);
        Information("Calculate sha256 for x86 binary");
        hash64 = CalculateFileHash(fullFileName).ToHex();
        Information("Write x64 data in sources");
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${url64bit}", url64bit);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksum64}", hash64);
        ReplaceTextInFiles(System.IO.Path.Combine(binDir, "*"), "${checksumType64}", "sha256");
    }
});

Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Set package info")
    .IsDependentOn("Set package args")
    .Does(() =>
{
    ChocolateyPack(packageInfo);
});

Task("Publish")
    .IsDependentOn("Pack")
    .Does(() =>
{
    var publishKey = EnvironmentVariable<string>("CHOCOAPIKEY", null);
    var package = $"{packageInfo.Id}.{packageInfo.Version}.nupkg";

    ChocolateyPush(package, new ChocolateyPushSettings
    {
        ApiKey = publishKey
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
