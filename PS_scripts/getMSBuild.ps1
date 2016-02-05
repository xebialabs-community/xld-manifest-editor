function getMSBuild([string]$version) {
    $ErrorActionPreference = "Stop"
    $registryKey = "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\" + $version
    $msbuildLocationRegistryKey = (reg.exe query $registryKey /v MSBuildToolsPath)[2]
    $msbuildLocation = (Split-String -Separator "    " $msbuildLocationRegistryKey)[3]
    $msbuild = $msbuildLocation + "MSBuild.exe"
    Write-Host "MSBuild found at: " $msbuild
    $msbuild
}