param([System.String]$version="SNAPSHOT")
$ErrorActionPreference = "Stop"
# Script to package the XL Deploy Manifest Editor
# Uses the PowerShell Community Extensions from http://pscx.codeplex.com/ for creating a ZIP

Import-Module Pscx -MinimumVersion 3.0.0.0

$manifestEditorProj = "..\XebiaLabs.XLDeploy\XebiaLabs.XLDeploy.ManifestEditor\XebiaLabs.XLDeploy.ManifestEditor.csproj"
$tempdir = "$($env:TEMP)\$([system.guid]::newguid().tostring())"
$outdir = "$tempDir\out"

. .\getMSBuild.ps1
& (getMSBuild 4.0) $manifestEditorProj "/p:Configuration=Release,OutDir=$outdir\"

$target = "$tempdir\xl-deploy-manifest-editor"
$dir = Mkdir $target

Copy-Item $outdir\*.dll $target
Copy-Item $outdir\*.exe $target

$zip = Write-Zip $target ..\build\distributions\xl-deploy-manifest-editor-$version.zip

Rmdir $tempdir -Recurse -Force -ErrorAction Ignore
