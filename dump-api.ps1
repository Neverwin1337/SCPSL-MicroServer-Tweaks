# Run on Windows: powershell -ExecutionPolicy Bypass -File "dump-api.ps1"
$ErrorActionPreference = "Continue"
$binDir = "$PSScriptRoot\Binaries"

function Find-Type($asm, $name) {
    $t = $asm.GetExportedTypes() | Where-Object { $_.FullName -eq $name -or $_.Name -eq $name }
    if ($t) { return $t } else { return $null }
}

# Load LabApi
$labapi = [System.Reflection.Assembly]::LoadFrom("$binDir\LabApi.dll")
Write-Host "=== LabApi.dll loaded ===" -ForegroundColor Green

# 1. CommandType enum values
$cmdType = Find-Type $labapi "LabApi.Features.Enums.CommandType"
if ($cmdType) {
    Write-Host "`n--- CommandType enum ---" -ForegroundColor Yellow
    [System.Enum]::GetNames($cmdType) | ForEach-Object { Write-Host "  $_" }
}

# 2. Player.Get overloads  
$playerType = Find-Type $labapi "LabApi.Features.Wrappers.Player"
if ($playerType) {
    Write-Host "`n--- Player.Get overloads ---" -ForegroundColor Yellow
    $getMethods = $playerType.GetMethods() | Where-Object { $_.Name -eq "Get" -and $_.IsStatic -and $_.IsPublic }
    foreach ($m in $getMethods) {
        $prms = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.FullName)" }) -join ", "
        Write-Host "  Get($prms) -> $($m.ReturnType.FullName)"
    }
    
    Write-Host "`n--- Player.TryGet overloads ---" -ForegroundColor Yellow
    $tryGetMethods = $playerType.GetMethods() | Where-Object { $_.Name -eq "TryGet" -and $_.IsStatic -and $_.IsPublic }
    foreach ($m in $tryGetMethods) {
        $prms = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.FullName) $($_.Name)" }) -join ", "
        Write-Host "  TryGet($prms) -> $($m.ReturnType)"
    }
}

# 3. CustomEventsHandler - all virtual methods with "Command" or "RoundStart" in name
Write-Host "`n--- CustomEventsHandler methods (Command+/RoundStart+) ---" -ForegroundColor Yellow
$handlerType = Find-Type $labapi "LabApi.Events.CustomHandlers.CustomEventsHandler"
if ($handlerType) {
    $methods = $handlerType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { $_.IsVirtual }
    foreach ($m in $methods) {
        if ($m.Name -match 'Command|Round|Lobby|Waiting|Started') {
            $prms = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name)" }) -join ", "
            Write-Host "  override $($m.Name)($prms)"
        }
    }
}

# Load Assembly-CSharp (may fail on many deps)
Write-Host "`n=== Trying Assembly-CSharp.dll ===" -ForegroundColor Green
try {
    $asc = [System.Reflection.Assembly]::LoadFrom("$binDir\Assembly-CSharp.dll")
    
    # 4. RoundStart class
    $rsType = Find-Type $asc "RoundStart"
    if (-not $rsType) {
        $rsType = Find-Type $asc "GameCore.RoundStart"
    }
    if ($rsType) {
        Write-Host "`n--- RoundStart: $($rsType.FullName) ---" -ForegroundColor Yellow
        Write-Host "  Base: $($rsType.BaseType.FullName)"
        
        # Static fields (singletons)
        $fields = $rsType.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($f in $fields) { Write-Host "  STATIC: $($f.FieldType.Name) $($f.Name)" }
        
        # Properties
        $props = $rsType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($p in $props) {
            $rw = ""
            if ($p.CanRead) { $rw += "get;" }
            if ($p.CanWrite) { $rw += "set;" }
            Write-Host "  PROP: $($p.PropertyType.Name) $($p.Name) {$rw}"
        }
        
        # Methods
        $methods = $rsType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { !$_.IsSpecialName -and !$_.Name.StartsWith('get_') -and !$_.Name.StartsWith('set_') }
        foreach ($m in $methods) {
            $isStatic = if ($m.IsStatic) { "static " } else { "" }
            $prms = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ", "
            Write-Host "  $isStatic$($m.Name)($prms)"
        }
    } else {
        Write-Host "  RoundStart NOT FOUND" -ForegroundColor Red
        
        # Find any type containing "RoundStart"
        $rsTypes = $asc.GetExportedTypes() | Where-Object { $_.Name -match 'RoundStart' }
        foreach ($t in $rsTypes) {
            Write-Host "  Found: $($t.FullName) : $($t.BaseType.FullName)" -ForegroundColor Cyan
        }
    }
    
    # 5. CommandSender class
    $csType = Find-Type $asc "CommandSender"
    if ($csType) {
        Write-Host "`n--- CommandSender: $($csType.FullName) ---" -ForegroundColor Yellow
        Write-Host "  Base: $($csType.BaseType.FullName)"
        
        $props = $csType.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($p in $props) {
            $rw = ""
            if ($p.CanRead) { $rw += "get;" }
            if ($p.CanWrite) { $rw += "set;" }
            Write-Host "  PROP: $($p.PropertyType.Name) $($p.Name) {$rw}"
        }
    }
    
} catch {
    Write-Host "  Assembly-CSharp.dll LOAD FAILED: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nDone." -ForegroundColor Green
