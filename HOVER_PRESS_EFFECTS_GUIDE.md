# Guía de Efectos Visuales: Hover y Press

## Descripción
Sistema de efectos visuales interactivos para cartas y botones que responden al pasar el ratón por encima (hover) y al hacer clic (press).

## Archivos Creados/Modificados

### Scripts Modificados
1. `Assets/Scripts/Combat/Card.cs` (`CardDisplay`) - Añadidos efectos de hover y press para cartas

### Scripts Nuevos
2. `Assets/Scripts/Combat/ButtonPressEffect.cs` - Efectos de hover y press para botones

## Efectos Visuales Implementados

### Para Cartas (CardDisplay)

#### 🖱️ Hover (Ratón Encima)
- **Escala aumentada**: La carta crece ligeramente (10% por defecto)
- **Elevación**: La carta se eleva unos píxeles hacia arriba (20px por defecto)
- **Animación suave**: Transición fluida con interpolación

#### 👆 Press (Al Hacer Clic)
- **Escala reducida**: La carta se reduce ligeramente (95% por defecto)
- **Sin elevación**: Vuelve a la posición base
- **Feedback táctil**: Sensación de "pulsar" la carta

#### 🎯 Estados Visuales
```
Normal:    [Escala 100%]  [Posición base]
Hover:     [Escala 110%]  [Elevada +20px]  ✨
Press:     [Escala 95%]   [Posición base]  👇
```

### Para Botones (ButtonPressEffect)

#### 🖱️ Hover (Ratón Encima)
- **Escala aumentada**: El botón crece ligeramente (10% por defecto)
- **Rotación**: Ligera rotación (5° por defecto) para dinamismo
- **Solo si está activo**: No anima botones deshabilitados

#### 👆 Press (Al Hacer Clic)
- **Escala reducida**: El botón se reduce (90% por defecto)
- **Sin rotación**: Vuelve a rotación normal
- **Feedback táctil**: Sensación de "apretar" el botón

#### 🎯 Estados Visuales
```
Normal:    [Escala 100%]  [Rotación 0°]
Hover:     [Escala 110%]  [Rotación 5°]   ✨
Press:     [Escala 90%]   [Rotación 0°]   👇
```

## Configuración en Unity

### Paso 0: IMPORTANTE - Verificar EventSystem

**CRÍTICO**: Para que los efectos de hover funcionen correctamente, necesitas un **EventSystem** en la escena:

1. **Verifica si existe EventSystem**:
   - En la jerarquía, busca un objeto llamado "EventSystem"
   - Si NO existe, créalo: `GameObject > UI > Event System`

2. **Configuración del EventSystem**:
   - Debe tener el componente `EventSystem`
   - Debe tener el componente `Standalone Input Module` (o `Input System UI Input Module` si usas el nuevo Input System)

3. **Síntomas si falta EventSystem**:
   - ❌ Los eventos hover solo funcionan cuando mantienes el botón del ratón presionado
   - ❌ Los eventos OnPointerEnter/OnPointerExit no se disparan correctamente
   - ✅ Con EventSystem: Los eventos hover funcionan simplemente pasando el ratón por encima

### Paso 1: Configurar Efectos de Cartas

Las cartas ya tienen el script `CardDisplay` modificado, pero puedes ajustar los parámetros:

1. **Selecciona una carta** en la escena (o el prefab de carta)
2. En el componente **CardDisplay**, verás una nueva sección **Visual Effects**:
   - **Hover Scale**: 1.1 (escala al pasar el ratón)
   - **Press Scale**: 0.95 (escala al hacer clic)
   - **Scale Animation Speed**: 10 (velocidad de animación)
   - **Hover Elevation**: 20 (píxeles que se eleva al hacer hover)

3. **Ajusta a tu gusto**:
   - Hover más sutil: 1.05
   - Hover más dramático: 1.15
   - Press más sutil: 0.98
   - Elevación más alta: 30-40px

### Paso 2: Configurar Efectos de Botones

1. **Selecciona el botón +** en la jerarquía
2. **Add Component** > `ButtonPressEffect`
3. Configura los parámetros:
   - **Hover Scale**: 1.1
   - **Press Scale**: 0.9
   - **Scale Animation Speed**: 12
   - **Hover Rotation**: 5

4. **Repite para el botón -**

5. **Ajusta a tu gusto**:
   - Rotación más dramática: 10-15°
   - Sin rotación: 0°
   - Hover más sutil: 1.05
   - Press más dramático: 0.85

### Paso 3: Asegurar Raycast Target

Para que los eventos de hover funcionen correctamente:

1. **Cartas**: Verifica que el componente `Image` tenga **Raycast Target** marcado
2. **Botones**: Los botones ya tienen esto configurado por defecto

## Personalización Avanzada

### Efectos Diferentes por Tipo de Carta

Puedes tener valores diferentes para cada carta si quieres:

```csharp
// En el Inspector, cada carta puede tener:
Carta 1: Hover Scale = 1.05, Hover Elevation = 15
Carta 5: Hover Scale = 1.15, Hover Elevation = 30
```

### Deshabilitar Efectos en Cartas Bloqueadas

Si quieres que las cartas bloqueadas no tengan efecto hover, puedes modificar `Card.cs`:

```csharp
public void OnPointerEnter(PointerEventData eventData)
{
    // Solo hacer hover si la carta es válida
    if (ownerManager != null && !ownerManager.IsCardValid(this))
        return;
        
    isHovering = true;
    UpdateVisualState();
}
```

### Efectos de Sonido (Opcional)

Puedes añadir sonidos en los eventos:

```csharp
public void OnPointerEnter(PointerEventData eventData)
{
    // AudioManager.Instance.PlaySound("CardHover");
    isHovering = true;
    UpdateVisualState();
}

public void OnPointerDown(PointerEventData eventData)
{
    // AudioManager.Instance.PlaySound("CardPress");
    isPressing = true;
    UpdateVisualState();
}
```

## Ejemplos de Configuración

### Estilo Sutil y Elegante
```
CardDisplay:
  Hover Scale: 1.05
  Press Scale: 0.98
  Hover Elevation: 10
  Animation Speed: 15

ButtonPressEffect:
  Hover Scale: 1.05
  Press Scale: 0.95
  Hover Rotation: 2
  Animation Speed: 15
```

### Estilo Dinámico y Expresivo
```
CardDisplay:
  Hover Scale: 1.15
  Press Scale: 0.9
  Hover Elevation: 30
  Animation Speed: 8

ButtonPressEffect:
  Hover Scale: 1.15
  Press Scale: 0.85
  Hover Rotation: 10
  Animation Speed: 10
```

### Estilo Minimalista
```
CardDisplay:
  Hover Scale: 1.03
  Press Scale: 0.97
  Hover Elevation: 5
  Animation Speed: 20

ButtonPressEffect:
  Hover Scale: 1.03
  Press Scale: 0.97
  Hover Rotation: 0
  Animation Speed: 20
```

## Solución de Problemas

### ⚠️ PROBLEMA COMÚN: Hover solo funciona con ratón presionado
**Síntoma**: Los efectos de hover solo se activan cuando mantienes el botón del ratón presionado y arrastras por encima de las cartas.

**Solución**:
1. ✅ **Crea un EventSystem** si no existe:
   - `GameObject > UI > Event System`
2. ✅ Verifica que el EventSystem esté activo en la jerarquía
3. ✅ Asegúrate de que el EventSystem tenga el componente `Standalone Input Module`

**Por qué ocurre**: Sin EventSystem, Unity no procesa correctamente los eventos de UI como `OnPointerEnter` y `OnPointerExit`. Solo procesa eventos de clic/drag.

### ⚠️ PROBLEMA: Todas las cartas aparecen desplazadas hacia arriba
**Síntoma**: Al iniciar el juego, todas las cartas están elevadas como si estuvieran seleccionadas.

**Solución**: Este problema ha sido **corregido** en la última versión del código. El sistema ahora usa `offsetMin/offsetMax` en lugar de `anchoredPosition` para evitar conflictos.

Si el problema persiste:
1. ✅ Asegúrate de que las cartas en el prefab tienen `anchoredPosition = (0, 0)`
2. ✅ Verifica que no haya scripts adicionales modificando la posición de las cartas
3. ✅ Resetea la posición en el Inspector si es necesario

### Los efectos no funcionan en las cartas
- ✓ Verifica que `Image` tenga **Raycast Target** marcado
- ✓ Asegúrate de que no hay otro objeto UI bloqueando los raycast
- ✓ Verifica que el Canvas tenga un `GraphicRaycaster` component

### Los botones no responden al hover
- ✓ Verifica que el botón tenga el componente `ButtonPressEffect`
- ✓ Asegúrate de que el botón esté `interactable` (no deshabilitado)
- ✓ Verifica que el botón tenga el componente `Button`

### Las animaciones son muy lentas o rápidas
- ✓ Ajusta `Scale Animation Speed`:
  - Más lento: valores 5-8
  - Normal: valores 10-12
  - Más rápido: valores 15-20

### Las cartas se quedan "flotando"
- ✓ Verifica que `Hover Elevation` no sea demasiado alto
- ✓ Asegúrate de que `OnPointerExit` se está llamando (puede ser bloqueado por otros objetos UI)

### La elevación no se ve
- ✓ Asegúrate de que las cartas están en un Canvas UI (no en world space 3D sin configuración apropiada)
- ✓ Verifica que el `RectTransform` de la carta permite movimiento en Y

## Ventajas del Sistema

✅ **Feedback inmediato**: Los jugadores ven respuesta instantánea a sus acciones
✅ **Sensación táctil**: Los efectos de press/hover crean sensación de interfaz física
✅ **Configurable**: Todos los valores son ajustables desde el Inspector
✅ **Performante**: Usa interpolación eficiente, no crea objetos nuevos
✅ **Intuitivo**: Los jugadores saben intuitivamente qué elementos son interactuables

## Próximas Mejoras (Opcional)

1. **Partículas**: Añadir efecto de partículas en hover
2. **Sonidos**: Integrar con sistema de audio
3. **Vibración**: Para dispositivos móviles
4. **Trails**: Efecto de estela al mover cartas
5. **Glow**: Brillo sutil alrededor de cartas en hover

## Notas Técnicas

- Los efectos usan `Update()` para animación suave con `Lerp`
- No interfieren con la lógica de juego existente
- Compatible con el sistema de colores implementado anteriormente
- Los estados se resetean automáticamente al salir con el ratón
- `IPointerEnterHandler`, `IPointerExitHandler`, `IPointerDownHandler`, `IPointerUpHandler` de Unity Event System
