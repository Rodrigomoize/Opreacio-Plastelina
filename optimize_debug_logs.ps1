# Script de Optimización Automática para Calcul Fatal
# Reemplaza Debug.Log por DebugHelper.Log en todos los scripts

Write-Host "🚀 Iniciando optimización automática de scripts..." -ForegroundColor Green

$scriptsPath = "c:\Users\User\Documents\GitHub\Calcul-Fatal\Assets\Scripts"
$csFiles = Get-ChildItem -Path $scriptsPath -Filter *.cs -Recurse

$totalFiles = 0
$modifiedFiles = 0

foreach ($file in $csFiles) {
    # Ignorar DebugHelper.cs
    if ($file.Name -eq "DebugHelper.cs") {
        continue
    }
    
    $totalFiles++
    $content = Get-Content -Path $file.FullName -Raw
    
    # Hacer los reemplazos
    $originalContent = $content
    $content = $content -replace 'Debug\.Log\(', 'DebugHelper.Log('
    $content = $content -replace 'Debug\.LogWarning\(', 'DebugHelper.LogWarning('
    $content = $content -replace 'Debug\.LogError\(', 'DebugHelper.LogError('
    
    # Solo guardar si hubo cambios
    if ($content -ne $originalContent) {
        try {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            $modifiedFiles++
            Write-Host "✅ Optimizado: $($file.Name)" -ForegroundColor Yellow
        }
        catch {
            Write-Host "❌ Error en $($file.Name): $_" -ForegroundColor Red
        }
    }
}

Write-Host "`n📊 RESUMEN:" -ForegroundColor Cyan
Write-Host "   Archivos analizados: $totalFiles" -ForegroundColor White
Write-Host "   Archivos modificados: $modifiedFiles" -ForegroundColor Green
Write-Host "`n✨ Optimización completada!" -ForegroundColor Green
Write-Host "   Los Debug.Log ahora se eliminan automáticamente en builds de producción." -ForegroundColor White
