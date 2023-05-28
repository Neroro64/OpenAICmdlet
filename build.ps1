param(
    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [string] $OutputPath = "$PSSCriptRoot/Artifacts",

    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [ValidateSet("Release", "Debug")]
    [string] $Configuration = "Release",

    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [string] $ModuleName = "OpenAICmdlet",

    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [Parameter(ParameterSetName = "UpdateVersionMinor")]
    [Switch] $BumpVersionMinor,

    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [Parameter(ParameterSetName = "UpdateVersionMajor")]
    [Switch] $BumpVersionMajor,

    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [Switch] $NoHelpFiles,

    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [Switch] $NoTestVerification,

    [Parameter(ParameterSetName = "DefaultParameterSet")]
    [Switch] $Clean
)


# Clean any previous artifacts
if ($Clean) {
    if (Test-Path $OutputPath) {
        Remove-Item -Recurse -Force $OutputPath/*
    }
}

# Run the tests unless $NoTestVerification is set 
if (-not $NoTestVerification) {
    dotnet test "$PSScriptRoot/$ModuleName.sln" --verbosity quiet
}

# Abort if tests fail
if (-not $?) {
    Write-Error "Failed to pass the tests. Aborting the build..."
    return
}

# Build the Cmdlet project
dotnet build "$PSScriptRoot/Cmdlet/Cmdlet.csproj" --configuration $Configuration --output $OutputPath

# Copy and Update the module manifest
Copy-Item -Path "$PSScriptRoot/Module/*" -Destination $OutputPath
$manifest = Test-ModuleManifest -Path "$OutputPath/$ModuleName.psd1"

if ($BumpVersionMinor) {
    $manifest.Version = [Version]::New($manifest.Version.Major, $manifest.Version.Minor + 1)
    Update-ModuleManifest -Path "$OutputPath/$ModuleName.psd1" -ModuleVersion $manifest.Version
}
elseif ($BumpVersionMajor) {
    $manifest.Version = [Version]::New($manifest.Version.Major + 1, 0)
    Update-ModuleManifest -Path "$OutputPath/$ModuleName.psd1" -ModuleVersion $manifest.Version
}


if (-not $NoHelpFiles) {
    # Starts a new PowerShell session
    Invoke-Expression "pwsh -c {
        # Generate the help files
        Import-Module '$OutputPath/$ModuleName.dll'
        Import-Module platyPS
        `$docsDir = Join-Path $PSScriptRoot -ChildPath 'docs'
        `$externalHelpDir = Join-Path $OutputPath -ChildPath 'en-US'
        `$platyPSParameters = @{
            Module                = '$ModuleName'
            OutputFolder          = `$docsDir
            AlphabeticParamsOrder = `$true
            WithModulePage        = `$true
            ExcludeDontShow       = `$true
            Encoding              = [System.Text.Encoding]::UTF8    
        }

        if (Test-Path `$docsDir) {
            Update-MarkdownHelpModule -Path `$docsDir -RefreshModulePage
        }
        else {
            New-MarkdownHelp @platyPSParameters
            New-MarkdownAboutHelp -OutputFolder `$docsDir -AboutName '$ModuleName'
        }
        New-ExternalHelp -Path `$docsDir -OutputPath `$externalHelpDir
    }"
}



