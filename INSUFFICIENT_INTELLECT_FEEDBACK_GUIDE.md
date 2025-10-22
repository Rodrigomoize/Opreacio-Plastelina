# Guía de Configuración: Sistema de Feedback Visual Negativo

## Descripción
Este sistema proporciona feedback visual cuando el jugador intenta jugar cartas sin suficiente intelecto. Incluye:
- **Camera Shake**: Sacudida de la cámara
- **Screen Flash**: Flash rojizo de pantalla
- **Deselección automática**: Todas las cartas y operadores se deseleccionan inmediatamente

## Archivos Creados/Modificados

### Nuevos Scripts
1. `Assets/Scripts/vfx/CameraShake.cs` - Sistema de sacudida de cámara
2. `Assets/Scripts/vfx/ScreenFlashEffect.cs` - Sistema de flash de pantalla
3. `Assets/Scripts/Combat/CardManager.cs` - Modificado para retornar códigos de error
4. `Assets/Scripts/Combat/PlayerCardManager.cs` - Modificado para usar el feedback visual

## Configuración en Unity

### Paso 1: Configurar CameraShake

1. Selecciona la **Main Camera** en la escena
2. Añade el componente **CameraShake** (Add Component > CameraShake)
3. Configura los parámetros:
   - **Shake Duration**: 0.3 segundos (duración del shake)
   - **Shake Magnitude**: 0.2 (intensidad del shake)
   - **Damping Speed**: 1.0 (velocidad de atenuación)

> **Nota**: Puedes ajustar estos valores para hacer el shake más o menos intenso.

### Paso 2: Configurar ScreenFlashEffect

1. En la jerarquía, busca o crea un Canvas de UI para el efecto
   - Si ya tienes un Canvas de UI, úsalo
   - Si no, crea uno: `GameObject > UI > Canvas`

2. Crea un GameObject vacío como hijo del Canvas:
   - Click derecho en el Canvas > `Create Empty`
   - Nómbralo "ScreenFlashEffect"

3. Añade el componente **ScreenFlashEffect** al GameObject:
   - Add Component > ScreenFlashEffect

4. Añade un componente **Image** al mismo GameObject:
   - Add Component > Image

5. Configura la Image:
   - En el Inspector, selecciona la Image
   - **Source Image**: Déjalo en None (usará color sólido)
   - Expande **Rect Transform** y configura para que cubra toda la pantalla:
     - Anchor Presets: Click en el cuadrado inferior derecho y selecciona "stretch" (el ícono en la esquina inferior derecha con Alt)
     - **Left, Right, Top, Bottom**: Todos en 0
   - **Color**: Blanco (1, 1, 1, 0) - con Alpha en 0
   - **Raycast Target**: Desmarcar (para que no bloquee clics)

6. Configura el componente ScreenFlashEffect:
   - **Flash Color**: Rojo semitransparente (R:1, G:0, B:0, A:0.5)
   - **Flash Duration**: 0.3 segundos
   - **Flash Image**: Arrastra la Image component del mismo GameObject aquí

7. Asegúrate de que el GameObject esté al frente en el Canvas:
   - En el Canvas, mueve el GameObject "ScreenFlashEffect" al final de la lista de hijos (para que se renderice encima)

### Paso 3: Verificar la Configuración

1. **Verifica que el CameraShake esté en la Main Camera**:
   - Selecciona Main Camera
   - Debe tener el componente CameraShake

2. **Verifica que el ScreenFlashEffect esté configurado correctamente**:
   - El GameObject debe tener ambos componentes: Image y ScreenFlashEffect
   - La Image debe cubrir toda la pantalla
   - El componente ScreenFlashEffect debe tener la referencia a la Image

3. **Orden de Canvas (opcional pero recomendado)**:
   - Si tienes múltiples Canvas, asegúrate de que el Canvas con el ScreenFlashEffect tenga un Sort Order alto
   - Esto garantiza que el flash aparezca sobre todos los demás elementos UI

## Funcionamiento

### Cuándo se activa
El feedback visual negativo se activa cuando:
- Intentas jugar una carta sola (ej: 3) sin suficiente intelecto
- Intentas jugar una operación (ej: 3 + 2) sin suficiente intelecto

### Qué sucede
1. **Camera Shake**: La cámara se sacude brevemente (0.3 segundos por defecto)
2. **Screen Flash**: Aparece un flash rojizo que cubre toda la pantalla y se desvanece
3. **Deselección**: TODAS las cartas seleccionadas y el operador se deseleccionan automáticamente

Esto proporciona un feedback claro e inmediato de que no hay suficiente intelecto para realizar la acción.

## Personalización

### Ajustar la intensidad del Camera Shake
En el componente CameraShake de la Main Camera:
- **Más intenso**: Aumenta Shake Magnitude (ej: 0.3 - 0.5)
- **Menos intenso**: Reduce Shake Magnitude (ej: 0.1 - 0.15)
- **Más largo**: Aumenta Shake Duration (ej: 0.4 - 0.5)
- **Más corto**: Reduce Shake Duration (ej: 0.2 - 0.25)

### Ajustar el color del flash
En el componente ScreenFlashEffect:
- **Más rojo**: Flash Color = (1, 0, 0, 0.5)
- **Más oscuro**: Flash Color = (0.5, 0, 0, 0.7)
- **Más amarillo (warning)**: Flash Color = (1, 0.5, 0, 0.5)
- **Más duradero**: Aumenta Flash Duration

### Probar sin jugar
Puedes probar los efectos manualmente desde código o el Inspector:
```csharp
// Para probar el shake
CameraShake.Instance.Shake();

// Para probar el flash
ScreenFlashEffect.Instance.Flash();
```

## Solución de Problemas

### El shake no funciona
- ✓ Verifica que CameraShake esté en la Main Camera
- ✓ Verifica que la Shake Magnitude sea > 0
- ✓ Asegúrate de que la cámara no esté bloqueada por otro script

### El flash no se ve
- ✓ Verifica que la Image esté configurada para cubrir toda la pantalla (Rect Transform en stretch)
- ✓ Verifica que Flash Image esté asignado en el componente ScreenFlashEffect
- ✓ Verifica que el GameObject esté activo en el Canvas
- ✓ Asegúrate de que el Canvas esté en modo Screen Space

### El feedback no se activa
- ✓ Verifica que CameraShake.Instance y ScreenFlashEffect.Instance no sean null
- ✓ Revisa la consola para ver advertencias
- ✓ Asegúrate de que los scripts estén habilitados (checkbox marcado en Inspector)

## Notas Técnicas

- Ambos sistemas usan el patrón **Singleton** para fácil acceso desde cualquier script
- Los efectos son **no bloqueantes** - el juego continúa mientras se muestran
- Los efectos se pueden **apilar** si se llaman múltiples veces (aunque no es recomendado)
- El sistema es **compatible** con el resto del código existente - no rompe funcionalidad anterior

## Próximos Pasos (Opcional)

Si quieres mejorar aún más el feedback:
1. Añade un **sonido de error** cuando no hay intelecto suficiente
2. Añade un **texto flotante** que diga "Intelecto insuficiente"
3. Añade una **animación** a los botones de operación cuando no se pueden usar
4. Implementa un **cooldown visual** en la barra de intelecto
