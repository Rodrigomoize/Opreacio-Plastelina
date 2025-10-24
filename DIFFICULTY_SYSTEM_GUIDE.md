# Sistema de Dificultad de la IA

## Resumen
El sistema de dificultad ajusta automáticamente tres parámetros clave para proporcionar diferentes niveles de desafío. **Todos los valores son configurables desde el Inspector de Unity.**

## Parámetros Afectados por Dificultad

### 1. **Agresividad**
- **Fácil**: 0.43 (muy defensiva)
- **Media**: 0.60 (equilibrada)
- **Difícil**: 0.80 (muy agresiva)

Controla la tendencia de la IA a atacar vs defender.

### 2. **Intervalo de Decisión**
- **Fácil**: 1.5 segundos (IA más lenta)
- **Media**: 1.0 segundos (velocidad normal)
- **Difícil**: 0.6 segundos (IA muy rápida)

Controla cada cuánto tiempo la IA toma una decisión de juego.

### 3. **Velocidad de Regeneración de Intelecto**
- **Fácil**: 3.5 segundos por punto (regenera más lento)
- **Media**: 2.8 segundos por punto (estándar Clash Royale)
- **Difícil**: 2.2 segundos por punto (regenera más rápido)

Controla la velocidad a la que **AMBOS JUGADORES** (jugador y IA) recuperan su intelecto para desplegar tropas.

> **Nota:** El `gameSpeed` (velocidad de movimiento de tropas) ahora se controla desde otro script y NO está incluido en este sistema.

## Implementación Técnica

### Archivo Modificado
`Assets/Scripts/Combat/AI Player/AICardManager.cs`

### Sistema de Intervalo Estricto
La IA ahora tiene un **intervalo completamente estricto** entre decisiones:

- ✅ **SIEMPRE** respeta el `intervaloDecision` configurado
- ❌ **NO hay excepciones** por amenazas críticas
- ❌ **NO hay reacciones rápidas** bajo ninguna circunstancia
- ❌ **NO hay cooldowns reducidos** por proximidad de enemigos

Ejemplo con `intervaloDecision = 3.0s` (Fácil):
- Primera decisión: t=3.0s
- Segunda decisión: t=6.0s
- Tercera decisión: t=9.0s
- **Siempre exactamente 3.0s entre cada decisión**

Esto garantiza que la dificultad configurada se respete al 100%.

### Estructura de Configuración
```csharp
[System.Serializable]
public class DifficultySettings
{
    [Range(0f, 1f)]
    public float agresividad = 0.5f;
    
    [Range(0.3f, 3f)]
    public float intervaloDecision = 1.0f;
    
    [Range(1.5f, 5f)]
    public float regenInterval = 2.8f;
}

// Configuraciones editables en el Inspector
public DifficultySettings facil;
public DifficultySettings media;
public DifficultySettings dificil;

// Referencias necesarias
public IntelectManager intelectManagerIA;
public IntelectManager intelectManagerPlayer;
```

### Método `SetDificultad(AIDificultad dificultad)`
```csharp
public void SetDificultad(AIDificultad dificultad)
{
    DifficultySettings settings = null;
    
    switch (dificultad)
    {
        case AIDificultad.Facil: settings = facil; break;
        case AIDificultad.Media: settings = media; break;
        case AIDificultad.Dificil: settings = dificil; break;
    }

    if (settings != null)
    {
        agresividad = settings.agresividad;
        intervaloDecision = settings.intervaloDecision;
        
        // Aplica regenInterval a AMBOS jugadores
        intelectManagerIA.regenInterval = settings.regenInterval;
        intelectManagerPlayer.regenInterval = settings.regenInterval;
    }
    
    InicializarIA();
}
```

## Configuración en Unity Inspector

### Ubicación
1. Abre la escena de combate
2. Selecciona el GameObject con el componente `IAController`
3. En el Inspector, busca la sección **"Ajustes de Dificultad"**

### Referencias Requeridas
Asegúrate de asignar estas referencias en el Inspector:
- **Intelect Manager IA**: El IntelectManager de la IA
- **Intelect Manager Player**: El IntelectManager del jugador

### Ajustes Disponibles
Verás tres secciones expandibles:
- **Facil**: Valores para dificultad fácil
- **Media**: Valores para dificultad media
- **Dificil**: Valores para dificultad difícil

Cada una contiene sliders para:
- **Agresividad** (0.0 - 1.0)
- **Intervalo Decision** (0.3s - 3.0s)
- **Regen Interval** (1.5s - 5.0s) - Aplica a AMBOS jugadores

### Ejemplo de Ajuste Personalizado
Si quieres hacer la dificultad Media más agresiva:
1. Expande la sección `Media`
2. Ajusta `Agresividad` a 0.75
3. Los cambios se aplicarán la próxima vez que llames `SetDificultad(AIDificultad.Media)`

## Efecto en el Gameplay

### Dificultad Fácil 🟢
- La IA piensa más lentamente (1.5s entre decisiones)
- Juega más defensivamente (agresividad 0.43)
- **AMBOS jugadores generan intelecto más lento (3.5s por punto)** 🔋
- **Resultado**: Partidas más relajadas, ritmo más lento para ambos

### Dificultad Media 🟡
- Velocidad de decisión equilibrada (1.0s)
- Equilibrio ataque/defensa (agresividad 0.6)
- **AMBOS jugadores regeneran a velocidad estándar (2.8s por punto)** 🔋
- **Resultado**: Partidas equilibradas, ritmo normal

### Dificultad Difícil 🔴
- La IA piensa muy rápido (0.6s entre decisiones)
- Muy agresiva (agresividad 0.8)
- **AMBOS jugadores generan intelecto rápido (2.2s por punto)** 🔋
- **Resultado**: Partidas intensas, presión constante, más recursos disponibles

## Uso

El método `SetDificultad()` se llama automáticamente al iniciar la partida o cambiar la dificultad desde el menú principal.

```csharp
// Ejemplo de uso
IAController iaController = GetComponent<IAController>();
iaController.SetDificultad(IAController.AIDificultad.Facil);
```

## Ajustes Recomendados

### Desde el Inspector de Unity
Todos los valores ahora son **100% configurables desde el Inspector**. No necesitas modificar código.

### Sugerencias de Balance

**Para hacer más fácil:**
- ⬆️ Aumentar `intervaloDecision` (IA decide más lento)
- ⬆️ Aumentar `regenInterval` (ambos jugadores regeneran más lento)
- ⬇️ Reducir `agresividad` (IA más defensiva)

**Para hacer más difícil:**
- ⬇️ Reducir `intervaloDecision` (IA decide más rápido)
- ⬇️ Reducir `regenInterval` (ambos jugadores regeneran más rápido)
- ⬆️ Aumentar `agresividad` (IA más agresiva)

### Valores Extremos
- **Modo Tutorial**: `intervaloDecision = 3.0s`, `regenInterval = 5.0s`
- **Modo Pesadilla**: `intervaloDecision = 0.3s`, `regenInterval = 1.5s`, `agresividad = 1.0`

## Notas Importantes

1. ✅ **Todos los valores son editables en el Inspector** - No necesitas tocar código
2. 🔋 El `regenInterval` afecta a **AMBOS JUGADORES** (jugador e IA por igual)
3. 🤖 El `intervaloDecision` y `agresividad` solo afectan a la IA
4. ⚡ Los cambios son **inmediatos** al llamar `SetDificultad()`
5. 🔧 Puedes crear dificultades personalizadas ajustando los valores en el Inspector
6. 🎮 El `gameSpeed` (velocidad de tropas) se controla desde otro script
7. ⚠️ Debes asignar **ambas referencias** de IntelectManager en el Inspector:
   - `intelectManagerIA` (IA)
   - `intelectManagerPlayer` (Jugador)

## Uso

El método `SetDificultad()` se llama automáticamente al iniciar la partida o cambiar la dificultad desde el menú principal.

```csharp
// Ejemplo de uso
IAController iaController = GetComponent<IAController>();
iaController.SetDificultad(IAController.AIDificultad.Media);
```

Esto aplicará automáticamente:
- Agresividad de la IA
- Intervalo de decisión de la IA
- Velocidad de regeneración de intelecto (ambos jugadores)
