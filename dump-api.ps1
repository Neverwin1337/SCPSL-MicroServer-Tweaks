# Run this on Windows PowerShell in the project root. Paste output back.
# powershell -ExecutionPolicy Bypass -File "dump-api.ps1" > api-dump.txt

$binDir = "$PSScriptRoot\Binaries"

function Dump-Types($dll) {
    $path = "$binDir\$dll"
    if (!(Test-Path $path)) { "=== $dll NOT FOUND ==="; return }
    
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($path)
    } catch {
        "=== $dll LOAD FAILED: $($_.Exception.Message) ==="
        return
    }
    
    "=== $dll ==="
    $types = $asm.GetExportedTypes() | Sort-Object FullName
    
    # 1. Player.Get overloads
    $playerType = $types | Where-Object FullName -eq "LabApi.Features.Wrappers.Player"
    if ($playerType) {
        "--- Player.Get overloads ---"
        $getMethods = $playerType.GetMethods() | Where-Object { $_.Name -eq "Get" -and $_.IsStatic -and $_.IsPublic }
        foreach ($m in $getMethods) {
            $prms = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.FullName) $($_.Name)" }) -join ", "
            "  Get($prms)"
        }
    }

    # 2. CustomEventsHandler virtual methods
    $handlerType = $types | Where-Object FullName -eq "LabApi.Events.CustomHandlers.CustomEventsHandler"
    if ($handlerType) {
        "--- CustomEventsHandler virtual methods ---"
        $methods = $handlerType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($m in ($methods | Where-Object { $_.IsVirtual } | Sort-Object Name)) {
            $prms = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            "  override void $($m.Name)($prms)"
        }
    }
}

Dump-Types "LabApi.dll"
