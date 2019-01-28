<#
.SYNOPSIS
This adds the complete closure of project references to a .sln file
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$WorkingDir,
    [Alias('sln')]
    [string]$SolutionFile
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path "$PSScriptRoot/../../"
$listFile = New-TemporaryFile

if (-not $WorkingDir) {
    $WorkingDir = Get-Location
}

Push-Location $WorkingDir
try {
    if (-not $SolutionFile) {

        $slnCount = Get-ChildItem *.sln | Measure

        if ($slnCount.count -eq 0) {
            Write-Error "Could not find a solution in this directory. Specify one with -sln <PATH>"
            exit 1
        }
        if ($slnCount.count -gt 1) {
            Write-Error "Multiple solutions found in this directory. Specify which one to modify with -sln <PATH>"
            exit 1
        }
        $SolutionFile = Get-ChildItem *.sln | select -first 1
    }

    & "$repoRoot\build.ps1" -projects "$(Get-Location)\**\*.*proj" /t:ShowProjectClosure "/p:ProjectsReferencedOutFile=$listFile"

    foreach ($proj in (Get-Content $listFile)) {
        & dotnet sln $SolutionFile add $proj
    }
}
finally {
    Pop-Location
    rm $listFile -ea ignore
}
