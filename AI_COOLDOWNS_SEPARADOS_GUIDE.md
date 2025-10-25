# Guía: Sistema de Cooldowns Separados para IA

## 📋 Resumen de Cambios

Se ha implementado un sistema de **cooldowns separados** para la IA, dividiendo el intervalo de decisiones en dos categorías independientes:

1. **Intervalo de Ataque**: Tiempo mínimo entre lanzamientos de unidades ofensivas
2. **Intervalo de Defensa**: Tiempo mínimo entre lanzamientos de unidades defensivas

---

## 🎯 Tipos de Acciones

### 🗡️ Ataques (TipoAccion.Ataque)
- **CharacterCombined** (Operaciones: Suma, Resta)
- **Camiones**
- **Operaciones ofensivas**

Acciones implementadas:
- `AccionAtacarConSuma`
- `AccionAtacarConResta`

### 🛡️ Defensas (TipoAccion.Defensa)
- **Characters** (Números individuales)
- **Cartas defensivas**

Acciones implementadas:
- `AccionDefender`

### ⚪ Neutral (TipoAccion.Neutral)
- Acciones que no consumen cooldown específico
- Siempre disponible cuando cualquier cooldown esté listo

Acciones implementadas:
- `AccionEsperar`

---

## ⚙️ Configuración por Dificultad

### Fácil (Defensiva)
```csharp
agresividad = 0.3f        // Prioriza defensa sobre ataque
intervaloAtaque = 1.8f    // IA ataca cada 1.8 segundos (rango: 0.3-10s)
intervaloDefensa = 1.2f   // IA defiende cada 1.2 segundos (rango: 0.3-10s)
regenInterval = 3.5f      // Regeneración lenta de intelecto
```

### Media (Equilibrada)
```csharp
agresividad = 0.5f        // Balance perfecto entre ataque y defensa
intervaloAtaque = 1.2f    // IA ataca cada 1.2 segundos (rango: 0.3-10s)
intervaloDefensa = 0.8f   // IA defiende cada 0.8 segundos (rango: 0.3-10s)
regenInterval = 2.8f      // Regeneración normal de intelecto
```

### Difícil (Agresiva)
```csharp
agresividad = 0.7f        // Prioriza ataque sobre defensa
intervaloAtaque = 0.8f    // IA ataca cada 0.8 segundos (rango: 0.3-10s)
intervaloDefensa = 0.5f   // IA defiende cada 0.5 segundos (rango: 0.3-10s)
regenInterval = 2.2f      // Regeneración rápida de intelecto
```

---

## 🎯 Sistema de Agresividad

La variable `agresividad` controla la **prioridad entre atacar y defender**:

### Multiplicadores de Score

**Ataques:**
- `agresividad = 0.0` → Multiplicador x0.0 (¡No ataca!)
- `agresividad = 0.5` → Multiplicador x1.0 (Normal)
- `agresividad = 1.0` → Multiplicador x2.0 (¡Ataca el doble!)

**Defensas:**
- `agresividad = 0.0` → Multiplicador x2.0 (¡Defiende el doble!)
- `agresividad = 0.5` → Multiplicador x1.0 (Normal)
- `agresividad = 1.0` → Multiplicador x0.0 (¡No defiende!)

### Fórmulas
```csharp
multiplicadorAtaque = agresividad * 2.0f
multiplicadorDefensa = (1.0f - agresividad) * 2.0f
```

### Ejemplos Prácticos

**Agresividad 0.0 (Solo Defensa):**
- Ataque con score 0.8 → 0.8 × 0.0 = **0.0** ❌
- Defensa con score 0.6 → 0.6 × 2.0 = **1.2** ✅
- **Resultado:** Siempre defiende

**Agresividad 0.5 (Equilibrado):**
- Ataque con score 0.8 → 0.8 × 1.0 = **0.8** 
- Defensa con score 0.6 → 0.6 × 1.0 = **0.6**
- **Resultado:** Elige según situación táctica

**Agresividad 1.0 (Solo Ataque):**
- Ataque con score 0.8 → 0.8 × 2.0 = **1.6** ✅
- Defensa con score 0.6 → 0.6 × 0.0 = **0.0** ❌
- **Resultado:** Siempre ataca

---

## 🔧 Cómo Funciona

### 1. Timers Independientes

```csharp
private float tiempoAcumuladoAtaque = 0f;   // Timer para ataques
private float tiempoAcumuladoDefensa = 0f;  // Timer para defensas
```

Ambos timers se actualizan en paralelo en cada frame:

```csharp
void Update()
{
    tiempoAcumuladoAtaque += Time.deltaTime;
    tiempoAcumuladoDefensa += Time.deltaTime;
    
    bool puedeAtacar = tiempoAcumuladoAtaque >= intervaloAtaque;
    bool puedeDefender = tiempoAcumuladoDefensa >= intervaloDefensa;
    
    if (puedeAtacar || puedeDefender)
    {
        TomarDecision(puedeAtacar, puedeDefender);
    }
}
```

### 2. Filtrado de Acciones

El sistema evalúa **solo las acciones disponibles** según qué cooldown haya expirado:

```csharp
foreach (AIAction accion in accionesPosibles)
{
    bool accionDisponible = false;
    
    if (accion.tipoAccion == AIAction.TipoAccion.Ataque)
    {
        accionDisponible = puedeAtacar;
    }
    else if (accion.tipoAccion == AIAction.TipoAccion.Defensa)
    {
        accionDisponible = puedeDefender;
    }
    else // Neutral (Esperar)
    {
        accionDisponible = puedeAtacar || puedeDefender;
    }
    
    if (!accionDisponible)
        continue; // Saltar esta acción
    
    // ... calcular score y elegir mejor acción
}
```

### 3. Reset Selectivo de Cooldowns

Solo se resetea el cooldown de la acción ejecutada:

```csharp
if (mejorAccion.tipoAccion == AIAction.TipoAccion.Ataque)
{
    tiempoAcumuladoAtaque = 0f;  // Solo resetear ataque
}
else if (mejorAccion.tipoAccion == AIAction.TipoAccion.Defensa)
{
    tiempoAcumuladoDefensa = 0f; // Solo resetear defensa
}
```

---

## 🎮 Ventajas del Sistema

### ✅ Control Preciso de Comportamiento
- **Cooldowns separados**: Ajustar independientemente frecuencia de ataques y defensas
- **Agresividad como prioridad**: Control exacto del balance táctico
- **Mayor flexibilidad**: Crear perfiles de IA muy distintos

### ✅ Sistema de Prioridad Inteligente
```
Agresividad 0.0 = Tank defensivo (solo defiende)
Agresividad 0.3 = Cauteloso (prefiere defender)
Agresividad 0.5 = Balanceado (táctico)
Agresividad 0.7 = Agresivo (prefiere atacar)
Agresividad 1.0 = Rush extremo (solo ataca)
```

### ✅ Comportamiento Más Natural
- La IA puede defender más rápido que atacar
- La agresividad no afecta la calidad de decisiones, solo la prioridad
- Reacciones defensivas ágiles ante amenazas

### ✅ Mejor Balanceo
```
FÁCIL:    Ataca lento (1.8s) | Defiende lento (1.2s) | Defensiva (0.3)
MEDIA:    Ataca normal (1.2s) | Defiende rápido (0.8s) | Equilibrada (0.5)
DIFÍCIL:  Ataca rápido (0.8s) | Defiende muy rápido (0.5s) | Agresiva (0.7)
```

---

## 📊 Ejemplo de Comportamiento

### Escenario: IA en dificultad MEDIA

**Timeline:**
```
T=0.0s:  Cooldowns reseteados
         [Ataque: 0.0/1.2s] [Defensa: 0.0/0.8s]

T=0.8s:  Defensa disponible
         [Ataque: 0.8/1.2s] [Defensa: ✅ LISTO]
         → IA evalúa solo DEFENSAS → Defiende amenaza
         → Reset cooldown defensa

T=1.2s:  Ataque disponible
         [Ataque: ✅ LISTO] [Defensa: 0.4/0.8s]
         → IA evalúa solo ATAQUES → Ataca con suma
         → Reset cooldown ataque

T=1.6s:  Defensa disponible nuevamente
         [Ataque: 0.4/1.2s] [Defensa: ✅ LISTO]
         → IA puede defender de nuevo
```

---

## 🔍 Debugging

### Logs de Decisión
```
========== [IA] NUEVA DECISIÓN (t=12.5s) ==========
[IA] Puede Atacar: True | Puede Defender: True
[IA] Intelecto: 5/10
[IA] Cartas en mano: 4
[IA] Amenazas detectadas: 2
[IA]   • Defender Ataque: 0.850
[IA]     → Score DEFENSA ajustado por agresividad (0.70): x0.60
[IA]   • Defender Ataque: 0.510
[IA]   • Atacar con Suma: 0.745
[IA]     → Score ATAQUE ajustado por agresividad (0.70): x1.40
[IA]   • Atacar con Suma: 1.043
[IA]   • Atacar con Resta: 0.623
[IA]     → Score ATAQUE ajustado por agresividad (0.70): x1.40
[IA]   • Atacar con Resta: 0.872
[IA] ⭐ DECISIÓN: Atacar con Suma (score: 1.043)
[IA] ⏱️ Cooldown de ATAQUE reseteado (0.8s)
==========================================================
```

**Nota:** En este ejemplo con agresividad 0.7:
- Los ataques reciben un boost de x1.40
- Las defensas reciben una penalización de x0.60
- La IA prefiere atacar incluso con amenazas presentes

---

## 🛠️ Ajustes en Runtime

### Cambiar Intervalos desde Inspector
```csharp
public float intervaloAtaque = 1.2f;  // Rango: 0.5-10 segundos
public float intervaloDefensa = 0.8f; // Rango: 0.5-10 segundos
```

**Ejemplos de configuración:**
- **IA muy rápida**: `intervaloAtaque = 0.5s` | `intervaloDefensa = 0.3s`
- **IA estándar**: `intervaloAtaque = 1.2s` | `intervaloDefensa = 0.8s`
- **IA lenta/torpe**: `intervaloAtaque = 5.0s` | `intervaloDefensa = 3.0s`
- **IA casi inactiva**: `intervaloAtaque = 10s` | `intervaloDefensa = 10s`

### Forzar Decisión (Testing)
```csharp
iaController.ForzarDecision(); // Ignora cooldowns
```

---

## 📝 Archivos Modificados

1. **AIAction.cs**
   - Añadido enum `TipoAccion`
   - Añadida propiedad `tipoAccion`

2. **AIActionDefender.cs**
   - Marcada como `TipoAccion.Defensa`

3. **AIAtacarConSuma.cs**
   - Marcada como `TipoAccion.Ataque`

4. **AIAtacarConResta.cs**
   - Marcada como `TipoAccion.Ataque`

5. **AiActionWait.cs**
   - Marcada como `TipoAccion.Neutral`

6. **AICardManager.cs** (Cambios principales)
   - Reemplazado `intervaloDecision` por `intervaloAtaque` y `intervaloDefensa`
   - Actualizado `DifficultySettings` con dos intervalos
   - Implementados timers separados
   - Modificado `Update()` para cooldowns paralelos
   - Modificado `TomarDecision()` con filtrado por tipo

---

## ✨ Resultado Final

La IA ahora puede:
- ✅ **Cooldowns independientes**: Reaccionar más rápido a defensas mientras mantiene ataques espaciados
- ✅ **Agresividad como prioridad real**: 
  - `0.0` = Solo defiende (multiplicador ataque x0)
  - `0.5` = Equilibrado (ambos x1)
  - `1.0` = Solo ataca (multiplicador defensa x0)
- ✅ **Perfiles de dificultad distintos**: Fácil defensivo, Media equilibrado, Difícil agresivo
- ✅ **Sistema limpio**: Multiplicadores centralizados, no código duplicado en cada acción
- ✅ **Balanceo fino**: Control total sobre el comportamiento táctico de la IA

**¡Sistema implementado con éxito!** 🎉
