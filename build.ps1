param(
    [Parameter(Mandatory = $true)]
    [string]$ServerPath,

    [switch]$Deploy,

    [string]$PluginDirectory = "$env:APPDATA\SCP Secret Laboratory\LabAPI\plugins\global"
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$BinaryDirectory = Join-Path $Root "Binaries"
$ProjectFile = Join-Path $Root "SCPSL-MicroServer-Tweaks\SCPSL-MicroServer-Tweaks.csproj"

if (Test-Path (Join-Path $ServerPath "SCPSL_Data\Managed")) {
    $ManagedDirectory = Join-Path $ServerPath "SCPSL_Data\Managed"
}
elseif ((Split-Path $ServerPath -Leaf) -ieq "Managed") {
    $ManagedDirectory = $ServerPath
}
else {
    throw "Cannot find SCPSL_Data\Managed below: $ServerPath"
}

New-Item -ItemType Directory -Force -Path $BinaryDirectory | Out-Null

function Copy-RequiredAssembly {
    param(
        [string[]]$Names,
        [string]$DestinationName
    )

    foreach ($Name in $Names) {
        $Source = Join-Path $ManagedDirectory $Name
        if (Test-Path $Source) {
            Copy-Item $Source (Join-Path $BinaryDirectory $DestinationName) -Force
            Write-Host "Copied $Name"
            return
        }
    }

    throw "Missing required assembly in ${ManagedDirectory}: $($Names -join ' or ')"
}

Copy-RequiredAssembly @("Assembly-CSharp.dll") "Assembly-CSharp.dll"
Copy-RequiredAssembly @("LabApi.dll", "LabAPI.dll") "LabApi.dll"
Copy-RequiredAssembly @("UnityEngine.CoreModule.dll") "UnityEngine.CoreModule.dll"
Copy-RequiredAssembly @("UnityEngine.UIElementsModule.dll") "UnityEngine.UIElementsModule.dll"
Copy-RequiredAssembly @("UnityEngine.TextRenderingModule.dll") "UnityEngine.TextRenderingModule.dll"

Write-Host "Building SCPSL-MicroServer-Tweaks..."
dotnet build $ProjectFile -c Release

if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

$OutputDll = Join-Path $Root "SCPSL-MicroServer-Tweaks\bin\Release\net48\SCPSL_MicroServer_Tweaks.dll"

if (!(Test-Path $OutputDll)) {
    throw "Build completed but output DLL was not found: $OutputDll"
}

Write-Host ""
Write-Host "Built: $OutputDll"

if ($Deploy) {
    New-Item -ItemType Directory -Force -Path $PluginDirectory | Out-Null
    Copy-Item $OutputDll (Join-Path $PluginDirectory "SCPSL_MicroServer_Tweaks.dll") -Force
    Write-Host "Deployed to: $PluginDirectory"
}

Write-Host ""
Write-Host "Restart the SCP:SL server after deployment."
