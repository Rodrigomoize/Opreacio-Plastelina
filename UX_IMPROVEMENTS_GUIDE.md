# GUÍA DE MEJORAS UX IMPLEMENTADAS

## 📋 Resumen de Cambios

Se han implementado 3 mejoras de UX para mejorar la retroalimentación visual del juego:

### 1. ✅ Shake en Barra de Intelecto (en vez de pantalla completa)
### 2. ✅ Parpadeo en Zonas de Deployment  
### 3. ✅ Efecto Hover en Botones

---

## 🔧 1. SHAKE EN BARRA DE INTELECTO

**Problema anterior:** Cuando el jugador no tenía suficiente intelecto, toda la cámara temblaba, lo cual era confuso.

**Solución:** Ahora solo la barra de intelecto tiembla, llamando la atención directamente al problema.

### Archivos modificados:
- `Assets/Scripts/IntelectBar.cs`
- `Assets/Scripts/Combat/PlayerCardManager.cs`

### Código añadido en IntelectBar.cs:

```csharp
/// <summary>
/// Shake visual de la barra para indicar intelecto insuficiente
/// </summary>
public void ShakeBar(float duration = 0.5f, float magnitude = 10f)
{
    if (gameObject.activeInHierarchy)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
}

private System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude)
{
    RectTransform rectTransform = GetComponent<RectTransform>();
    if (rectTransform == null) yield break;

    Vector3 originalPosition = rectTransform.anchoredPosition;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        float x = Random.Range(-1f, 1f) * magnitude;
        float y = Random.Range(-1f, 1f) * magnitude;

        rectTransform.anchoredPosition = originalPosition + new Vector3(x, y, 0);

        elapsed += Time.unscaledDeltaTime; // Funciona en pausa
        yield return null;
    }

    rectTransform.anchoredPosition = originalPosition;
}
```

### Uso:
Se llama automáticamente desde `PlayerCardManager.cs` cuando el jugador intenta una acción sin suficiente intelecto.

**Parámetros configurables:**
- `duration`: Duración del shake (default: 0.5s)
- `magnitude`: Intensidad del movimiento (default: 10 píxeles, pero se usa 15 en el juego)

---

## 🔧 2. PARPADEO EN ZONAS DE DEPLOYMENT

**Problema anterior:** No estaba claro dónde se podían soltar las tropas.

**Solución:** Las zonas de deployment ahora parpadean suavemente para llamar la atención.

### Archivo modificado:
- `Assets/Scripts/DeploymentZoneFeedback.cs`

### Nuevos parámetros en Inspector:

```
[Header("Configuración de Animación")]
✅ Enable Pulse: true/false (activar/desactivar parpadeo)
✅ Pulse Speed: 1.5 (velocidad del parpadeo en ciclos/segundo)
✅ Pulse Intensity: 0.3 (intensidad del parpadeo, 0-1)
```

### Configuración recomendada:
- **enablePulse:** `true`
- **pulseSpeed:** `1.5` (parpadea 1.5 veces por segundo - sutil pero visible)
- **pulseIntensity:** `0.3` (30% de variación de alpha - no agresivo)

### Cómo funciona:
- Parpadeo continuo mientras las zonas están visibles
- Se detiene automáticamente al ocultar las zonas
- Usa `Mathf.Sin()` para un parpadeo suave y natural
- No interfiere con el feedback de hover (cambio de color al pasar el ratón)

### Código añadido:

```csharp
private System.Collections.IEnumerator PulseZones()
{
    while (isVisible)
    {
        float time = Time.time * pulseSpeed;
        float pulse = Mathf.Sin(time * Mathf.PI * 2f) * 0.5f + 0.5f; // 0 a 1
        float alphaMultiplier = 1f - (pulse * pulseIntensity);
        
        if (leftZoneImage != null)
        {
            Color c = availableColor;
            c.a *= alphaMultiplier;
            leftZoneImage.color = c;
        }
        
        if (rightZoneImage != null)
        {
            Color c = availableColor;
            c.a *= alphaMultiplier;
            rightZoneImage.color = c;
        }
        
        yield return null;
    }
}
```

---

## 🔧 3. EFECTO HOVER EN BOTONES

**Problema anterior:** Los botones no tenían feedback visual al pasar el ratón.

**Solución:** Nuevo componente `ButtonHoverEffect.cs` que se puede añadir a cualquier botón.

### Nuevo archivo creado:
- `Assets/Scripts/UI/ButtonHoverEffect.cs`

### Características:
✅ **Escala al hover** (botón crece ligeramente)
✅ **Rotación opcional** (giro sutil)
✅ **Cambio de color opcional**
✅ **Sonido de hover opcional**
✅ **Completamente configurable desde Inspector**
✅ **Animación suave** con lerp

### Parámetros configurables:

```csharp
[Header("Configuración de Hover")]
✅ hoverScale = 1.1f          // 110% del tamaño original
✅ scaleSpeed = 10f           // Velocidad de animación
✅ enableRotation = false     // Activar rotación
✅ rotationAngle = 5f         // Grados de rotación
✅ enableColorChange = false  // Activar cambio de color
✅ hoverColor = Color.white   // Color al hacer hover

[Header("Audio (Opcional)")]
✅ hoverSound = null          // AudioClip para hover
✅ hoverVolume = 0.5f         // Volumen del sonido
```

### Cómo usar:

#### Opción 1: Manualmente en Unity Editor
1. Selecciona un botón en la escena
2. Click en "Add Component"
3. Busca "ButtonHoverEffect"
4. Ajusta los parámetros en el Inspector

#### Opción 2: Añadir por código
```csharp
Button myButton = GetComponent<Button>();
ButtonHoverEffect hover = myButton.gameObject.AddComponent<ButtonHoverEffect>();
hover.hoverScale = 1.15f;
hover.scaleSpeed = 12f;
```

#### Opción 3: Crear prefab de botón con hover
1. Crea un botón base con `ButtonHoverEffect` ya añadido
2. Configura los parámetros deseados
3. Guárdalo como prefab
4. Usa ese prefab para todos los botones nuevos

### Configuraciones recomendadas por tipo de botón:

**Botones principales (Play, Continue, etc.):**
```
hoverScale: 1.15
scaleSpeed: 12
enableRotation: false
enableColorChange: false
```

**Botones secundarios (Back, Settings):**
```
hoverScale: 1.08
scaleSpeed: 10
enableRotation: false
enableColorChange: false
```

**Botones pequeños (PowerUps, Cards):**
```
hoverScale: 1.12
scaleSpeed: 15
enableRotation: true
rotationAngle: 3
enableColorChange: true
hoverColor: Color amarillo claro
```

---

## 📝 CHECKLIST DE IMPLEMENTACIÓN EN UNITY

### ✅ Shake en Barra de Intelecto
- [x] Código implementado automáticamente
- [ ] Probar en escena de combate
- [ ] Verificar que el shake sea visible pero no molesto
- [ ] Ajustar magnitude si es necesario (actualmente 15f)

### ✅ Parpadeo en Zonas de Deployment
- [x] Código implementado automáticamente
- [ ] Abrir escena de combate (CombatScene o similar)
- [ ] Localizar objeto con `DeploymentZoneFeedback.cs`
- [ ] En Inspector, verificar:
  - [x] Enable Pulse = true
  - [x] Pulse Speed = 1.5
  - [x] Pulse Intensity = 0.3
- [ ] Probar en Play Mode
- [ ] Ajustar valores si el parpadeo es muy sutil o muy agresivo

### ✅ Hover en Botones
- [x] Script `ButtonHoverEffect.cs` creado
- [ ] **IMPORTANTE:** Añadir manualmente a todos los botones del juego:

#### Escenas a revisar:
1. **MainMenuScene:**
   - [ ] Botón "Jugar" / "Play"
   - [ ] Botón "Configuración" / "Settings"
   - [ ] Botón "Salir" / "Exit"
   - [ ] Botón "Créditos" (si existe)

2. **DifficultyPanel / SelectDifficultyScene:**
   - [ ] Botón "Fácil"
   - [ ] Botón "Normal"
   - [ ] Botón "Difícil"
   - [ ] Botón "Volver" / "Back"
   - [ ] Botón "Tutorial"

3. **CombatScene / GameScene:**
   - [ ] Botón de Pausa
   - [ ] Botones de PowerUps (Heal, SlowTime, etc.)
   - [ ] Botones de operadores (+, -)
   - [ ] Botón "Mostrar Instrucciones"
   - [ ] Botón "Cerrar Instrucciones"
   - [ ] Botón "Siguiente" (instrucciones)
   - [ ] Botón "Anterior" (instrucciones)

4. **PausePanel:**
   - [ ] Botón "Reanudar" / "Resume"
   - [ ] Botón "Reiniciar" / "Restart"
   - [ ] Botón "Menú Principal" / "Main Menu"

5. **WinScene:**
   - [ ] Botón "Continuar" / "Continue"
   - [ ] Botón "Menú Principal"

6. **LoseScene:**
   - [ ] Botón "Reintentar" / "Retry"
   - [ ] Botón "Menú Principal"

7. **TutorialScene:**
   - [ ] Botón "Continuar" del tutorial
   - [ ] Cualquier otro botón de UI del tutorial

---

## 🎮 TESTING

### Prueba 1: Shake de Barra de Intelecto
1. Iniciar partida
2. Intentar usar una carta sin suficiente intelecto
3. **Esperado:** La barra de intelecto tiembla + flash rojo en pantalla
4. **Verificar:** El shake es visible pero no molesto

### Prueba 2: Parpadeo de Zonas
1. Iniciar partida
2. Seleccionar una carta o empezar a hacer una operación
3. **Esperado:** Las zonas de deployment parpadean suavemente
4. **Verificar:** El parpadeo es visible pero no distrae
5. Al mover el ratón sobre una zona, debería cambiar de color (hover feedback original)

### Prueba 3: Hover de Botones
1. Navegar por todos los menús
2. Pasar el ratón sobre cada botón
3. **Esperado:** El botón crece ligeramente (1.1x)
4. **Verificar:** La animación es suave y responsiva

---

## 🔧 AJUSTES OPCIONALES

### Si el parpadeo es muy sutil:
```csharp
// En Inspector de DeploymentZoneFeedback
pulseSpeed = 2.0f         // Más rápido
pulseIntensity = 0.5f     // Más contraste
```

### Si el parpadeo es muy agresivo:
```csharp
pulseSpeed = 1.0f         // Más lento
pulseIntensity = 0.2f     // Menos contraste
```

### Si el shake es muy sutil:
```csharp
// En PlayerCardManager.cs línea ~993
intelectBar.ShakeBar(0.6f, 20f);  // Más duración y magnitud
```

### Si el shake es muy agresivo:
```csharp
intelectBar.ShakeBar(0.3f, 10f);  // Menos duración y magnitud
```

### Para ajustar hover de botones específicos:
1. Seleccionar botón en Unity
2. Modificar componente `ButtonHoverEffect`
3. Aumentar `hoverScale` para más efecto
4. Aumentar `scaleSpeed` para animación más rápida

---

## 📊 IMPACTO EN RENDIMIENTO

- **Shake de barra:** Mínimo (solo una coroutine temporal)
- **Parpadeo de zonas:** Muy bajo (solo 2 updates de color por frame cuando visible)
- **Hover de botones:** Muy bajo (lerp en Update, pero solo en botones activos)

**Total:** Impacto negligible en rendimiento, incluso en WebGL.

---

## ✅ COMPLETADO

Todos los cambios están implementados y listos para usar. Solo falta:
1. Añadir `ButtonHoverEffect` manualmente a los botones en Unity Editor
2. Probar y ajustar parámetros según preferencia
3. Guardar escenas modificadas

**Tiempo estimado para añadir hover a todos los botones:** 10-15 minutos
