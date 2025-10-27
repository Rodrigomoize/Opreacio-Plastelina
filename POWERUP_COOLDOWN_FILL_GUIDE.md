# Guía de Configuración: Fill Visual de Cooldown para PowerUps

## 📋 Descripción
Sistema visual de cooldown para PowerUps similar a la barra de intelecto. El fill se vacía cuando se usa el PowerUp y se llena gradualmente conforme se recarga.

---

## 🎨 Configuración en Unity

### **Paso 1: Estructura de UI para cada PowerUp Button**

Cada botón de PowerUp debe tener esta jerarquía:

```
PowerUpButton (Button component)
├── Icon (Image) - El icono del PowerUp
├── CooldownFill (Image) - El fill visual del cooldown
└── Background (Image) - Fondo del botón (opcional)
```

### **Paso 2: Configurar el CooldownFill Image**

1. **Selecciona el GameObject `CooldownFill`**
2. En el componente `Image`:
   - **Image Type**: `Filled`
   - **Fill Method**: `Radial 360` (para efecto circular tipo reloj)
     - O `Vertical` (para llenar de abajo hacia arriba como la barra de intelecto)
   - **Fill Origin**: 
     - Si usas `Radial 360`: `Top` (empieza desde arriba)
     - Si usas `Vertical`: `Bottom` (empieza desde abajo)
   - **Clockwise**: ✓ (activado)
   - **Fill Amount**: 1.0 (se controlará por código)

3. **Configuración visual:**
   - **Color**: Color semi-transparente (ej: blanco con alpha 0.5-0.8)
   - **Material**: Dejar por defecto o usar uno con efecto de brillo
   - **Raycast Target**: ❌ (desactivar, no debe recibir clicks)

4. **Posicionamiento:**
   - **Anchor**: Stretch en ambos ejes (llena todo el botón)
   - **Left/Right/Top/Bottom**: 0, 0, 0, 0 (ocupa todo el espacio del padre)
   - **Z Position**: -1 (detrás del icono pero delante del background)

### **Paso 3: Asignar en PowerUpManager**

1. **Selecciona el GameObject con el componente `PowerUpManager`**
2. En el Inspector, expande la lista `Power Ups`
3. Para cada PowerUp:
   - **Power Up Name**: Nombre del PowerUp (`"SlowTime"`, `"Health"`, etc.)
   - **Duration**: Duración del efecto (ej: 5.0 para SlowTime, 0 para Health instantáneo)
   - **Cooldown Time**: Tiempo de recarga (ej: 10.0 segundos)
   - **Power Up Button**: Arrastra el GameObject del botón
   - **Cooldown Fill Image**: Arrastra el GameObject `CooldownFill` (el Image con Fill)

---

## 🔧 Configuraciones Recomendadas

### **Opción A: Fill Radial (Tipo Reloj)**
- Más moderno y dinámico
- Perfecto para PowerUps circulares
- **Image Type**: `Filled`
- **Fill Method**: `Radial 360`
- **Fill Origin**: `Top`

**Visual:** El fill gira como un reloj desde arriba, llenándose completamente cuando está disponible.

### **Opción B: Fill Vertical (Tipo Barra de Intelecto)**
- Más claro y directo
- Perfecto para PowerUps rectangulares
- **Image Type**: `Filled`
- **Fill Method**: `Vertical`
- **Fill Origin**: `Bottom`

**Visual:** El fill se llena de abajo hacia arriba, igual que la barra de intelecto.

### **Opción C: Fill Horizontal**
- Alternativa si los botones son anchos
- **Image Type**: `Filled`
- **Fill Method**: `Horizontal`
- **Fill Origin**: `Left`

---

## 🎨 Mejoras Visuales Opcionales

### **1. Sprite personalizado para CooldownFill**
Puedes usar un sprite con forma específica:
- Crear un sprite con forma de círculo, barra, o el contorno del icono
- Asignarlo al componente `Image` del `CooldownFill`
- Ajustar el `Image Type` a `Filled` igualmente

### **2. Gradiente de color**
En vez de un color sólido, puedes:
- Usar un Material con gradiente
- O cambiar el color del fill según el progreso (modificar código)

### **3. Animación al completarse**
Añadir un efecto cuando el cooldown termina:
- Glow/resplandor
- Scale pulse
- Partículas

---

## 📊 Comportamiento del Fill

### **Estados del PowerUp:**

1. **Disponible (isOnCooldown = false, isActive = false)**
   - `fillAmount = 1.0` (completamente lleno)
   - Color del botón: `availableColor` (blanco)
   - Botón interactable: ✓

2. **En Cooldown (isOnCooldown = true)**
   - `fillAmount = cooldownTimer / cooldownTime` (se vacía gradualmente de 1.0 a 0.0)
   - Color del botón: `cooldownColor` (gris)
   - Botón interactable: ❌
   - **Ejemplo**: Si el cooldown es 10s y quedan 7s → fill = 7/10 = 0.7 (70% lleno)

3. **Activo (isActive = true, solo para PowerUps con duración)**
   - `fillAmount = 1.0` (lleno)
   - Color del botón: `activeColor` (verde)
   - Botón interactable: ❌

---

## 🧪 Prueba en Unity

1. **Inicia el juego**
2. **Usa un PowerUp** (clic en el botón)
3. **Observa:**
   - El fill debe empezar en 0 (vacío)
   - Se debe llenar gradualmente durante el tiempo de cooldown
   - Al completarse, debe estar en 1.0 (lleno) y el botón debe volver a ser interactable

---

## 🐛 Troubleshooting

### **Problema: El fill no se ve**
- ✅ Verifica que el `CooldownFill` tenga un componente `Image`
- ✅ Verifica que el color tenga alpha > 0
- ✅ Verifica que esté en el layer correcto (delante del background, detrás del icono)

### **Problema: El fill no se anima**
- ✅ Verifica que `Cooldown Fill Image` esté asignado en el PowerUpManager
- ✅ Verifica que `Cooldown Time` sea > 0
- ✅ Verifica que el Image Type esté en `Filled`

### **Problema: El fill va en dirección incorrecta**
- ✅ Cambia el `Fill Origin` del Image
- ✅ Activa/desactiva `Clockwise`

---

## 📝 Código Relevante

El sistema actualiza el fill en `PowerUpManager.cs`:

```csharp
private void UpdatePowerUpUI(PowerUpData p)
{
    if (p.cooldownFillImage != null)
    {
        if (p.isOnCooldown && p.cooldownTime > 0f)
        {
            // El fill va de 1 (lleno) a 0 (vacío) conforme pasa el cooldown
            float fillProgress = p.cooldownTimer / p.cooldownTime;
            p.cooldownFillImage.fillAmount = fillProgress;
        }
        else
        {
            // Cuando está disponible, el fill está completamente lleno
            p.cooldownFillImage.fillAmount = 1f;
        }
    }
}
```

**Lógica:**
- `cooldownTimer` empieza en `cooldownTime` y disminuye a 0
- `fillProgress = timer / total` hace que el fill empiece en 1.0 (lleno) y llegue a 0 (vacío)
- Cuando el cooldown termina, se fuerza a 1.0 (lleno y disponible)

### **Efecto Visual del PowerUp SlowTime:**

El PowerUp de SlowTime ahora muestra un **filtro azul constante** durante toda su duración:

```csharp
private System.Collections.IEnumerator ConstantFilterCoroutine(Color filterColor, float duration)
{
    // Fade in rápido (0.2s)
    // → Mantiene el filtro constante durante la duración
    // → Fade out al final (0.3s)
}
```

- **No hay flashes intermitentes**, solo un overlay azul semi-transparente continuo
- Se activa cuando usas SlowTime y desaparece gradualmente cuando termina el efecto

---

## ✅ Resumen

1. Crea un `Image` hijo en cada botón de PowerUp llamado `CooldownFill`
2. Configúralo como `Filled` (Radial 360 o Vertical)
3. Asígnalo en el PowerUpManager → `Cooldown Fill Image`
4. El código ya está implementado y funcionará automáticamente
5. El fill se **vaciará gradualmente** conforme pasa el cooldown (de lleno a vacío)
6. El PowerUp SlowTime mostrará un **filtro azul constante** durante su duración
