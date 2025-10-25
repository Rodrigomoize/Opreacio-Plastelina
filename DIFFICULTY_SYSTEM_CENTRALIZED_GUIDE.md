# Sistema Centralizado de Dificultad

## 📋 Resumen

Todo el sistema de dificultad ahora está **centralizado en un único manager**: `DifficultyManager`. Este gestiona:
- ✅ Duración de la partida
- ✅ Velocidad de las tropas (delegado a `GameSpeedManager`)
- ✅ Configuración de la IA (delegado a `IAController`)

## 🏗️ Arquitectura

```
DifficultyPanel (UI)
       ↓
DifficultyManager (Central)
       ↓
    ┌──┴──────────────┬────────────────┐
    ↓                 ↓                ↓
GameTimer      GameSpeedManager   IAController
(Duración)     (Velocidad)        (IA)
```

## 🎮 Configuración por Dificultad

### Fácil
- **Duración**: 240s (4 minutos)
- **Velocidad Tropas**: 40% (0.4x) - Configurado en `GameSpeedManager`
- **IA Agresividad**: 0.43 - Configurado en `IAController`
- **IA Intervalo**: 1.5s - Configurado en `IAController`
- **Regen Intelecto**: 3.5s - Configurado en `IAController`

### Media
- **Duración**: 180s (3 minutos)
- **Velocidad Tropas**: 70% (0.7x)
- **IA Agresividad**: 0.6
- **IA Intervalo**: 1.0s
- **Regen Intelecto**: 2.8s

### Difícil
- **Duración**: 120s (2 minutos)
- **Velocidad Tropas**: 100% (1.0x)
- **IA Agresividad**: 0.8
- **IA Intervalo**: 0.6s
- **Regen Intelecto**: 2.2s

## 📁 Scripts Involucrados

### 1. `DifficultyManager.cs` (NUEVO - Central)
**Ubicación**: `Assets/Scripts/Managers/DifficultyManager.cs`

**Responsabilidades**:
- Almacenar la dificultad actual
- Coordinar la aplicación de ajustes a todos los sistemas
- Proporcionar la duración de partida según dificultad

**Métodos Principales**:
```csharp
// Establece la dificultad y aplica todos los cambios
public void SetDifficulty(IAController.AIDificultad difficulty)

// Aplica los ajustes a GameSpeedManager e IAController
public void ApplyDifficultySettings()

// Obtiene la duración para la dificultad actual
public float GetGameDuration()

// Info de debug de los ajustes actuales
public string GetCurrentSettingsInfo()
```

### 2. `GameManager.cs` (MODIFICADO)
**Cambios**:
- Ahora **delega** toda la gestión de dificultad al `DifficultyManager`
- `SetDificultad()` llama internamente a `DifficultyManager.SetDifficulty()`
- `GetDurationForDifficulty()` obtiene la duración del `DifficultyManager`
- Se eliminaron las variables `duracionFacil`, `duracionMedia`, `duracionDificil` y `currentDifficulty`

### 3. `DifficultyPanel.cs` (MODIFICADO)
**Cambios**:
- Los botones ahora llaman a `DifficultyManager.Instance.SetDifficulty()`
- Mantiene compatibilidad con `GameManager` como fallback

### 4. `GameSpeedManager.cs` (SIN CAMBIOS)
**Responsabilidades**:
- Controla la velocidad de movimiento de las tropas
- Ya tiene configuraciones internas por dificultad

### 5. `IAController.cs` (SIN CAMBIOS)
**Responsabilidades**:
- Controla la agresividad, intervalo de decisión y regeneración de intelecto
- Ya tiene configuraciones internas por dificultad

## 🔄 Flujo de Ejecución

### Al Seleccionar Dificultad (Menú)

1. Usuario presiona botón de dificultad en `DifficultyPanel`
2. `DifficultyPanel` llama a `DifficultyManager.SetDifficulty(dificultad)`
3. `DifficultyManager` guarda la dificultad seleccionada
4. `DifficultyManager` llama a `ApplyDifficultySettings()`
5. Se aplica la configuración:
   - `GameSpeedManager.SetSpeedByDifficulty()` → Velocidad de tropas
   - `IAController.SetDificultad()` → Agresividad, intervalo, regeneración
6. Se carga la escena `PlayScene`

### Al Cargar PlayScene

1. `GameManager.HandleSceneLoaded()` detecta la carga de `PlayScene`
2. Se resetea el `GameTimer`
3. Se obtiene la duración del `DifficultyManager.GetGameDuration()`
4. Se inicia la cuenta atrás con la duración correspondiente
5. Se aplican nuevamente los ajustes con `ApplyDifficultySpeed()`

## 🛠️ Configuración en Unity

### Setup del DifficultyManager

1. **Crear GameObject**:
   - En la escena de inicio, crear un GameObject vacío llamado `DifficultyManager`
   
2. **Añadir Componente**:
   - Añadir el componente `DifficultyManager.cs`
   
3. **Configurar Duraciones**:
   En el Inspector, ajustar los valores:
   - `Duracion Facil`: 240 (por defecto)
   - `Duracion Media`: 180 (por defecto)
   - `Duracion Dificil`: 120 (por defecto)

4. **DontDestroyOnLoad**:
   - El script ya se marca automáticamente como persistente

### Actualizar GameManager

1. Seleccionar el GameObject `GameManager`
2. En el Inspector, en la sección `References`, asignar:
   - `Difficulty Manager`: Arrastrar el GameObject `DifficultyManager`

## 📊 Ventajas de la Centralización

### ✅ Antes (Distribuido)
- ❌ Duración en `GameManager`
- ❌ Velocidad en `GameSpeedManager`
- ❌ IA en `IAController`
- ❌ Difícil de mantener y ajustar
- ❌ Configuraciones duplicadas

### ✅ Ahora (Centralizado)
- ✅ Un único punto de control: `DifficultyManager`
- ✅ Fácil ver todas las configuraciones de un vistazo
- ✅ Cambios coordinados automáticamente
- ✅ Cada manager mantiene su configuración interna
- ✅ `DifficultyManager` solo coordina, no duplica

## 🔍 Debugging

### Ver Configuración Actual
```csharp
if (DifficultyManager.Instance != null)
{
    Debug.Log(DifficultyManager.Instance.GetCurrentSettingsInfo());
}
```

Esto mostrará:
```
=== CONFIGURACIÓN DE DIFICULTAD: Media ===
Duración de Partida: 180s (3.0 minutos)
Velocidad de Juego: 0.7x
IA Agresividad: 0.60
IA Intervalo Decisión: 1.00s
```

## 🎯 Próximos Pasos

Si quieres añadir más ajustes por dificultad:

1. **Identifica qué ajustar** (ej: vida de torres, daño de tropas, etc.)
2. **Configura en el manager correspondiente** (ej: `TowerManager`, `CharacterManager`)
3. **Llama desde `DifficultyManager.ApplyDifficultySettings()`** para coordinar

**Ejemplo**:
```csharp
// En DifficultyManager.ApplyDifficultySettings()
TowerManager towerManager = GameManager.Instance?.GetTowerManager();
if (towerManager != null)
{
    towerManager.SetDifficulty(currentDifficulty);
}
```

## 📝 Notas Importantes

- El `DifficultyManager` **NO duplica configuraciones**, solo coordina
- Cada sistema (`GameSpeedManager`, `IAController`) mantiene sus propios ajustes internos
- El `DifficultyManager` solo gestiona **la duración de partida directamente**
- Todo lo demás se delega a los managers correspondientes
- Esto mantiene la separación de responsabilidades y evita acoplamiento
