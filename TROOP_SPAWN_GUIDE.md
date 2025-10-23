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

4. **Crear el Splat de Plastelina (Esfera Aplastada):**
   - Crea una **Sphere** dentro del VFX: `SplatSphere`
   - Escala: `(1, 0.2, 1)` (aplastada en Y)
   - Posición: Justo en el suelo (Y = 0)
   - Material: Material de plastelina con color del equipo
     - Shader: Standard o Unlit
     - Rendering Mode: Transparent (para fade)
     - Color: Color de la tropa

5. **Asignar referencias en TroopSpawnVFX:**
   - Dirt Particles → Sistema 1
   - Clay Particles → Sistema 2
   - **Splat Sphere** → La esfera aplastada
   - **Splat Initial Scale**: `2.0` (empieza grande)
   - **Splat Final Scale**: `0.1` (termina pequeña)
   - **Splat Duration**: `2.0` (misma duración que el spawn)
   - **Splat Curve**: Curva EaseInOut (por defecto)
   - **Lifetime**: `2.5`

6. Guarda como Prefab en `Assets/VFX/`

#### Opción B: Duplicar VFX Existente
1. Duplica `vfx_plastilinaImpact.prefab`
2. Renómbralo a `vfx_troopSpawn.prefab`
3. Modifica el script para usar `TroopSpawnVFX`
4. Ajusta las partículas para que vayan hacia arriba

---

### 🎨 Tips Visuales para el Splat

**Para un efecto más realista:**

1. **Material del Splat:**
   - Usa un Shader con Emission para que brille ligeramente
   - Añade un Normal Map para textura de plastelina
   - Color debe coincidir con el color del equipo (PlayerTeam/AITeam)

2. **Forma del Splat:**
   - Escala Y entre 0.1 y 0.3 para efecto "aplastado"
   - Puedes usar un modelo custom en vez de una esfera
   - Considera añadir un mesh de "salpicadura" irregular

3. **Animación Avanzada:**
   - El splat puede tener un ligero "rebote" al inicio
   - Añade rotación lenta en Y para dinamismo
   - Considera partículas pequeñas que "saltan" del splat

---

### 3. **Configurar TroopUI**

El sistema de UI tiene **dos estados**: UI de Spawn (durante spawneo) y UI Normal (después del spawn).

#### Estructura de UI Requerida:

1. **Crear el Prefab TroopUI** (si no existe):
   - GameObject principal: `TroopUI`
   - Canvas (World Space) como componente

2. **UI Normal Container** (se muestra DESPUÉS del spawn):
   - Crea un GameObject hijo: `NormalUIContainer`
   - Dentro añade:
     - **Image**: `IconImage` - Icono de la tropa
     - **TextMeshPro**: `ValueText` - Número de la tropa (1-5)
   
3. **UI de Spawn Container** (se muestra DURANTE el spawn):
   - Crea un GameObject hijo: `SpawnUIContainer`
   - Dentro añade:
     - **Image**: `SpawnIconImage` - Icono (mismo que el normal)
       - Sprite: Icono del equipo
     - **Image**: `SpawnFillImage` - Barra de progreso radial
       - Image Type: **Filled**
       - Fill Method: **Radial 360**
       - Fill Origin: **Top**
       - Clockwise: ✓
       - Fill Amount: 0 (se anima de 0 a 1)
       - Color: Verde brillante o color del equipo con transparencia
       - Posición: Superpuesta sobre el icono

#### Configuración del Componente TroopUI:

En el Inspector del prefab TroopUI:

**UI Elements:**
- `World Canvas` → Arrastra el Canvas
- `Icon Image` → Imagen del icono en Normal UI
- `Value Text` → TextMeshPro del valor en Normal UI

**Normal Troop UI:**
- `Normal UI Container` → Arrastra el container completo

**Spawn UI:**
- `Spawn UI Container` → Arrastra el container de spawn
- `Spawn Icon Image` → Imagen del icono en Spawn UI
- `Spawn Fill Image` → Imagen con fill radial

**Team Sprites:**
- `Blue Team Icon` → Sprite del equipo azul
- `Red Team Icon` → Sprite del equipo rojo
- `Default Icon` → Sprite por defecto

**Settings:**
- `Offset` → (0, 1.5, 0) - Altura sobre la tropa

#### Estilo Visual Recomendado:

**Spawn UI:**
- Tamaño: 64x64 píxeles
- Icono de fondo: Semi-opaco (50% alpha)
- Fill radial:
  - Color: Verde brillante (#00FF00) o color del equipo
  - Alpha: 70-80%
  - Grosor: Borde de ~8 píxeles alrededor del icono
- Posición: Centrada sobre la tropa

**Normal UI:**
- Tamaño icono: 48x48 píxeles
- Valor numérico:
  - Font Size: 24-28
  - Color: Blanco con outline negro
  - Posición: Esquina inferior derecha del icono o centrado debajo

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
splatInitialScale: 2.0f     // Tamaño inicial del splat (grande)
splatFinalScale: 0.1f       // Tamaño final del splat (pequeño)
splatDuration: 2.0f         // Duración de la contracción
lifetime: 2.5f              // Tiempo antes de auto-destruirse
```

### TroopUI:
```
offset: (0, 1.5, 0)         // Altura sobre la tropa
spawnFillImage.fillAmount   // Se anima de 0 a 1 durante el spawn
```

---

## 🎮 Cómo Funciona

### Secuencia de Spawn:

1. **Inicio (t=0s):**
   - Tropa aparece con escala 0 (invisible)
   - Collider deshabilitado
   - VFX de spawn se instancia
   - **UI de Spawn visible** (icono + fill radial en 0%)

2. **Durante Spawn (t=0s - 2s):**
   - Tropa crece gradualmente desde escala 0 a escala completa
   - Partículas de tierra/plastelina emergen del suelo
   - Splat sphere se contrae (grande → pequeño)
   - **Fill radial se llena** progresivamente (0% → 100%)
   - **No puede atacar ni ser atacada**

3. **Finalización (t=2s):**
   - Escala completa alcanzada
   - Collider habilitado
   - VFX destruido
   - **UI cambia de Spawn UI a Normal UI** (icono + valor numérico)
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
