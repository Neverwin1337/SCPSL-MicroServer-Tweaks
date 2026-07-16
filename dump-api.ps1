# Run on Windows PowerShell in project root. Paste ALL output back.
# powershell -ExecutionPolicy Bypass -File "dump-api.ps1" > api-dump.txt

$ErrorActionPreference = "Continue"
$binDir = "$PSScriptRoot\Binaries"

function Dump-All($dll) {
    $path = "$binDir\$dll"
    if (!(Test-Path $path)) { 
        "=== $dll NOT FOUND ==="
        return 
    }
    
    try {
        $asm = [System.Reflection.Assembly]::LoadFrom($path)
    } catch {
        "=== $dll LOAD FAILED: $($_.Exception.Message) ==="
        return
    }
    
    "`n========== $dll =========="
    $types = $asm.GetExportedTypes() | Sort-Object FullName
    
    foreach ($t in $types) {
        $isEnum = $t.IsEnum
        $isIface = $t.IsInterface
        $kind = if ($isEnum) { "enum" } elseif ($isIface) { "interface" } else { "class" }
        
        # Skip compiler-generated types
        if ($t.FullName -match '<|AnonymousType|__StaticArrayInit') { continue }
        
        # Print type header
        if ($t.BaseType -and $t.BaseType.FullName -ne "System.Object" -and $t.BaseType.FullName -ne "System.Enum" -and $t.BaseType.FullName -ne "System.MulticastDelegate" -and $t.BaseType.FullName -ne "System.ValueType") {
            "  $kind $($t.FullName) : $($t.BaseType.FullName)"
        } else {
            "  $kind $($t.FullName)"
        }

        # Enums: list values
        if ($isEnum) {
            $vals = [System.Enum]::GetNames($t)
            foreach ($v in $vals) {
                "      $v"
            }
            continue
        }

        # Properties
        $props = $t.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { $_.GetMethod -and !$_.GetMethod.IsSpecialName }
        foreach ($p in $props) {
            $rw = ""
            if ($p.CanRead) { $rw += "get;" }
            if ($p.CanWrite) { $rw += "set;" }
            "      PROP $($p.PropertyType) $($p.Name) {$rw}"
        }

        # Methods (public, instance/static, declared)
        $methods = $t.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly) | Where-Object { !$_.IsSpecialName -and !$_.Name.StartsWith("get_") -and !$_.Name.StartsWith("set_") }
        foreach ($m in $methods) {
            $stat = if ($m.IsStatic) { "static" } else { "" }
            $virt = if ($m.IsVirtual -and !$m.IsFinal) { "virtual" } else { "" }
            $ovrd = if ($m.GetBaseDefinition().DeclaringType -ne $t) { "override" } else { "" }
            if ($ovrd) { $virt = "" }
            $flags = @($stat, $virt, $ovrd) | Where-Object { $_ } -join " "
            $prms = ($m.GetParameters() | ForEach-Object { "$($_.ParameterType) $($_.Name)" }) -join ", "
            "      $flags $($m.ReturnType) $($m.Name)($prms)"
        }

        # Static fields (for singletons)
        $fields = $t.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)
        foreach ($f in $fields) {
            "      STATIC FIELD $($f.FieldType) $($f.Name)"
        }
    }
}

Dump-All "LabApi.dll"
Dump-All "Assembly-CSharp.dll"
