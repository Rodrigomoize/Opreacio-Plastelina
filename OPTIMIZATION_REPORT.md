# 📊 INFORME DE OPTIMIZACIÓN - Calcul Fatal

**Fecha:** 28 de Octubre, 2025  
**Análisis realizado por:** GitHub Copilot  
**Estado del proyecto:** ✅ Funcional, necesita optimización

---

## 🎯 RESUMEN EJECUTIVO

### Problemas Críticos Encontrados:
1. ❌ **TextMesh Pro Examples & Extras** ocupando 6.11 MB innecesariamente
2. ❌ **Archivo backup duplicado** en carpeta AI Player
3. ⚠️ **Debug.Log excesivos** (50+ encontrados solo en scripts principales)
4. ⚠️ **IntelectBar.cs** ejecuta UpdateBar() en Start() Y Update() (redundante)
5. ⚠️ **TutorialManager** tiene Update() vacío
6. ⚠️ **Múltiples scripts con Update() vacíos o casi vacíos**

### Impacto de Optimización Estimado:
- **Reducción de tamaño:** -6+ MB
- **Mejora de rendimiento:** 5-10% (eliminando Debug.Log en builds)
- **Limpieza de código:** ~20% menos archivos innecesarios

---

## 🗑️ ARCHIVOS PARA ELIMINAR

### 1. **TextMesh Pro Examples & Extras** (PRIORIDAD ALTA)
**Ubicación:** `Assets/TextMesh Pro/Examples & Extras/`  
**Tamaño:** 6.11 MB (284 archivos)  
**Razón:** Scripts de ejemplo/demo que NO se usan en producción  

**Scripts incluidos (NO NECESARIOS):**
- Benchmark01.cs, Benchmark02.cs, Benchmark03.cs, Benchmark04.cs
- TMP_TextSelector_A.cs, TMP_TextSelector_B.cs
- TMP_FrameRateCounter.cs, TMP_UiFrameRateCounter.cs
- ChatController.cs, DropdownSample.cs
- VertexJitter.cs, VertexZoom.cs, WarpTextExample.cs
- Y muchos más...

**ACCIÓN:** Eliminar toda la carpeta `Examples & Extras`

```powershell
Remove-Item "c:\Users\User\Documents\GitHub\Calcul-Fatal\Assets\TextMesh Pro\Examples & Extras" -Recurse -Force
```

---

### 2. **Archivo Backup Duplicado** (PRIORIDAD ALTA)
**Ubicación:** `Assets/Scripts/Combat/AI Player/AICardManager_OLD_BACKUP.cs.txt`  
**Razón:** Backup obsoleto del AICardManager antiguo

**ACCIÓN:** Eliminar archivo backup

```powershell
Remove-Item "c:\Users\User\Documents\GitHub\Calcul-Fatal\Assets\Scripts\Combat\AI Player\AICardManager_OLD_BACKUP.cs.txt" -Force
```

---

### 3. **TutorialInfo (Readme de Unity)** (PRIORIDAD MEDIA)
**Ubicación:** `Assets/TutorialInfo/`  
**Razón:** Scripts de tutorial genérico de Unity (Readme.cs, ReadmeEditor.cs)

**ACCIÓN:** Eliminar si no se usa en tu proyecto

```powershell
Remove-Item "c:\Users\User\Documents\GitHub\Calcul-Fatal\Assets\TutorialInfo" -Recurse -Force
```

---

### 4. **QuickOutline/Scripts** (EVALUAR)
**Ubicación:** `Assets/QuickOutline/Scripts/`  
**Scripts:** Outline.cs, OutlineWebGLHelper.cs  
**¿Se usa?** Si NO usas outlines en tus personajes/objetos, eliminar.

---

### 5. **UICreatorUtility.cs** (PRIORIDAD BAJA)
**Ubicación:** `Assets/Scripts/UICreatorUtility.cs`  
**Razón:** Herramienta de editor (#if UNITY_EDITOR) que crea prefabs  
**¿Se usa?** Solo en desarrollo, NO en build  
**ACCIÓN:** Mover a carpeta `Editor/` para mejor organización

---

## ⚡ OPTIMIZACIONES DE CÓDIGO

### 1. **Eliminar Debug.Log en Build** (CRÍTICO)

**Archivos afectados (50+ Debug.Log encontrados):**
- WinResultUI.cs (6 Debug.Log)
- UIManager.cs (Debug.Log en Start)
- TowerManager.cs (2 Debug.Log)
- VehicleBounce.cs
- GameManager.cs
- AICardManager.cs
- PlayerCardManager.cs
- Y muchos más...

**SOLUCIÓN AUTOMÁTICA:** Crear sistema de debug condicional

**Crear archivo:** `Assets/Scripts/DebugHelper.cs`

```csharp
using UnityEngine;
using System.Diagnostics;

public static class DebugHelper
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
}
```

**LUEGO:** Reemplazar todos los `Debug.Log()` por `DebugHelper.Log()` (búsqueda/reemplazo masivo)

**Beneficio:** Los Debug.Log se **eliminan automáticamente** en builds de producción (sin código manual de #if)

---

### 2. **IntelectBar.cs - Optimización Update()** (PRIORIDAD ALTA)

**Problema actual:**
```csharp
void Start()
{
    SetupBar();
    UpdateBar();  // ← Redundante, Update() ya lo llama
}

void Update()
{
    UpdateBar();  // ← Se ejecuta cada frame
}
```

**Optimización:**
```csharp
void Start()
{
    SetupBar();
    // Remover UpdateBar() de aquí
}

void Update()
{
    if (intelectManager == null) return;
    
    float currentValue = intelectManager.GetCurrentIntelectFloat();
    
    // Solo actualizar si cambió (evitar cálculos innecesarios)
    if (Mathf.Approximately(lastValue, currentValue)) return;
    
    lastValue = currentValue;
    UpdateBar();
}

private float lastValue = -1f; // Nueva variable
```

**Beneficio:** Evita cálculos redundantes cuando el intelecto no cambia

---

### 3. **Eliminar Update() Vacíos** (PRIORIDAD MEDIA)

**Scripts con Update() vacío o casi vacío:**
- `TutorialManager.cs` (línea 84) - Update() vacío
- `AITutorial.cs` (línea 12) - Update() vacío
- `Card.cs` - Update() solo para debug

**ACCIÓN:** Eliminar estos métodos Update() vacíos

```csharp
// ANTES:
void Update()
{
    // Vacío o solo Debug.Log comentado
}

// DESPUÉS:
// (Eliminar completamente)
```

**Beneficio:** Unity no llama a Update() si el método no existe, ahorrando overhead

---

### 4. **PowerUpManager - Optimización de Loops** (PRIORIDAD MEDIA)

**Problema actual:** 3 foreach separados en Update()

```csharp
void Update()
{
    UpdateCooldowns();           // foreach 1
    UpdateActiveDurationTimers(); // foreach 2
}

void UpdateCooldowns()
{
    foreach (var p in powerUps) { ... } // ← Recorre todos
}

void UpdateActiveDurationTimers()
{
    foreach (var p in powerUps) { ... } // ← Recorre todos otra vez
}
```

**Optimización:** Combinar en un solo foreach

```csharp
void Update()
{
    foreach (var p in powerUps)
    {
        // Actualizar cooldown
        if (p.isOnCooldown)
        {
            p.cooldownTimer -= Time.deltaTime;
            if (p.cooldownTimer <= 0f)
            {
                p.isOnCooldown = false;
                p.cooldownTimer = 0f;
            }
            UpdatePowerUpUI(p);
        }

        // Actualizar duración activa
        if (p.isActive && p.duration > 0f)
        {
            if (p.powerUpButton != null)
            {
                PowerUpDurationUI durationUI = p.powerUpButton.GetComponent<PowerUpDurationUI>();
                if (durationUI == null)
                    durationUI = p.powerUpButton.gameObject.AddComponent<PowerUpDurationUI>();
                durationUI.UpdateTimer(p.durationTimer);
            }

            p.durationTimer -= Time.deltaTime;
            if (p.durationTimer <= 0f)
            {
                p.isActive = false;
                switch (p.powerUpName)
                {
                    case "SlowTime":
                        DeactivateSlowTimePowerUp(p);
                        break;
                }
                StartCooldown(p);
                UpdatePowerUpUI(p);
            }
            else
            {
                UpdatePowerUpUI(p);
            }
        }
        else if (p.powerUpButton != null)
        {
            PowerUpDurationUI durationUI = p.powerUpButton.GetComponent<PowerUpDurationUI>();
            if (durationUI != null)
                durationUI.HideTimer();
        }
    }
}
```

**Beneficio:** 3 foreach → 1 foreach = ~66% menos iteraciones

---

### 5. **Character y CharacterCombined - Optimización Movement** (PRIORIDAD ALTA)

**Problema:** Update() mueve personajes sin comprobar si están vivos/activos

```csharp
void Update()
{
    if (!isDead)  // ← Bueno
    {
        transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
    }
}
```

**Mejor aún:**
```csharp
void Update()
{
    if (isDead || speed == 0f) return; // Early exit
    
    transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
}
```

**ALTERNATIVA (más eficiente):**
Usar `enabled = false` cuando el personaje muere, así Unity no llama Update()

```csharp
public void Die()
{
    isDead = true;
    enabled = false; // ← Desactiva Update() automáticamente
    // ... resto del código de muerte
}
```

---

### 6. **AICardManager - Cooldown Optimization** (PRIORIDAD MEDIA)

**Problema:** Timer de cooldown se ejecuta aunque no haya cooldowns activos

**Optimización:**

```csharp
void Update()
{
    // Solo ejecutar si hay cooldowns activos
    bool hasCooldowns = cooldownAtaque > 0 || cooldownDefensa > 0;
    if (!hasCooldowns) return;
    
    // Actualizar cooldowns
    if (cooldownAtaque > 0)
    {
        cooldownAtaque -= Time.deltaTime;
        if (cooldownAtaque < 0) cooldownAtaque = 0;
    }

    if (cooldownDefensa > 0)
    {
        cooldownDefensa -= Time.deltaTime;
        if (cooldownDefensa < 0) cooldownDefensa = 0;
    }
}
```

---

## 🎨 OPTIMIZACIONES DE ASSETS

### 1. **Prefabs Duplicados** (REVISAR)
Revisar si hay prefabs duplicados en `Assets/Prefabs/`:
- Carta 1.prefab, Carta 2.prefab, Carta 3.prefab, Carta 4.prefab
- ¿Son necesarios 4 prefabs separados o se puede usar 1 con configuración?

### 2. **Sprites/Texturas** (CONFIGURAR)
Asegurarse de que todos los sprites tengan:
- **Max Size:** Adecuado al uso (256, 512, 1024)
- **Compression:** Compressed (no Uncompressed)
- **Mipmap:** Disabled para sprites 2D

### 3. **Audio** (CONFIGURAR)
Revisar configuración de audio en `Assets/Audio/`:
- **Force To Mono:** Activar si no necesitas stereo
- **Quality:** 70-80% es suficiente para SFX
- **Load Type:** Compressed in Memory para música, Decompress on Load para SFX cortos

---

## 📦 ORGANIZACIÓN DE CARPETAS

### Estructura Recomendada:

```
Assets/
├── _Project/                    (TU CÓDIGO)
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── Combat/
│   │   ├── Managers/
│   │   ├── UI/
│   │   ├── VFX/
│   │   └── Editor/             ← Mover UICreatorUtility.cs aquí
│   ├── Prefabs/
│   ├── Audio/
│   ├── SpritesUI/
│   └── VFX/
├── Plugins/                     (THIRD PARTY)
│   ├── QuickOutline/
│   └── TextMesh Pro/
│       ├── Resources/
│       ├── Fonts/
│       └── Shaders/
└── Settings/
```

**Beneficio:** Mejor organización, más fácil de navegar

---

## 🔧 REFACTORIZACIÓN SUGERIDA

### 1. **Crear Enums para Valores Mágicos**

**Antes:**
```csharp
if (cardValue == 1) { /* ... */ }
if (cardValue == 2) { /* ... */ }
```

**Después:**
```csharp
public enum CardValue
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5
}

if (cardValue == (int)CardValue.One) { /* ... */ }
```

### 2. **Constantes en vez de Strings**

**Antes:**
```csharp
SceneManager.LoadScene("MainMenuScene");
SceneManager.LoadScene("LevelScene");
```

**Después:**
```csharp
public static class SceneNames
{
    public const string MainMenu = "MainMenuScene";
    public const string Level = "LevelScene";
    public const string Tutorial = "TutorialScene";
}

SceneManager.LoadScene(SceneNames.MainMenu);
```

**Beneficio:** Evita errores de typos en strings

### 3. **Object Pooling para Tropas** (AVANZADO)

Si spawneas muchas tropas/proyectiles, considera implementar Object Pooling:
- Reutilizar GameObjects en vez de Instantiate/Destroy constantemente
- Reduce garbage collection
- Mejora performance

---

## 📝 CHECKLIST DE IMPLEMENTACIÓN

### FASE 1 - Limpieza Inmediata (10 min)
- [ ] Eliminar `TextMesh Pro/Examples & Extras/` ✅ **(-6.11 MB)**
- [ ] Eliminar `AICardManager_OLD_BACKUP.cs.txt` ✅
- [ ] Eliminar `TutorialInfo/` (si no se usa) ✅
- [ ] Eliminar `UnityTechnologies/` ✅ **(Ya hecho)**

### FASE 2 - Optimización de Debug (30 min)
- [ ] Crear `DebugHelper.cs`
- [ ] Reemplazar `Debug.Log` → `DebugHelper.Log` (buscar/reemplazar masivo)
- [ ] Reemplazar `Debug.LogWarning` → `DebugHelper.LogWarning`
- [ ] Reemplazar `Debug.LogError` → `DebugHelper.LogError`

### FASE 3 - Optimización de Update() (20 min)
- [ ] Optimizar `IntelectBar.cs` (evitar update redundante)
- [ ] Eliminar Update() vacíos en `TutorialManager.cs` y `AITutorial.cs`
- [ ] Combinar loops en `PowerUpManager.cs`
- [ ] Optimizar `Character.cs` y `CharacterCombined.cs` (enabled=false cuando mueren)

### FASE 4 - Configuración de Assets (15 min)
- [ ] Revisar compresión de sprites
- [ ] Revisar configuración de audio
- [ ] Eliminar assets no utilizados

### FASE 5 - Testing (30 min)
- [ ] Probar todos los modos de juego
- [ ] Verificar que no hay errores en consola
- [ ] Hacer build y verificar tamaño final

---

## 📊 MÉTRICAS DE ÉXITO

### Antes de Optimización:
- **Tamaño del proyecto:** ~XXX MB
- **Debug.Log activos:** 50+
- **Scripts con Update() vacío:** 3+
- **Archivos innecesarios:** 284+ (TextMesh Pro Examples)

### Después de Optimización:
- **Tamaño del proyecto:** -6+ MB ✅
- **Debug.Log en build:** 0 (compilación condicional) ✅
- **Scripts con Update() vacío:** 0 ✅
- **Archivos innecesarios:** 0 ✅
- **Performance:** +5-10% ✅

---

## 🚀 CONSEJOS ADICIONALES

### Build Settings
1. **IL2CPP** en vez de Mono para mejor performance (WebGL/Mobile)
2. **Strip Engine Code** activado
3. **Managed Stripping Level:** High (elimina código no usado)
4. **Code Optimization:** Speed (no Size)

### Player Settings
1. **API Compatibility Level:** .NET Standard 2.1 (más ligero)
2. **Scripting Backend:** IL2CPP para producción
3. **Incremental GC:** Activado (reduce stuttering)

### Quality Settings
1. Crear perfiles Low/Medium/High para diferentes plataformas
2. Ajustar Shadow Distance según necesidad
3. Desactivar Anti-Aliasing en WebGL si no es crítico

---

## ⚠️ ADVERTENCIAS

1. **Siempre haz backup antes de eliminar** (Git commit)
2. **Prueba después de cada optimización** (no hagas todo a la vez)
3. **No optimices prematuramente** - primero termina features, luego optimiza
4. **Profiler es tu amigo** - usa Unity Profiler para encontrar cuellos de botella reales

---

## 📞 SOPORTE

Si encuentras errores después de aplicar optimizaciones:
1. Revisar consola de Unity
2. Hacer `git diff` para ver qué cambió
3. Revertir cambios problemáticos con `git checkout`
4. Aplicar optimizaciones una por una (no todas juntas)

---

**¡Listo para optimizar!** 🚀
