# Solución: Zonas de Despliegue con Mapa y Torres en Misma RenderTexture

## 🎯 Problema Específico

Tu mapa Y torres se renderizan **juntos** en una **misma RenderTexture** desde una cámara 3D. Esto significa:
- No puedes separar el mapa de las torres
- Todo es una sola imagen (RawImage) en el Canvas
- No hay forma de poner las zonas "entre" el mapa y las torres

```
❌ IMPOSIBLE separar:
┌─────────────────────────────────┐
│    RenderTexture (una imagen)   │
│  ┌──────────────────────────┐   │
│  │  Mapa + Torres juntos    │   │ ← Todo renderizado junto
│  │        🏰                 │   │
│  └──────────────────────────┘   │
└─────────────────────────────────┘
```

---

## ✅ Solución Real: Zonas como Marco de Fondo

Como no puedes separar el mapa de las torres, las zonas deben ir **DETRÁS** del RawImage, visibles en los bordes como un marco de color.

### 🎨 Enfoque Visual

```
Resultado final:
┌─────────────────────────────────┐
│🔵🔵🔵                     🟠🟠🟠│ ← Zonas visibles en bordes
│🔵  ┌─────────────────┐      🟠│
│🔵  │                 │      🟠│
│🔵  │  Mapa + Torres  │      🟠│ ← RawImage cubre centro
│🔵  │      🏰          │      🟠│
│🔵  └─────────────────┘      🟠│
│🔵🔵🔵                     🟠🟠🟠│
└─────────────────────────────────┘
```

---

## �️ Configuración en Unity Editor

### PASO 1: Organizar la Jerarquía

Las zonas deben estar **ANTES** del MapRawImage en la Hierarchy para renderizarse detrás:

```
Canvas
├── DeploymentZoneFeedback  ← PRIMERO (se dibuja primero = atrás)
│   ├── LeftDeploymentZone
│   │   └── RectTransform
│   │       ├── Anchors: Min (0, 0) Max (0.5, 1)
│   │       └── Offsets: Left 0, Right 0, Top 0, Bottom 0
│   │
│   └── RightDeploymentZone
│       └── RectTransform
│           ├── Anchors: Min (0.5, 0) Max (1, 1)
│           └── Offsets: Left 0, Right 0, Top 0, Bottom 0
│
└── MapRawImage             ← DESPUÉS (se dibuja después = delante)
    ├── RectTransform
    │   ├── Anchors: Min (0.05, 0.1) Max (0.95, 0.9)  ← MÁS PEQUEÑO
    │   └── Deja márgenes para que se vean las zonas
    │
    ├── RawImage Component
    │   └── Texture: (tu RenderTexture con mapa+torres)
    │
    └── PlayableAreaUI (script)
        └── zoneFeedback: DeploymentZoneFeedback
```

### PASO 2: Redimensionar el MapRawImage

El truco está en hacer el RawImage **ligeramente más pequeño** que el Canvas completo:

1. Selecciona `MapRawImage`
2. En Inspector → **RectTransform**:

**Opción A - Usando Anchors**:
```
Anchors:
├── Min: (0.05, 0.1)   ← 5% desde izquierda, 10% desde abajo
└── Max: (0.95, 0.9)   ← 5% desde derecha, 10% desde arriba

Offsets: Todos en 0
```

**Opción B - Usando Offsets**:
```
Anchors: Min (0, 0) Max (1, 1)  ← Stretch completo

Offsets:
├── Left: 30 px
├── Right: 30 px
├── Top: 50 px
└── Bottom: 50 px
```

Esto deja espacio alrededor donde las zonas de fondo serán visibles.

### PASO 3: Ajustar Colores para Mayor Visibilidad

Como las zonas estarán parcialmente cubiertas, usa colores **más brillantes**:

**En el Inspector de `DeploymentZoneFeedback`**:

```
Available Color (verde brillante en bordes):
R: 100  (0.4)
G: 255  (1.0)
B: 100  (0.4)
A: 220  (0.86) ← Más opaco

Left Highlight Color (azul vibrante):
R: 50   (0.2)
G: 150  (0.6)
B: 255  (1.0)
A: 240  (0.94) ← Muy opaco

Right Highlight Color (naranja vibrante):
R: 255  (1.0)
G: 150  (0.6)
B: 50   (0.2)
A: 240  (0.94) ← Muy opaco
```

---

## 🎯 Verificación Visual en Play Mode

### Test 1: Sin Carta Seleccionada
```
Vista:
┌─────────────────────────────────┐
│     ┌───────────────┐           │
│     │               │           │
│     │  Mapa+Torres  │           │ ← RawImage visible
│     │      🏰        │           │
│     └───────────────┘           │
│  (Zonas ocultas detrás)         │
└─────────────────────────────────┘
```

### Test 2: Carta Seleccionada - Vista Completa
```
Vista:
┌─────────────────────────────────┐
│🟢🟢│ ┌───────────────┐ │🟢🟢  │ ← Bordes verdes visibles
│🟢🟢│ │               │ │🟢🟢  │
│🟢🟢│ │  Mapa+Torres  │ │🟢🟢  │
│🟢🟢│ │      🏰        │ │🟢🟢  │
│🟢🟢│ └───────────────┘ │🟢🟢  │
└─────────────────────────────────┘
  ↑                           ↑
Izq verde                 Der verde
```

### Test 3: Hover en Zona Izquierda
```
Vista:
┌─────────────────────────────────┐
│🔵🔵│ ┌───────────────┐ │🟢🟢  │
│🔵🔵│ │               │ │🟢🟢  │ ← Azul vs Verde
│🔵🔵│ │  Mapa+Torres  │ │🟢🟢  │
│🔵🔵│ │      🏰        │ │🟢🟢  │
│🔵🔵│ └───────────────┘ │🟢🟢  │
└─────────────────────────────────┘
  ↑                           ↑
Azul resaltado           Verde normal
```

✅ **El jugador entiende**:
- Zonas verdes en bordes = Puede desplegar
- Azul izquierda = La tropa irá por el camino izquierdo
- Naranja derecha = La tropa irá por el camino derecho

---

## 🎨 Variantes de Diseño

### Variante 1: Marco Grueso (Más Visible)

Redimensiona el MapRawImage para dejar **más espacio**:

```
RectTransform del MapRawImage:
├── Anchors: Min (0.1, 0.15) Max (0.9, 0.85)
└── Deja 10% a los lados, 15% arriba/abajo
```

Resultado: Marco de zonas más grueso y visible

### Variante 2: Solo Bordes Laterales

Si quieres que solo se vean los bordes izquierdo/derecho:

```
RectTransform del MapRawImage:
├── Anchors: Min (0.08, 0) Max (0.92, 1)
└── Solo márgenes laterales, sin top/bottom
```

Resultado: Franjas verticales de color a los lados

### Variante 3: Transparencia Sutil del Mapa

Si quieres que las zonas se vean "a través" del mapa:

1. Selecciona `MapRawImage`
2. Add Component → **Canvas Group**
3. Alpha: `0.95` (95% opaco, 5% transparente)

Las zonas de fondo se verán muy sutilmente a través del mapa.

---

## 🐛 Solución de Problemas

### No veo las zonas en absoluto

**Causa**: MapRawImage cubre completamente las zonas

**Soluciones**:
1. ✅ Redimensiona MapRawImage dejando márgenes (Paso 2)
2. ✅ Verifica que `DeploymentZoneFeedback` esté **antes** de `MapRawImage` en Hierarchy
3. ✅ Aumenta el Alpha de los colores de las zonas (más opacos)

### Las zonas se ven muy tenues/difíciles de ver

**Solución**:
1. Aumenta el **Alpha** de los colores (ej: 240-255)
2. Usa colores más **vibrantes** (más saturados)
3. Aumenta los **márgenes** del MapRawImage (más espacio visible)

### Los clicks no funcionan en las zonas visibles

**Verificar**:
1. ✅ `Raycast Target` de las zonas debe estar **desactivado**
2. ✅ `PlayableAreaUI` debe estar en el `MapRawImage`, no en las zonas
3. ✅ El área clickeable es el `MapRawImage`, no las zonas de fondo

### Las zonas se ven pixeladas o borrosas

**Solución**:
1. Usa un **sprite blanco simple** como Source Image
2. O deja **None** (Unity usará un cuadrado blanco por defecto)
3. El color se controla desde el componente Image, no la textura

---

## 📊 Diagrama de Configuración Final

```
Canvas (Sort Order: 0 o cualquiera)
│
├── DeploymentZoneFeedback ← PRIMERO (atrás)
│   ├── DeploymentZoneFeedback (script)
│   │   ├── leftZoneImage:  [LeftDeploymentZone]
│   │   ├── rightZoneImage: [RightDeploymentZone]
│   │   └── Colores configurados
│   │
│   ├── LeftDeploymentZone (Image)
│   │   ├── Anchors: (0, 0) to (0.5, 1)
│   │   ├── Raycast Target: OFF
│   │   └── Color: (controlado por script)
│   │
│   └── RightDeploymentZone (Image)
│       ├── Anchors: (0.5, 0) to (1, 1)
│       ├── Raycast Target: OFF
│       └── Color: (controlado por script)
│
└── MapRawImage ← DESPUÉS (delante, cubre el centro)
    ├── Anchors: (0.05, 0.1) to (0.95, 0.9) ← MÁS PEQUEÑO
    ├── RawImage
    │   └── Texture: RenderTexture (mapa+torres)
    │
    └── PlayableAreaUI (script)
        ├── playerManager: [PlayerCardManager]
        ├── worldCamera: [Main Camera]
        ├── mapRawImage: [self]
        └── zoneFeedback: [DeploymentZoneFeedback] ← Apunta al objeto de atrás
```

---

## 📝 Checklist Final

- [ ] `DeploymentZoneFeedback` está **antes** de `MapRawImage` en Hierarchy
- [ ] `MapRawImage` tiene anchors reducidos (ej: 0.05-0.95) dejando márgenes
- [ ] Colores de las zonas son brillantes y con Alpha alto (>200)
- [ ] `Raycast Target` desactivado en ambas zonas
- [ ] `PlayableAreaUI.zoneFeedback` apunta a `DeploymentZoneFeedback`
- [ ] En Play Mode: Seleccionar carta → ver bordes de color
- [ ] En Play Mode: Mover cursor → bordes cambian de azul a naranja
- [ ] En Play Mode: Click funciona y despliega tropa
- [ ] Torres y mapa completamente visibles en el centro

---

## 🎓 Resumen de la Solución

**La clave**: No puedes poner las zonas "entre" el mapa y las torres porque son la misma imagen. Entonces:

1. **Zonas VAN DETRÁS** del mapa (primero en Hierarchy)
2. **MapRawImage SE HACE MÁS PEQUEÑO** para dejar ver los bordes
3. **Colores BRILLANTES** para que los bordes sean muy visibles
4. **Resultado**: Marco de color alrededor del mapa que indica las zonas

El jugador ve claramente:
- 🟢 Verde = Zonas disponibles
- 🔵 Azul izquierda = Camino izquierdo
- 🟠 Naranja derecha = Camino derecho
- 🏰 Torres y mapa totalmente visibles en el centro

---

**Fecha**: 24 de Octubre de 2025  
**Solución**: ✅ Validada para mapa+torres en misma RenderTexture

---

**Fecha**: 24 de Octubre de 2025  
**Solución**: ✅ Validada para rendering 3D con RenderTexture
