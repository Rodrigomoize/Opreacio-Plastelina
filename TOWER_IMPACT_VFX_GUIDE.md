# Guía de Configuración: VFX de Impacto en Torre

## 🎯 Sistema VFX para Combined → Torre

Este VFX es específico para cuando los Combined (operaciones/camiones) impactan en las torres enemigas. Es más grande y dramático que el VFX normal de combate.

---

## 📋 Crear el Prefab VFX

### 1. Crear GameObject Base

1. **Hierarchy** → Clic derecho → Create Empty
2. Nombre: `TowerImpactVFX`
3. Añadir componente: `TowerImpactVFX.cs`

---

### 2. Añadir Sistemas de Partículas

Crea varios sistemas de partículas como hijos:

#### **A. Partículas Principales (Explosion Burst)**
- Hierarchy → Clic derecho en TowerImpactVFX → Effects → Particle System
- Nombre: `ExplosionParticles`
- Configuración:
  ```
  Duration: 1.0
  Start Lifetime: 0.5 - 1.5
  Start Speed: 5 - 15 (velocidad alta para dispersión)
  Start Size: 0.2 - 0.5
  Start Color: Naranja/Amarillo (gradiente)
  
  Emission:
    Rate over Time: 0
    Bursts: 1 burst de 50-100 partículas al inicio
  
  Shape:
    Shape: Sphere
    Radius: 0.5
  
  Velocity over Lifetime:
    Linear: Y = -5 (gravedad)
  
  Color over Lifetime:
    Gradient: Amarillo → Naranja → Rojo → Transparente
  
  Size over Lifetime:
    Curve: Crece ligeramente y luego se reduce
  ```

#### **B. Partículas de Chispas**
- Duplicate `ExplosionParticles`
- Nombre: `Sparks`
- Modificar:
  ```
  Start Size: 0.05 - 0.1 (más pequeñas)
  Start Speed: 10 - 20 (más rápidas)
  Start Color: Amarillo brillante
  Bursts: 30-50 partículas
  
  Trails:
    Enabled: true
    Lifetime: 0.2
    Width: 0.02
  ```

#### **C. Humo (Opcional)**
- Particle System
- Nombre: `Smoke`
- Configuración:
  ```
  Duration: 2.0
  Start Lifetime: 1.5 - 2.5
  Start Speed: 1 - 3 (lento)
  Start Size: 0.5 - 1.5
  Start Color: Gris oscuro
  
  Emission:
    Bursts: 10-20 partículas
  
  Velocity over Lifetime:
    Linear Y: 2 (sube)
  
  Size over Lifetime:
    Curve: Crece con el tiempo
  ```

#### **D. Onda de Choque (Opcional)**
- Particle System
- Nombre: `Shockwave`
- Configuración:
  ```
  Duration: 0.5
  Start Lifetime: 0.3
  Start Speed: 0
  Start Size: 0.1
  
  Emission:
    Bursts: 1 partícula
  
  Size over Lifetime:
    Curve: Crece rápidamente (0 → 5)
  
  Color over Lifetime:
    Blanco brillante → Transparente
  
  Renderer:
    Render Mode: Horizontal Billboard
    Material: Un material con textura de anillo/onda
  ```

---

### 3. Añadir Mesh de Explosión

1. **Crear esfera/mesh:**
   - Hierarchy → Clic derecho en TowerImpactVFX → 3D Object → Sphere
   - Nombre: `ExplosionMesh`
   - Scale: (0, 0, 0) inicial

2. **Material:**
   - Crear material nuevo con shader `Standard` o `Unlit`
   - Color: Naranja brillante con emisión
   - Transparency: Enabled
   - Asignar al mesh

3. **Configurar en script:**
   - En `TowerImpactVFX` component:
     - Arrastrar `ExplosionMesh` al campo `Explosion Mesh`

---

### 4. Configurar el Script TowerImpactVFX

En el Inspector del objeto `TowerImpactVFX`:

#### **Particle Systems:**
- Size: 4 (o cuantos sistemas añadiste)
- Element 0: ExplosionParticles
- Element 1: Sparks
- Element 2: Smoke
- Element 3: Shockwave

#### **Explosion Settings:**
- **Explosion Mesh**: ExplosionMesh (GameObject)
- **Explosion Scale Multiplier**: `2.0` (el doble que el VFX normal)
- **Explosion Duration**: `0.3` (explosión rápida)

#### **Camera Shake:**
- **Enable Camera Shake**: ✅ Checked
- **Shake Magnitude**: `0.3` (sacudida moderada)
- **Shake Duration**: `0.2`

#### **Flash Effect:**
- **Enable Flash**: ✅ Checked
- **Flash Color**: Naranja (RGB: 255, 128, 0, Alpha: 77)
- **Flash Duration**: `0.15`

---

### 5. Guardar como Prefab

1. Arrastra `TowerImpactVFX` desde Hierarchy a `Assets/Prefabs/VFX/`
2. Elimina el objeto de la escena

---

## 🎮 Asignar al CharacterCombined

1. Selecciona todos los **prefabs de CharacterCombined**
2. En el Inspector, sección **"VFX Torre"**:
   - **Tower Impact VFX Prefab**: Arrastra `TowerImpactVFX` prefab
   - **Shrink Speed**: `5` (velocidad de encogimiento antes de explotar)
   - **Tower VFX Offset**: `1` (altura de la explosión)

---

## 🎨 Diferencias con el VFX Normal

| Característica | VFX Combate Normal | VFX Torre |
|----------------|-------------------|-----------|
| Tamaño | 1x | 2x (más grande) |
| Partículas | Moderadas | Muchas (explosivas) |
| Camera Shake | ❌ No | ✅ Sí |
| Screen Flash | ❌ No | ✅ Sí (naranja) |
| Duración explosión | 0.2s | 0.3s |
| Onda de choque | ❌ No | ✅ Sí (opcional) |
| Humo | Poco | Más humo |

---

## 💡 Ideas Adicionales

### Efectos Avanzados:
1. **Light Component**: Añade una luz naranja que crece y se desvanece
2. **Audio Source**: Sonido de explosión
3. **Decal**: Marca de quemadura en el suelo
4. **Distorsión**: Efecto de calor/distorsión del aire

### Variaciones por Valor:
Puedes crear diferentes intensidades según el valor de la operación:
- Operación valor 1-2: Explosión pequeña
- Operación valor 3-4: Explosión mediana
- Operación valor 5: Explosión grande

---

## 🔧 Ajustes Finos

### Si la explosión es muy grande:
- Reduce `explosionScaleMultiplier` (ej: 1.5 en vez de 2.0)
- Reduce número de partículas en los bursts

### Si la explosión es muy pequeña:
- Aumenta `explosionScaleMultiplier` (ej: 2.5 o 3.0)
- Aumenta número de partículas

### Si quieres más impacto visual:
- Aumenta `shakeMagnitude` (ej: 0.5)
- Aumenta intensidad del flash color
- Añade más sistemas de partículas

### Para optimización:
- Reduce número de partículas si hay lag
- Desactiva Camera Shake y Screen Flash
- Usa menos sistemas de partículas

---

## ✨ Resultado Final

Cuando un Combined llega a una torre:
1. 🎯 Combined se encoge hacia la torre
2. 💥 **GRAN EXPLOSIÓN** con partículas disparadas
3. 📹 Cámara sacude
4. 🌟 Flash naranja en pantalla
5. 🏰 Torre recibe daño y animación "hit"
6. 💨 Humo y partículas se disipan

¡Una experiencia visual mucho más impactante que el combate normal! 🎮✨
