# Guía de Configuración: Sistema de Spawn de Tropas

## 📋 Descripción General

Este sistema añade un **tiempo de spawn** a las tropas (Character y CharacterCombined), durante el cual:
- ✨ Se reproduce un VFX de plastelina saliendo del suelo
- 🛡️ La tropa **no puede atacar** ni **ser atacada**
- ⏱️ Se muestra un **temporizador visual** en la UI de la tropa
- 📈 La tropa crece gradualmente desde una escala pequeña

---

## 📁 Archivos Creados/Modificados

### Nuevos Archivos:
1. **`TroopSpawnController.cs`** - Controla el estado de spawn
2. **`TroopSpawnVFX.cs`** - Script para el VFX de spawn
3. **`TROOP_SPAWN_GUIDE.md`** - Esta guía

### Archivos Modificados:
1. **`Character.cs`** - Integración del sistema de spawn
2. **`CharacterCombined.cs`** - Integración del sistema de spawn
3. **`TroopUI.cs`** - Temporizador visual de spawn

---

## 🔧 Configuración en Unity

### 1. **Configurar los Prefabs de Tropas**

#### Para Character (Tropa Simple):
1. Abre el prefab de tu tropa en `Assets/Prefabs/Characters/`
2. **Añadir el componente TroopSpawnController:**
   - Click en "Add Component"
   - Busca "Troop Spawn Controller"
   - Configura los parámetros:
     - **Spawn Duration**: `2.0` segundos (ajustable)
     - **Spawn VFX Prefab**: Arrastra el prefab VFX de spawn
     - **Initial Scale**: `0` (empieza invisible)
     - **Growth Speed**: `2` (velocidad de crecimiento)

#### Para CharacterCombined (Operación):
1. Abre el prefab de operación en `Assets/Prefabs/Characters/`
2. **Añadir el componente TroopSpawnController** con los mismos pasos

---

### 2. **Crear el VFX de Spawn**

#### Opción A: Crear desde Cero
1. Crea un nuevo GameObject vacío: `VFX_TroopSpawn`
2. Añade el componente **TroopSpawnVFX**
3. **Añadir Particle Systems:**
   - Sistema 1: **Dirt Particles** (partículas de tierra)
     - Shape: Circle, Radio: 0.5
     - Start Lifetime: 1-2
     - Start Speed: 2-4
     - Start Color: Marrón/tierra
     - Emission: 20 partículas
   
   - Sistema 2: **Clay Particles** (partículas de plastelina)
     - Shape: Circle, Radio: 0.3
     - Start Lifetime: 0.5-1
     - Start Speed: 1-3
     - Start Color: Color de la tropa
     - Emission: 10 partículas

4. **Añadir Ground Ripple (Opcional):**
   - Crea un Quad plano rotado 90° en X
   - Asigna un material con textura de onda circular
   - Escala inicial: 0.1

5. **Asignar referencias en TroopSpawnVFX:**
   - Dirt Particles → Sistema 1
   - Clay Particles → Sistema 2
   - Ground Ripple → Quad
   - Ripple Max Scale: `2`
   - Ripple Duration: `1`
   - Lifetime: `2.5`

6. Guarda como Prefab en `Assets/VFX/`

#### Opción B: Duplicar VFX Existente
1. Duplica `vfx_plastilinaImpact.prefab`
2. Renómbralo a `vfx_troopSpawn.prefab`
3. Modifica el script para usar `TroopSpawnVFX`
4. Ajusta las partículas para que vayan hacia arriba

---

### 3. **Configurar TroopUI**

1. Abre el prefab `TroopUI` en `Assets/Prefabs/UI/`
2. **Añadir Temporizador de Spawn:**
   - Crea un nuevo GameObject hijo: `SpawnTimerContainer`
   - Añade un **Panel** con fondo semi-transparente
   - Dentro del panel añade:
     - **TextMeshPro**: Para mostrar el tiempo (ej: "1.5s")
     - **Image** (opcional): Barra de progreso con Type: Filled

3. **Configurar el componente TroopUI:**
   - **Spawn Timer Container**: Arrastra el panel creado
   - **Spawn Timer Text**: Arrastra el TextMeshPro
   - **Spawn Progress Bar**: Arrastra la Image (si la creaste)

4. **Estilo recomendado:**
   - Posición: Encima del icono principal
   - Tamaño: Pequeño (50x20 aprox)
   - Color fondo: Semi-transparente (negro 50% alpha)
   - Color texto: Blanco brillante
   - Font Size: 12-14

---

## ⚙️ Parámetros Ajustables

### TroopSpawnController:
```
spawnDuration: 2.0f         // Duración total del spawn
initialScale: 0.0f          // Escala inicial (0 = invisible)
growthSpeed: 2.0f           // Velocidad de crecimiento
```

### TroopSpawnVFX:
```
rippleMaxScale: 2.0f        // Tamaño máximo de la onda
rippleDuration: 1.0f        // Duración de la animación
lifetime: 2.5f              // Tiempo antes de auto-destruirse
```

---

## 🎮 Cómo Funciona

### Secuencia de Spawn:

1. **Inicio (t=0s):**
   - Tropa aparece con escala 0 (invisible)
   - Collider deshabilitado
   - VFX de spawn se instancia
   - Temporizador visible en UI

2. **Durante Spawn (t=0s - 2s):**
   - Tropa crece gradualmente
   - Partículas de tierra/plastelina emergen
   - Temporizador cuenta regresiva
   - Barra de progreso se llena
   - **No puede atacar ni ser atacada**

3. **Finalización (t=2s):**
   - Escala completa alcanzada
   - Collider habilitado
   - VFX destruido
   - Temporizador oculto
   - **Tropa completamente activa**

---

## 🔍 Verificación de Colisiones

El sistema modifica `Character.cs` y `CharacterCombined.cs` para verificar el estado de spawn:

```csharp
// En OnTriggerEnter:
if (spawnController != null && !spawnController.CanAttack()) return;

// Al atacar a otro:
TroopSpawnController enemySpawnController = other.GetComponent<TroopSpawnController>();
if (enemySpawnController != null && !enemySpawnController.CanBeAttacked()) return;
```

---

## 🎨 Personalización Visual

### Colores del Temporizador:
Puedes cambiar el color del texto según el tiempo restante:

1. Modifica `TroopUI.UpdateSpawnTimer()`:
```csharp
if (spawnTimerText != null)
{
    spawnTimerText.text = $"{timeRemaining:F1}s";
    
    // Color gradiente: rojo → amarillo → verde
    float progress = 1f - (timeRemaining / 2f); // Asume 2s de spawn
    spawnTimerText.color = Color.Lerp(Color.red, Color.green, progress);
}
```

### Animaciones Adicionales:
- Pulso en el icono durante spawn
- Partículas de color del equipo
- Sonido de "surgir" del suelo

---

## 🐛 Solución de Problemas

### Problema: "Type or namespace name 'TroopSpawnController' could not be found"
**Solución:**
1. En Unity, ve a `Assets → Reimport All`
2. O cierra y reabre Unity
3. Verifica que `TroopSpawnController.cs` esté en la carpeta correcta

### Problema: Las tropas no crecen
**Solución:**
- Verifica que `growthSpeed > 0`
- Asegúrate de que `targetScale` no es Vector3.zero
- Revisa que el script esté habilitado

### Problema: El temporizador no aparece
**Solución:**
- Verifica que `spawnTimerContainer` esté asignado
- Confirma que el objeto no esté deshabilitado en el prefab
- Revisa que `troopUIInstance` se cree correctamente

### Problema: Las tropas siguen siendo atacables durante spawn
**Solución:**
- Verifica que el Collider se deshabilite en `Awake()`
- Confirma que `CanBeAttacked()` retorna false
- Revisa los logs de Debug para ver si se detectan las colisiones

---

## 🎵 Audio (Opcional)

Para añadir sonido al spawn:

1. **En AudioManager**, añade:
```csharp
public AudioClip troopSpawnSound;

public void PlayTroopSpawn()
{
    PlaySound(troopSpawnSound);
}
```

2. **En TroopSpawnController.Start():**
```csharp
if (AudioManager.Instance != null)
{
    AudioManager.Instance.PlayTroopSpawn();
}
```

3. **En AudioManagerBridge**, añade:
```csharp
public void PlayTroopSpawn()
{
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayTroopSpawn();
}
```

---

## ✅ Checklist de Configuración

- [ ] TroopSpawnController.cs creado en `Assets/Scripts/Combat/`
- [ ] TroopSpawnVFX.cs creado en `Assets/Scripts/vfx/`
- [ ] Character.cs modificado con verificaciones de spawn
- [ ] CharacterCombined.cs modificado con verificaciones de spawn
- [ ] TroopUI.cs modificado con temporizador
- [ ] VFX de spawn creado como prefab
- [ ] Prefabs de tropas tienen TroopSpawnController
- [ ] TroopUI tiene elementos de temporizador configurados
- [ ] Referencias asignadas en Inspector
- [ ] Probado en juego: tropas crecen correctamente
- [ ] Probado: tropas no son atacables durante spawn
- [ ] Probado: temporizador se muestra y oculta correctamente

---

## 📝 Notas Adicionales

- El sistema es **modular**: puedes ajustar el tiempo de spawn por tipo de tropa
- Compatible con el sistema de velocidad de juego existente
- No afecta el rendimiento (solo activo durante 2 segundos por tropa)
- Fácil de extender para otros efectos (invulnerabilidad temporal, shields, etc.)

---

¡Sistema de Spawn completado! 🎉
