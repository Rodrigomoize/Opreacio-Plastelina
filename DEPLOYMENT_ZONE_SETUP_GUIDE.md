# Guía de Configuración: Sistema de Feedback de Zonas de Despliegue

## 📋 Resumen

Este sistema muestra zonas visuales destacadas donde el jugador puede desplegar tropas, diferenciando entre lado izquierdo (azul) y derecho (naranja) según la posición del cursor.

---

## 🎨 Paso 1: Crear las Imágenes de Zona en el Canvas

### 1.1 Crear el GameObject Contenedor

1. En la Hierarchy, localiza tu **Canvas principal** (donde está el UI del juego)
2. Dentro del Canvas, encuentra el objeto **MapRawImage** (o el área de juego clickeable)
3. Crea un nuevo GameObject hijo:
   - Click derecho en Canvas → **UI** → **Empty** (GameObject vacío)
   - Renómbralo: `DeploymentZoneFeedback`

### 1.2 Añadir el Script

1. Selecciona `DeploymentZoneFeedback`
2. **Add Component** → busca `DeploymentZoneFeedback`
3. El script se añadirá al GameObject

### 1.3 Crear la Zona Izquierda

1. Click derecho en `DeploymentZoneFeedback` → **UI** → **Image**
2. Renómbrala: `LeftDeploymentZone`
3. Configurar **RectTransform**:
   - **Anchors**: Min (0, 0) / Max (0.5, 1)
     - Esto hace que cubra la mitad izquierda del Canvas
   - **Left, Right, Top, Bottom**: Todos en `0`
   - **Pivot**: (0.5, 0.5)

4. Configurar **Image Component**:
   - **Source Image**: None (o un sprite blanco/simple)
   - **Color**: Blanco (RGB: 255,255,255, A: 255)
     - ⚠️ El script controlará el color dinámicamente
   - **Material**: None (default)
   - **Raycast Target**: ✅ **DESACTIVADO** (importante)
     - Esto permite que los clicks pasen a través al MapRawImage

### 1.4 Crear la Zona Derecha

1. Click derecho en `DeploymentZoneFeedback` → **UI** → **Image**
2. Renómbrala: `RightDeploymentZone`
3. Configurar **RectTransform**:
   - **Anchors**: Min (0.5, 0) / Max (1, 1)
     - Esto hace que cubra la mitad derecha del Canvas
   - **Left, Right, Top, Bottom**: Todos en `0`
   - **Pivot**: (0.5, 0.5)

4. Configurar **Image Component**:
   - **Source Image**: None (o un sprite blanco/simple)
   - **Color**: Blanco (RGB: 255,255,255, A: 255)
   - **Material**: None
   - **Raycast Target**: ✅ **DESACTIVADO** (importante)

---

## 🔧 Paso 2: Configurar el Componente DeploymentZoneFeedback

Selecciona el GameObject `DeploymentZoneFeedback` y configura:

### Referencias de UI
- **Left Zone Image**: Arrastra `LeftDeploymentZone` aquí
- **Right Zone Image**: Arrastra `RightDeploymentZone` aquí

### Colores de Feedback (valores recomendados)

**Available Color** (Verde suave - estado base):
```
R: 51  (0.2)
G: 255 (1.0)
B: 51  (0.2)
A: 76  (0.3)
```

**Left Highlight Color** (Azul - lado izquierdo):
```
R: 51  (0.2)
G: 128 (0.5)
B: 255 (1.0)
A: 128 (0.5)
```

**Right Highlight Color** (Naranja - lado derecho):
```
R: 255 (1.0)
G: 128 (0.5)
B: 51  (0.2)
A: 128 (0.5)
```

### Configuración de Animación
- **Fade Duration**: `0.2` (duración del fade in/out en segundos)

---

## 🔗 Paso 3: Conectar Referencias entre Scripts

### 3.1 Configurar PlayableAreaUI

1. Localiza el GameObject que tiene el componente `PlayableAreaUI`
   - Normalmente está en el `MapRawImage` o área de juego
2. En el Inspector, busca la sección **Deployment Zone Feedback**:
   - **Zone Feedback**: Arrastra el GameObject `DeploymentZoneFeedback` aquí

### 3.2 Configurar PlayerCardManager

1. Localiza el GameObject que tiene el componente `PlayerCardManager`
   - Normalmente está en un GameObject de gestión del juego
2. En el Inspector, busca la sección **Deployment Zone Feedback**:
   - **Playable Area UI**: Arrastra el GameObject que tiene `PlayableAreaUI` aquí

---

## 📐 Paso 4: Configurar el Orden de Renderizado (Mapa y Torres en Misma RenderTexture)

**⚠️ SITUACIÓN ESPECIAL**: Si tu mapa Y torres se renderizan juntos en una **misma RenderTexture** desde una cámara 3D (no puedes separarlos), las zonas deben ir **DETRÁS** del RawImage, no delante.

### ✅ SOLUCIÓN: Zonas como Fondo Detrás del Mapa

Ya que no puedes poner las zonas entre el mapa y las torres (porque son la misma imagen), las zonas deben renderizarse **detrás** del mapa, visible solo en los bordes o áreas transparentes.

### 🎨 Enfoque Alternativo: Usar Opacidad del Mapa

Si tu RawImage del mapa tiene algo de **transparencia** o **bordes**, las zonas se verán detrás. Si el mapa es completamente opaco, considera una de estas opciones:

#### **Opción A: Zonas como Marco/Borde**

Las zonas se colocan detrás del mapa pero se ven en los bordes:

```
┌─────────────────────────────────┐
│🟢│                         │🟢 │  ← Bordes visibles
│🟢│      Mapa + Torres       │🟢 │
│🟢│      (RawImage)          │🟢 │
│🟢│                         │🟢 │
└─────────────────────────────────┘
```

**Configuración**:
1. El `DeploymentZoneFeedback` queda en el **mismo Canvas** que el mapa
2. En Hierarchy, coloca `DeploymentZoneFeedback` **ANTES** de `MapRawImage`:
   ```
   Canvas
   ├── DeploymentZoneFeedback  ← Primero (atrás)
   │   ├── LeftDeploymentZone
   │   └── RightDeploymentZone
   └── MapRawImage             ← Después (delante)
       └── PlayableAreaUI
   ```
3. Ajusta el **RectTransform** del `MapRawImage` para que tenga márgenes:
   - Left: 50, Right: 50 (deja espacio para ver las zonas en los bordes)

#### **Opción B: Usar Shader/Material con Blend**

Si necesitas que las zonas se vean "a través" del mapa:

1. En el `MapRawImage`, cambia el **Material** a uno con blending
2. O añade un componente **Canvas Group** al RawImage:
   - Alpha: 0.9 (90% opacidad, 10% transparencia)
3. Las zonas detrás se verán levemente a través del mapa

#### **Opción C: Reducir Opacidad de las Zonas (Recomendado para tu caso)**

Si el mapa cubre completamente las zonas, simplemente usa las zonas como indicadores sutiles que se ven **alrededor** del área de juego:

**Configuración**:
1. Mantén las zonas **detrás** del MapRawImage (primero en Hierarchy)
2. Haz el MapRawImage ligeramente más pequeño para que las zonas se vean en los bordes:
   ```
   RectTransform del MapRawImage:
   ├── Anchors: Min (0.05, 0.1) Max (0.95, 0.9)
   └── Esto deja un 5% de margen a los lados y 10% arriba/abajo
   ```
3. Las zonas de fondo se verán como un **marco de color** alrededor del mapa

### Configuración Paso a Paso (Opción C - Recomendada):

#### 4.1 Ajustar la Jerarquía

```
Canvas (Sort Order: 0)
├── DeploymentZoneFeedback  ← ATRÁS (se dibuja primero)
│   ├── LeftDeploymentZone
│   │   └── Anchors: (0, 0) to (0.5, 1)  ← Cubre toda la izquierda
│   └── RightDeploymentZone
│       └── Anchors: (0.5, 0) to (1, 1)  ← Cubre toda la derecha
│
└── MapRawImage             ← DELANTE (se dibuja después, cubre las zonas)
    ├── Anchors: (0.05, 0.1) to (0.95, 0.9)  ← Más pequeño, deja bordes
    └── PlayableAreaUI (script)
```

#### 4.2 Redimensionar el MapRawImage

1. Selecciona `MapRawImage`
2. En RectTransform:
   - **Anchors**: Min (0.05, 0.1) / Max (0.95, 0.9)
   - O usa **Offset**: Left: 30, Right: 30, Top: 50, Bottom: 50
3. Esto deja márgenes donde las zonas de fondo serán visibles

#### 4.3 Ajustar Colores para Mayor Visibilidad

Ya que las zonas estarán parcialmente cubiertas, usa colores más **vibrantes** en los bordes:

```
Available Color (verde más brillante):
R: 100, G: 255, B: 100, A: 200

Left Highlight Color (azul brillante):
R: 50, G: 150, B: 255, A: 220

Right Highlight Color (naranja brillante):
R: 255, G: 150, B: 50, A: 220
```

---

### 🔍 Verificación Visual

**Resultado esperado**:
```
┌─────────────────────────────────┐
│🔵🔵🔵                     🟠🟠🟠│  ← Zonas visibles en bordes
│🔵  ┌─────────────────┐      🟠│
│🔵  │                 │      🟠│
│🔵  │  Mapa + Torres  │      🟠│  ← Mapa cubre el centro
│🔵  │   (RawImage)    │      🟠│
│🔵  └─────────────────┘      🟠│
│🔵🔵🔵                     🟠🟠🟠│
└─────────────────────────────────┘
```

El jugador verá:
- ✅ Bordes de color indicando las zonas (azul izquierda, naranja derecha)
- ✅ Mapa y torres completamente visibles en el centro
- ✅ Feedback claro de qué zona está seleccionando

---

## ✅ Paso 5: Verificación

### Checklist de Configuración

- [ ] `LeftDeploymentZone` y `RightDeploymentZone` creadas
- [ ] Anchors configurados correctamente (izq: 0-0.5, der: 0.5-1)
- [ ] **Raycast Target desactivado** en ambas imágenes
- [ ] Script `DeploymentZoneFeedback` añadido
- [ ] Referencias de zonas asignadas en el script
- [ ] Colores configurados (verde/azul/naranja)
- [ ] `PlayableAreaUI.zoneFeedback` asignado
- [ ] `PlayerCardManager.playableAreaUI` asignado
- [ ] Orden de renderizado correcto (zonas por encima del mapa, por debajo de torres)

### Testing en Play Mode

1. ▶️ **Play**
2. **Selecciona una carta** de tu mano
   - ✅ Las zonas deben aparecer (verde semi-transparente)
3. **Mueve el cursor** sobre el área de juego:
   - Lado izquierdo → Zona izquierda se vuelve **azul**
   - Lado derecho → Zona derecha se vuelve **naranja**
4. **Haz clic** para desplegar la tropa
   - ✅ La tropa aparece
   - ✅ Las zonas desaparecen con fade-out
5. **Selecciona 2 cartas + operador** (ej: 2+3)
   - ✅ Las zonas aparecen nuevamente
6. **Deselecciona** (clic en carta seleccionada)
   - ✅ Las zonas desaparecen

---

## 🎨 Personalización

### Cambiar Colores por Tema

**Tema Oscuro (Neón)**:
```
Available:     RGBA(0, 255, 255, 100)   // Cian neón
Left Highlight: RGBA(255, 0, 255, 150)  // Magenta
Right Highlight: RGBA(255, 255, 0, 150) // Amarillo
```

**Tema Equipos (Rojo vs Azul)**:
```
Available:     RGBA(255, 255, 255, 80)  // Blanco neutral
Left Highlight: RGBA(100, 100, 255, 140) // Azul equipo
Right Highlight: RGBA(255, 100, 100, 140) // Rojo equipo
```

### Ajustar Transparencia

- **Más visible**: Aumenta el canal Alpha (A) a 180-200
- **Más sutil**: Disminuye el canal Alpha (A) a 50-80

### Cambiar Velocidad de Fade

- **Más rápido**: `Fade Duration = 0.1`
- **Más lento**: `Fade Duration = 0.3`

---

## 🐛 Solución de Problemas

### Las zonas no aparecen

**Causa**: Referencias no asignadas
- ✅ Verifica `PlayableAreaUI.zoneFeedback` asignado
- ✅ Verifica `PlayerCardManager.playableAreaUI` asignado
- ✅ Verifica `DeploymentZoneFeedback.leftZoneImage` y `rightZoneImage` asignados

### Las zonas no cambian de color al mover el cursor

**Causa**: Eventos de pointer no funcionan
- ✅ Verifica que `MapRawImage` tiene **Raycast Target activado**
- ✅ Verifica que las zonas tienen **Raycast Target desactivado**
- ✅ Verifica que `PlayableAreaUI` implementa las interfaces correctas

### Los clicks no funcionan

**Causa**: Zonas bloquean los clicks
- ✅ **CRÍTICO**: Desactiva **Raycast Target** en `LeftDeploymentZone` y `RightDeploymentZone`

### Las zonas están detrás del mapa (no se ven)

**Causa**: Orden de renderizado incorrecto
- ✅ Aumenta el **Sort Order** del `DeploymentZonesCanvas`
- ✅ Debe ser mayor que el Canvas del mapa

### ⚠️ Las zonas cubren las torres/elementos del juego (PROBLEMA COMÚN)

**Causa**: Todo está renderizado desde una cámara 3D en el mismo plano
- ✅ **SOLUCIÓN**: Crear un Canvas separado para las zonas con Sort Order intermedio
- ✅ Configurar Sort Orders:
  ```
  Mapa Canvas:              Sort Order = 0
  DeploymentZonesCanvas:    Sort Order = 5  ← Las zonas
  Torres/Elementos Canvas:  Sort Order = 10 ← Por encima
  UI Canvas:                Sort Order = 100
  ```
- ✅ **NO** uses el orden de Hierarchy si todo está renderizado desde cámara 3D
- ✅ Debes usar **Canvas separados** con diferentes Sort Orders

### Las torres siguen cubiertas por las zonas después de crear Canvas separados

**Causa**: Sort Order incorrecto
- ✅ Verifica que el Canvas de las torres tenga un **Sort Order MAYOR** que `DeploymentZonesCanvas`
- ✅ Ejemplo: Zonas = 5, Torres = 10
- ✅ Si aún no funciona, aumenta aún más el Sort Order de las torres (ej: 20, 50)

---

## 📊 Diagrama de Configuración

### Estructura de Canvas (para Rendering 3D)

```
Scene Hierarchy:

Canvas (Mapa)
├── Sort Order: 0
├── Render Mode: Screen Space - Overlay (o Camera)
└── MapRawImage
    ├── RawImage Component
    │   └── Texture: (RenderTexture de cámara 3D)
    └── PlayableAreaUI (script)

DeploymentZonesCanvas ← NUEVO CANVAS SEPARADO
├── Sort Order: 5      ← CLAVE: Mayor que mapa, menor que torres
├── Render Mode: Screen Space - Overlay (igual que Canvas principal)
└── DeploymentZoneFeedback
    ├── leftZoneImage → LeftDeploymentZone (Image)
    │   └─ Raycast Target: OFF
    │   └─ Anchors: (0,0) to (0.5,1)
    └── rightZoneImage → RightDeploymentZone (Image)
        └─ Raycast Target: OFF
        └─ Anchors: (0.5,0) to (1,1)

Canvas (Torres y Elementos 3D)
├── Sort Order: 10     ← Mayor que zonas
└── (Elementos renderizados desde otra cámara)

Canvas (UI Cartas)
├── Sort Order: 100    ← Muy por encima
└── CardHand, botones, etc.
```

### Diagrama de Referencias

```
PlayableAreaUI (script en MapRawImage del Canvas principal)
    ↓
    └─ zoneFeedback → DeploymentZoneFeedback (GameObject en DeploymentZonesCanvas)
                          ↓
                          ├─ leftZoneImage → LeftDeploymentZone (Image)
                          └─ rightZoneImage → RightDeploymentZone (Image)

PlayerCardManager
    ↓
    └─ playableAreaUI → PlayableAreaUI (script)
```

---

## 🚀 ¡Listo!

El sistema de feedback de zonas de despliegue está completo. Los jugadores ahora verán claramente:
- ✅ **Dónde** pueden desplegar tropas (zonas verdes)
- ✅ **Qué lado** están eligiendo (azul izquierda / naranja derecha)
- ✅ **Cuándo** pueden desplegar (zonas aparecen con carta seleccionada)

**Fecha**: 24 de Octubre de 2025  
**Estado**: ✅ Implementado y listo para usar
