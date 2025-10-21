# Guía de Velocidad de Juego

## 📋 Descripción
Se ha implementado un sistema de **Velocidad de Juego** que permite controlar la velocidad de movimiento de todas las tropas y operaciones en tiempo real.

## 🎮 Características

### ✅ Lo que hace:
- Controla la velocidad de movimiento de TODAS las tropas (simples y combinadas)
- Afecta tanto a tropas del jugador como de la IA
- Se puede cambiar en tiempo real desde el Inspector
- Funciona con un multiplicador de velocidad (0.1x a 3.0x)
- Actualiza automáticamente todas las tropas activas cuando cambias el valor

### 🎯 Velocidades:
- **0.1x** - 10% de velocidad (cámara lenta)
- **1.0x** - Velocidad normal (default)
- **2.0x** - Doble velocidad
- **3.0x** - Triple velocidad

## 🔧 Configuración

### Paso 1: Agregar el GameSpeedManager a la Escena

1. Abre la escena de juego (`PlayScene.unity`)
2. Crea un GameObject vacío: `GameObject > Create Empty`
3. Renómbralo a `GameSpeedManager`
4. Agrega el componente: `Add Component > Game Speed Manager`

### Paso 2: Configurar en el Inspector

En el componente **GameSpeedManager** verás:

```
┌─────────────────────────────────────┐
│ Game Speed Manager (Script)         │
├─────────────────────────────────────┤
│ ▼ Configuración de Velocidad       │
│   Game Speed Multiplier   [1.0]    │
│   ────────────────────────────      │
│   0.1              1.5              3.0 │
└─────────────────────────────────────┘
```

### Paso 3: Probar en Tiempo de Ejecución

1. Inicia el juego (Play)
2. En el Inspector, cambia el valor del slider **Game Speed Multiplier**
3. Observa cómo todas las tropas ajustan su velocidad instantáneamente

## 💻 Implementación Técnica

### Archivos Modificados:

1. **`GameSpeedManager.cs`** (NUEVO)
   - Manager singleton que controla la velocidad global
   - Método `GetAdjustedSpeed()` que aplica el multiplicador
   - Actualización automática de todas las tropas cuando cambia el valor

2. **`Character.cs`** (MODIFICADO)
   - Añadido método `UpdateSpeed()` 
   - Velocidad ajustada automáticamente al multiplicador de juego

3. **`CharacterCombined.cs`** (MODIFICADO)
   - Añadido método `UpdateSpeed()`
   - Operaciones ajustan su velocidad al multiplicador

### Funcionamiento:

```csharp
// La velocidad final se calcula así:
velocidadFinal = velocidadBase * multiplicadorDeJuego

// Ejemplo con velocidad base de 2.0:
multiplicador = 1.5x
velocidadFinal = 2.0 * 1.5 = 3.0
```

## 🎨 Uso en Código

Si necesitas acceder al multiplicador desde otro script:

```csharp
// Obtener el multiplicador actual
float multiplicador = GameSpeedManager.Instance.GameSpeedMultiplier;

// Cambiar el multiplicador por código
GameSpeedManager.Instance.GameSpeedMultiplier = 2.0f;

// Obtener velocidad ajustada
float velocidadBase = 2.5f;
float velocidadAjustada = GameSpeedManager.Instance.GetAdjustedSpeed(velocidadBase);
```

## 🐛 Solución de Problemas

### Las tropas no cambian de velocidad:
1. Verifica que el `GameSpeedManager` esté en la escena
2. Asegúrate de que está activo (checkbox marcado)
3. Comprueba la consola para mensajes de error

### El slider no aparece:
- El componente debe estar agregado correctamente
- Verifica que el script esté compilado sin errores

### Las tropas van demasiado rápido o lento:
- Ajusta el valor del slider entre 0.1 y 3.0
- El valor por defecto es 1.0 (velocidad normal)

## 📝 Notas

- El sistema es completamente opcional - si no agregas el GameSpeedManager, las tropas usarán su velocidad base normal
- Es un Singleton - solo debe haber uno en la escena
- Los cambios son instantáneos - todas las tropas activas se actualizan inmediatamente
- Funciona para tropas ya existentes y tropas que se creen después del cambio

## 🔮 Mejoras Futuras Posibles

- Botones preconfigurados (0.5x, 1x, 2x, 3x)
- Atajos de teclado para cambiar velocidad
- Guardar la velocidad preferida del jugador
- Efectos visuales cuando cambia la velocidad
- Limitaciones por dificultad (ej: la IA no puede usar velocidad rápida)
