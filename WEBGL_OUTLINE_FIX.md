# Guía de Corrección de Outline para WebGL

## 🐛 Problema Identificado
En las builds de WebGL, el outline aparecía pintando todo el modelo de rojo en lugar de solo el contorno. Esto es causado por incompatibilidades del Stencil Buffer en WebGL.

## ✅ Soluciones Aplicadas

### 1. **Shaders Actualizados**

#### **OutlineFill.shader**
- ✅ Añadido `#pragma target 3.0` para mejor compatibilidad WebGL
- ✅ Añadido soporte de fog (`UNITY_APPLY_FOG`)
- ✅ Mejorado el chequeo de smooth normals: `length(input.smoothNormal) > 0.5` en lugar de `any()`
- ✅ Stencil buffer mejorado con operaciones explícitas (Keep, Keep, Keep)
- ✅ Añadido Fallback "Diffuse"

#### **OutlineMask.shader**
- ✅ Añadido vertex y fragment shader explícitos (antes solo tenía un pass vacío)
- ✅ Stencil operations explícitas: `Comp Always, Pass Replace, Fail Keep, ZFail Keep`
- ✅ Añadido `#pragma target 3.0`
- ✅ Añadido Fallback Off

### 2. **Cambios Técnicos**

```glsl
// ANTES (problemático en WebGL)
Stencil {
    Ref 1
    Pass Replace
}

// DESPUÉS (compatible con WebGL)
Stencil {
    Ref 1
    Comp Always
    Pass Replace
    Fail Keep
    ZFail Keep
}
```

## 🔧 Configuraciones Adicionales Recomendadas

### En Unity (Player Settings para WebGL):

1. **Graphics API**
   - Ve a: `Edit > Project Settings > Player > WebGL Settings`
   - En **"Other Settings" > "Graphics APIs for WebGL"**
   - Asegúrate de que esté: **WebGL 2.0** (primero)
   - Si no está, haz clic en el `+` y agrega WebGL 2.0

2. **Color Space**
   - Ve a: `Edit > Project Settings > Player > Other Settings`
   - **Color Space**: Linear (recomendado) o Gamma (más compatible)
   - Para WebGL, **Gamma** suele ser más seguro

3. **Compression Format**
   - Ve a: `Edit > Project Settings > Player > Publishing Settings`
   - **Compression Format**: Gzip o Brotli (no Disabled)

### En Graphics Settings:

1. Ve a: `Edit > Project Settings > Graphics`
2. En **"Built-in Shader Settings"**:
   - Asegúrate de que "Always Included Shaders" incluya tus outline shaders
3. En **"Shader Stripping"**:
   - **Instancing Variants**: Keep All
   - **Fog Modes**: Automatic

## 🧪 Testing

### Para Probar en el Editor:
1. Selecciona un objeto con Outline
2. En el Inspector, verifica:
   - **Outline Mode**: OutlineAll o OutlineVisible
   - **Outline Color**: Rojo (o el color que prefieras)
   - **Outline Width**: 2-5 (visible pero no excesivo)

### Para Probar en WebGL:
1. Ve a `File > Build Settings`
2. Selecciona **WebGL**
3. Haz clic en **"Player Settings"** y verifica las configuraciones anteriores
4. Haz clic en **"Build And Run"**
5. Prueba el outline en el navegador

## 🔍 Troubleshooting

### Si el outline sigue sin funcionar:

#### Problema 1: Modelo completo se pinta del color del outline
**Solución:**
- Verifica que el objeto tenga un material normal asignado en el slot 0
- El outline añade materiales adicionales, no reemplaza el original

#### Problema 2: No se ve ningún outline
**Solución:**
- Aumenta el **Outline Width** a 5-10 temporalmente
- Cambia **Outline Mode** a "OutlineAll"
- Verifica que el objeto tenga un Renderer activo

#### Problema 3: Outline funciona en editor pero no en WebGL
**Solución:**
```csharp
// En Outline.cs, en el método Awake(), añade verificación:
void Awake() {
    // Verificar soporte de Stencil Buffer en WebGL
    if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 ||
        SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2) {
        Debug.LogWarning("[Outline] WebGL detected - using simplified outline mode");
    }
    
    // ... resto del código
}
```

#### Problema 4: Performance pobre en WebGL
**Solución:**
- Reduce el **Outline Width** a 1-2
- Usa **"OutlineVisible"** en lugar de "OutlineAll"
- Considera usar outline solo en objetos seleccionados, no todos

## 📊 Configuración Óptima para WebGL

```
Outline Component:
├─ Outline Mode: OutlineVisible (mejor performance)
├─ Outline Color: Color con Alpha = 1 (opaco)
├─ Outline Width: 2-3 (balance entre visibilidad y performance)
└─ Precompute Outline: ✅ Activado (mejora performance en runtime)
```

## 🎨 Alternativas si el Problema Persiste

Si después de estos cambios el outline sigue dando problemas en WebGL, considera:

### Opción A: Usar Shader Simplificado
Crear un shader de outline más simple que no use Stencil Buffer:
- Solo expandir el modelo con las normales
- Sin máscaras de stencil

### Opción B: Usar Post-Processing Outline
- Unity Post-Processing Stack tiene efectos de outline
- Más pesado pero más compatible

### Opción C: Usar Sprite/Texture Outline
- Para modelos 2D o isométricos
- Crear un sprite duplicado más grande detrás

## 📝 Notas Importantes

1. **Stencil Buffer**: WebGL tiene soporte limitado para Stencil Buffer. Los cambios realizados son más explícitos para garantizar compatibilidad.

2. **Shader Target**: `#pragma target 3.0` asegura características de shader necesarias.

3. **any() vs length()**: La función `any()` puede causar problemas en WebGL. Usar `length() > 0.5` es más confiable.

4. **Fallback Shaders**: Siempre incluir fallbacks para que Unity use algo si el shader falla.

5. **Testing**: Siempre prueba en un build WebGL real, no solo en el editor.

## ✨ Resultado Esperado

Después de aplicar estos cambios:
- ✅ El outline debe verse solo en el borde del modelo
- ✅ El modelo mantiene su color/textura original
- ✅ El outline funciona tanto en Editor como en WebGL
- ✅ Performance aceptable en navegadores modernos

## 🔗 Recursos Adicionales

- [Unity Manual - WebGL Graphics](https://docs.unity3d.com/Manual/webgl-graphics.html)
- [Unity Manual - Stencil Buffer](https://docs.unity3d.com/Manual/SL-Stencil.html)
- [WebGL Best Practices](https://docs.unity3d.com/Manual/webgl-building.html)
