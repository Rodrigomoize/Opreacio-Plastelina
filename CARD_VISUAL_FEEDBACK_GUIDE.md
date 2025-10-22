# Guía de Feedback Visual de Cartas

## Descripción
Sistema mejorado de feedback visual para cartas que indica claramente el estado de cada carta según el contexto de selección.

## Colores de Cartas

### 1. **Blanco** (`validCardColor`) - Carta Válida
- **Cuándo**: Carta disponible para seleccionar
- **Ejemplo**: 
  - Sin selección: Todas las cartas
  - Con carta 2 y operador +: Cartas 1, 2, 3 (porque 2+1=3, 2+2=4, 2+3=5)

### 2. **Amarillo Claro** (`selectedCardColor`) - Carta Seleccionada
- **Cuándo**: Carta seleccionada (primera selección, o una de dos cartas diferentes)
- **Ejemplo**:
  - Seleccionas carta 3: La carta 3 se pone amarilla clara
  - Seleccionas 2 + y luego 3: Ambas cartas (2 y 3) amarillas claras

### 3. **Rosa/Magenta** (`doubleSelectedCardColor`) - 🌟 AUTO-COMBINACIÓN
- **Cuándo**: La MISMA carta está seleccionada 2 veces (auto-combinación válida)
- **Color por defecto**: RGB(1, 0.5, 1) - Rosa/magenta
- **Ejemplo**:
  - Carta 1 + luego carta 1 otra vez: La carta 1 cambia a **ROSA** (1+1=2 es válido)
  - Carta 2 + luego carta 2 otra vez: La carta 2 cambia a **ROSA** (2+2=4 es válido)
- **Feedback visual claro**: "Esta carta está siendo usada DOS VECES"

### 4. **Amarillo Oscuro/Grisáceo** (`selectedButBlockedCardColor`) - 🚫 SELECCIONADA PERO BLOQUEADA
- **Cuándo**: Carta está seleccionada CON operador activo, pero NO puede seleccionarse de nuevo
- **Color por defecto**: RGB(0.7, 0.7, 0.4, 0.8) - Amarillo grisáceo
- **Ejemplo**:
  - Carta 2 - activo: La carta 2 está en **AMARILLO GRISÁCEO** (porque 2-2=0, no válido)
  - Carta 3 + activo: La carta 3 está en **AMARILLO GRISÁCEO** (porque 3+3=6>5, no válido)
- **Feedback visual claro**: "Esta carta está seleccionada pero NO puedes usarla otra vez"

### 5. **Gris** (`invalidCardColor`) - Carta Inválida/Bloqueada
- **Cuándo**: Carta no disponible para seleccionar
- **Ejemplo**:
  - Con carta 3 y operador +: Cartas 4 y 5 en gris (porque 3+4=7, 3+5=8, ambos >5)
  - Con carta 2 y operador -: Cartas 3, 4, 5 en gris (porque 2-3=-1, negativo)

## Casos de Uso Visuales

### Caso 1: Auto-combinación válida (1+1)
```
Paso 1: Selecciona carta 1
  → Carta 1: AMARILLO CLARO (seleccionada)
  → Resto: BLANCO (disponibles)

Paso 2: Presiona botón +
  → Carta 1: AMARILLO CLARO (seleccionada)
  → Cartas 1,2,3,4: BLANCO (disponibles)
  → Carta 5: GRIS (bloqueada, porque 1+5=6)

Paso 3: Selecciona carta 1 de nuevo
  → Carta 1: ROSA/MAGENTA ✨ (doble selección)
  → Resto: BLANCO/GRIS según validez
  → Mensaje visual claro: "Estás usando el 1 DOS VECES"
```

### Caso 2: Carta seleccionada pero bloqueada para segunda selección (2-)
```
Paso 1: Selecciona carta 2
  → Carta 2: AMARILLO CLARO (seleccionada)
  → Resto: BLANCO (disponibles)

Paso 2: Presiona botón -
  → Carta 2: AMARILLO GRISÁCEO 🚫 (seleccionada pero bloqueada)
  → Carta 1: BLANCO (disponible, 2-1=1 es válido)
  → Cartas 3,4,5: GRIS (bloqueadas, resultados negativos)
  → Mensaje visual claro: "El 2 está seleccionado pero NO puedes hacer 2-2"
```

### Caso 3: Auto-combinación inválida (3+)
```
Paso 1: Selecciona carta 3
  → Carta 3: AMARILLO CLARO (seleccionada)
  → Resto: BLANCO (disponibles)

Paso 2: Presiona botón +
  → Carta 3: AMARILLO GRISÁCEO 🚫 (seleccionada pero bloqueada)
  → Cartas 1,2: BLANCO (disponibles, 3+1=4, 3+2=5)
  → Cartas 4,5: GRIS (bloqueadas, 3+4=7, 3+5=8)
  → Mensaje visual claro: "El 3 está seleccionado pero NO puedes hacer 3+3"
```

### Caso 4: Dos cartas diferentes (2+3)
```
Paso 1: Selecciona carta 2
  → Carta 2: AMARILLO CLARO (seleccionada)

Paso 2: Presiona botón +
  → Carta 2: AMARILLO GRISÁCEO (seleccionada pero bloqueada)
  → Otras cartas según validez

Paso 3: Selecciona carta 3
  → Carta 2: AMARILLO CLARO (primera selección)
  → Carta 3: AMARILLO CLARO (segunda selección)
  → Resto: según estado
```

## Personalización de Colores

En el Inspector de Unity, puedes ajustar estos colores en el componente `PlayerCardManager`:

### Colores Recomendados:

#### Para Auto-combinación (Double Selected):
- **Rosa brillante**: RGB(1, 0.5, 1) - Color por defecto
- **Cian brillante**: RGB(0.5, 1, 1) - Alternativa fría
- **Verde lima**: RGB(0.7, 1, 0.5) - Alternativa natural

#### Para Seleccionada Pero Bloqueada:
- **Amarillo grisáceo**: RGB(0.7, 0.7, 0.4, 0.8) - Color por defecto
- **Naranja oscuro**: RGB(0.8, 0.5, 0.3, 0.8) - Más cálido
- **Gris amarillento**: RGB(0.6, 0.6, 0.5, 0.9) - Más sutil

## Ventajas del Sistema

✅ **Claridad visual inmediata**: Los jugadores ven instantáneamente:
- Qué cartas están seleccionadas
- Si una carta está siendo usada 2 veces (rosa)
- Si una carta está seleccionada pero no puede usarse otra vez (amarillo grisáceo)
- Qué cartas están disponibles vs bloqueadas

✅ **Prevención de confusión**: 
- Ya no hay duda sobre si una carta puede seleccionarse de nuevo
- El color rosa/magenta destaca claramente las auto-combinaciones válidas
- El color amarillo grisáceo indica "seleccionada pero no disponible"

✅ **Feedback educativo**: 
- Los jugadores aprenden rápidamente qué combinaciones son válidas
- Los colores refuerzan las reglas del juego visualmente

## Notas Técnicas

- Los colores se actualizan automáticamente en cada cambio de estado
- El sistema es retrocompatible con el código existente
- Los logs de Debug muestran qué color se está aplicando
- Se puede extender fácilmente para añadir más estados visuales

## Próximas Mejoras (Opcional)

1. **Animaciones**: Hacer que las cartas rosa "pulsen" sutilmente
2. **Partículas**: Añadir efecto de partículas cuando se logra una auto-combinación
3. **Sonidos**: Sonido especial cuando se selecciona la misma carta 2 veces
4. **Tooltip**: Mostrar tooltip explicativo al pasar el mouse sobre cartas bloqueadas
