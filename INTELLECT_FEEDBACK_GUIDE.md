# Guía de Configuración: Feedback +1 Intelecto

## 🎯 Sistema de Feedback Visual para Operaciones Resueltas

Cuando el jugador resuelve correctamente una operación enemiga (Character derrota a Combined con el mismo valor), aparece un feedback visual animado que muestra "+1 Intelecto".

---

## 📋 Pasos para Configurar en Unity

### 1. Crear el Prefab de Feedback

1. **Crear un Canvas World Space:**
   - Hierarchy → Clic derecho → UI → Canvas
   - Nombre: `IntellectFeedback`
   - En el Inspector:
     - Render Mode: `World Space`
     - Width: `200`
     - Height: `100`
     - Scale: `0.01, 0.01, 0.01` (para que sea proporcional en el mundo 3D)

2. **Añadir el Sprite de +1 Intelecto:**
   - Dentro del Canvas, crear: UI → Image
   - Nombre: `IntellectIcon`
   - Asignar tu sprite de +1 Intelecto al componente Image
   - Ajustar tamaño según necesites (ej: 150x150)

3. **Añadir el Script:**
   - Selecciona el Canvas `IntellectFeedback`
   - Add Component → `IntellectFeedback`
   - Configurar parámetros:
     - **Rise Speed**: `2` (velocidad de elevación)
     - **Duration**: `1` (duración total del efecto)
     - **Initial Scale**: `0.5` (escala inicial para efecto pop)
     - **Target Scale**: `1` (escala final)
     - **Pop Duration**: `0.2` (duración del efecto pop-in)

4. **Añadir CanvasGroup (opcional, se añade automáticamente):**
   - El script añade automáticamente un CanvasGroup si no existe
   - Sirve para controlar el fade out

5. **Guardar como Prefab:**
   - Arrastra el objeto `IntellectFeedback` desde la Hierarchy a la carpeta `Assets/Prefabs/UI/`
   - Elimina el objeto de la Hierarchy

---

### 2. Configurar el Character Prefab

1. **Seleccionar todos los prefabs de Character (tropas):**
   - Navega a `Assets/Prefabs/` donde están tus prefabs de tropas

2. **En el Inspector, buscar la sección "Feedback de Intelecto":**
   - **Intellect Feedback Prefab**: Arrastra el prefab `IntellectFeedback` que creaste
   - **Feedback Y Offset**: `2` (altura donde aparece el feedback, ajusta según tu escala)

3. **Aplicar los cambios** a todos los prefabs de Character

---

## 🎨 Personalización

### Ajustar la Animación

En el script `IntellectFeedback.cs`, puedes modificar:

```csharp
riseSpeed = 2f;        // Más alto = sube más rápido
duration = 1f;         // Duración total del efecto
initialScale = 0.5f;   // Escala inicial (pop-in)
targetScale = 1f;      // Escala final
popDuration = 0.2f;    // Duración del efecto pop
```

### Cambiar el Sprite

- Solo necesitas cambiar el sprite en el componente Image del prefab
- Puedes usar:
  - Un icono de +1
  - Texto "CORRECTO!"
  - Efecto de estrella
  - Combinación de imagen + texto

---

## 🔍 Cómo Funciona

### Detección de Operación Resuelta

El sistema detecta cuando:
1. Un **Character del jugador** (PlayerTeam) derrota a un **Combined enemigo** (AITeam)
2. O un **Character de IA** (AITeam) derrota a un **Combined del jugador** (PlayerTeam)

### Flujo del Feedback

1. **Combate termina** (después de 1.1s de animación de ataque)
2. **Sistema detecta** que fue operación resuelta correctamente
3. **Instancia el prefab** en la posición del Character defensor + offset vertical
4. **Animación ejecuta:**
   - Pop-in: Escala de 0.5 a 1.0 (0.2s)
   - Rise: Se eleva hacia arriba (riseSpeed)
   - Fade: Se desvanece gradualmente
5. **Auto-destrucción** después de 1 segundo

---

## 🎮 Casos de Uso

### ✅ Muestra Feedback:
- Jugador tira `3` → Derrota operación enemiga `3` (1+2) → ✨ +1 Intelecto
- IA tira `5` → Derrota operación del jugador `5` (2+3) → ✨ +1 Intelecto

### ❌ NO Muestra Feedback:
- Character vs Character (tropa vs tropa)
- Combined vs Combined (operación vs operación, aunque esto no debería pasar)

---

## 💡 Ideas Adicionales de Feedback

Si quieres más efectos visuales, puedes añadir:

1. **Partículas brillantes** alrededor del Character defensor
2. **Flash de color** en la pantalla (verde para éxito)
3. **Sonido de éxito** (monedas, ding, etc.)
4. **Escala del Character** (pequeño bounce al resolver correctamente)
5. **Trail effect** en el sprite mientras sube

Estos se pueden añadir fácilmente modificando el método donde se llama a `IntellectFeedback.Create()`.

---

## 🐛 Troubleshooting

**El feedback no aparece:**
- Verifica que el prefab esté asignado en el Character
- Revisa que `feedbackYOffset` no sea muy alto o bajo
- Comprueba en la consola si aparece el log "✅ ¡Operación resuelta correctamente!"

**El feedback aparece muy pequeño/grande:**
- Ajusta el `Scale` del Canvas en el prefab (ej: 0.01)
- Modifica `initialScale` y `targetScale` en el script

**El feedback no mira a la cámara:**
- El script automáticamente rota el canvas hacia la cámara en `LateUpdate()`
- Verifica que existe una Main Camera en la escena

---

## ✨ Resultado Final

Cuando el jugador resuelve correctamente una operación:
1. ⚔️ Character ataca al Combined
2. 🌀 Implosión entre ambos
3. ✨ Aparece "+1 Intelecto" flotando y elevándose
4. 💥 VFX de impacto
5. 🎯 +1 de intelecto se añade al jugador

¡Todo en una secuencia fluida y satisfactoria! 🎮
