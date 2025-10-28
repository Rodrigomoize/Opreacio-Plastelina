# Script para eliminar todos los Debug.Log (mantener solo Warning y Error)
# Se ejecuta desde la raíz del proyecto

$scriptsPath = "Assets\Scripts"
$excludeFile = "DebugHelper.cs"
$dryRun = $false  # Cambiar a $true para ver qué se haría sin modificar archivos

$totalFiles = 0
$modifiedFiles = 0
$totalLogsRemoved = 0

Write-Host "Buscando archivos .cs en $scriptsPath..." -ForegroundColor Cyan

Get-ChildItem -Path $scriptsPath -Filter "*.cs" -Recurse | ForEach-Object {
    $file = $_
    $totalFiles++
    
    # Saltar DebugHelper.cs
    if ($file.Name -eq $excludeFile) {
        Write-Host "Saltando $($file.Name)" -ForegroundColor Yellow
        return
    }
    
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $logsInFile = 0
    
    # Eliminar Debug.Log() completos (no Warning ni Error)
    # Patrón: Debug.Log( seguido de cualquier cosa hasta ); en la misma línea o múltiples líneas
    # Elimina la línea completa incluyendo espacios
    $pattern = '^\s*Debug\.Log\([^;]*\);\s*$'
    
    $lines = $content -split "`r?`n"
    $newLines = @()
    $inMultilineLog = $false
    $multilineBuffer = ""
    $skipCount = 0
    
    foreach ($line in $lines) {
        # Detectar inicio de Debug.Log (no Warning/Error)
        if ($line -match '^\s*Debug\.Log\(') {
            $logsInFile++
            $totalLogsRemoved++
            
            # Verificar si es una línea completa
            if ($line -match '\);[\s]*$') {
                # Log completo en una línea - eliminar completamente
                Write-Host "  Removiendo: $($line.Trim())" -ForegroundColor Gray
                continue
            } else {
                # Log multilínea - comenzar buffer
                $inMultilineLog = $true
                $multilineBuffer = $line
                continue
            }
        }
        
        # Si estamos en un log multilínea
        if ($inMultilineLog) {
            $multilineBuffer += "`n" + $line
            
            # Buscar el cierre
            if ($line -match '\);[\s]*$') {
                Write-Host "  Removiendo log multilinea" -ForegroundColor Gray
                $inMultilineLog = $false
                $multilineBuffer = ""
                continue
            }
            continue
        }
        
        # Mantener la línea
        $newLines += $line
    }
    
    $newContent = $newLines -join "`r`n"
    
    if ($logsInFile -gt 0) {
        $modifiedFiles++
        Write-Host "$($file.Name) - $logsInFile logs removidos" -ForegroundColor Green
        
        if (-not $dryRun) {
            Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        }
    }
}

Write-Host ""
Write-Host "Proceso completado" -ForegroundColor Green
Write-Host "Archivos procesados: $totalFiles" -ForegroundColor Cyan
Write-Host "Archivos modificados: $modifiedFiles" -ForegroundColor Cyan
Write-Host "Total Debug.Log() removidos: $totalLogsRemoved" -ForegroundColor Cyan

if ($dryRun) {
    Write-Host ""
    Write-Host "MODO DRY RUN - No se modificaron archivos" -ForegroundColor Yellow
    Write-Host "Cambia `$dryRun = `$false en el script para aplicar cambios" -ForegroundColor Yellow
}
