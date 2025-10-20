# Elementos de UI para Calcul Fatal

Este documento explica cómo configurar y usar los nuevos elementos de UI añadidos al juego.

## Componentes creados

### 1. TroopUI - Iconos para Tropas
Muestra un sprite con un número encima de cada tropa indicando su valor numérico (1-5).

**Script:** `Assets/Scripts/Combat/TroopUI.cs`

### 2. OperationUI - Iconos para Operaciones
Muestra un sprite con la operación matemática (ej: 1+3, 2-1) encima de los personajes combinados.

**Script:** `Assets/Scripts/Combat/OperationUI.cs`

### 3. TowerHealthBar - Barras de Vida para Torres
Muestra una barra de vida sobre las torres que cambia de color según el nivel de daño.

**Script:** `Assets/Scripts/Combat/TowerHealthBar.cs`

## Configuración en Unity

### Paso 1: Crear los Prefabs de UI

Hay dos formas de crear los prefabs:

#### Opción A: Usar el menú automático (Recomendado)
1. En Unity, ve al menú superior: `Tools > Calcul Fatal > Create All UI Prefabs`
2. Esto creará automáticamente los 3 prefabs en `Assets/Prefabs/`

#### Opción B: Crear manualmente cada prefab
- `Tools > Calcul Fatal > Create TroopUI Prefab`
- `Tools > Calcul Fatal > Create OperationUI Prefab`
- `Tools > Calcul Fatal > Create TowerHealthBar Prefab`

### Paso 2: Personalizar los Prefabs (Opcional)

Puedes personalizar cada prefab según tus necesidades:

#### TroopUI Prefab
- **Icon**: Cambia el sprite y color del fondo
- **ValueText**: Ajusta el tamaño, fuente y color del texto
- **Offset**: En el componente TroopUI, ajusta el offset Y para cambiar la altura sobre la tropa

#### OperationUI Prefab
- **Icon**: Cambia el sprite y color del fondo
- **OperationText**: Ajusta el tamaño, fuente y color del texto
- **Offset**: En el componente OperationUI, ajusta el offset Y para cambiar la altura

#### TowerHealthBar Prefab
- **Background**: Cambia el color del fondo de la barra
- **Fill**: El color cambia automáticamente según la vida:
  - Verde: > 60%
  - Amarillo: 30-60%
  - Rojo: < 30%
- **Offset**: En el componente TowerHealthBar, ajusta el offset Y para cambiar la altura

### Paso 3: Asignar Prefabs a los Personajes

#### Para Character (Tropas simples):
1. Selecciona cada prefab de tropa en `Assets/Prefabs/Characters/`
2. En el Inspector, busca el campo `Troop UI Prefab`
3. Arrastra el prefab `TroopUI` desde `Assets/Prefabs/`

#### Para CharacterCombined (Operaciones):
1. Selecciona los prefabs de operaciones: `Suma.prefab` y `Resta.prefab` en `Assets/Prefabs/Characters/`
2. En el Inspector, busca el campo `Operation UI Prefab`
3. Arrastra el prefab `OperationUI` desde `Assets/Prefabs/`

#### Para Tower (Torres):
1. En la escena, selecciona cada torre (TorrePlayer, TorreEnemiga)
2. En el Inspector, busca el componente `Tower`
3. Asigna el campo `Max Health` (por defecto: 100)
4. Arrastra el prefab `TowerHealthBar` al campo `Health Bar Prefab`

## Características de los Scripts

### TroopUI
- **Sigue automáticamente** a la tropa
- **Mira siempre a la cámara** (billboard effect)
- Se destruye cuando la tropa es eliminada
- Muestra el valor numérico de la tropa (1-5)

### OperationUI
- **Sigue automáticamente** al personaje combinado
- **Mira siempre a la cámara** (billboard effect)
- Se destruye cuando el personaje combinado es eliminado
- Muestra la operación completa (ej: "1+3", "5-2")

### TowerHealthBar
- **Sigue automáticamente** a la torre
- **Mira siempre a la cámara** (billboard effect)
- **Cambio de color dinámico**:
  - Verde cuando la vida es alta (>60%)
  - Amarillo cuando está dañada (30-60%)
  - Rojo cuando está crítica (<30%)
- Se actualiza automáticamente cuando la torre recibe daño
- Se integra con el sistema de daño existente

## API de Programación

### Character.cs
```csharp
public GameObject troopUIPrefab; // Asignar en Inspector
private TroopUI troopUIInstance; // Se crea automáticamente
```

### CharacterCombined.cs
```csharp
public GameObject operationUIPrefab; // Asignar en Inspector
private OperationUI operationUIInstance; // Se crea automáticamente

// El CharacterManager llama a este método automáticamente:
public void SetOperationValues(int valA, int valB, char op)
```

### Tower.cs
```csharp
public GameObject healthBarPrefab; // Asignar en Inspector
public int maxHealth = 100; // Configurable en Inspector

// Métodos públicos:
public void TakeDamage(int damage)
public void Heal(int amount)
public int GetCurrentHealth()
public bool IsDead()
```

## Solución de Problemas

### El UI no aparece
- Verifica que los prefabs estén asignados en el Inspector
- Asegúrate de que TextMesh Pro esté importado en el proyecto
- Revisa que la cámara principal esté etiquetada como "MainCamera"

### El UI está muy grande o muy pequeño
- Ajusta el `localScale` del Canvas en el prefab (por defecto: 0.01)
- Modifica el offset en el componente UI para cambiar la distancia

### El texto no se ve
- Verifica que el color del texto no sea transparente
- Asegúrate de que TextMesh Pro esté correctamente configurado
- Revisa el tamaño del fontSize en el componente TextMeshProUGUI

### La barra de vida no cambia de color
- Verifica que los colores estén asignados en el componente TowerHealthBar
- Asegúrate de que el componente Tower esté llamando a `TakeDamage()`

## 🎨 Sprites Diferenciados por Equipo

**¡NUEVO!** Los elementos de UI ahora soportan sprites diferentes para cada equipo.

### Configuración Rápida

Cada prefab de UI tiene ahora una sección **"Team Sprites"**:

#### TroopUI y OperationUI
- **Blue Team Icon**: Sprite para el equipo del jugador (tag: "PlayerTeam")
- **Red Team Icon**: Sprite para el equipo enemigo (tag: "AITeam")
- **Default Icon**: Sprite de respaldo

#### TowerHealthBar
- **Use Team Colors**: Activa para usar colores sólidos por equipo
- **Blue Team Color**: Color para la torre del jugador
- **Red Team Color**: Color para la torre enemiga

El sistema detecta automáticamente el equipo según el **tag** del GameObject.

📖 **Ver guía completa**: `TEAM_SPRITES_GUIDE.md`

### TeamSpriteManager (Opcional)

Si necesitas gestión centralizada de sprites, puedes usar el `TeamSpriteManager`:

1. Crea un GameObject vacío en tu escena
2. Añade el componente `TeamSpriteManager`
3. Asigna todos los sprites de equipo en un solo lugar
4. Accede desde cualquier script: `TeamSpriteManager.Instance.GetTroopSprite(teamTag)`

## Próximas Mejoras

Posibles mejoras futuras:
- Añadir animaciones al UI (scale, fade)
- Efectos de partículas cuando se destruyen personajes
- Sonidos al cambiar la vida de las torres
- UI de feedback cuando las unidades colisionan
- Iconos personalizados por tipo de tropa

## Notas Técnicas

- Todos los UI usan **World Space Canvas** para seguir a los objetos 3D
- El sistema de **billboard** hace que los UI siempre miren a la cámara
- Los UI se destruyen automáticamente cuando sus objetos padre son eliminados
- El sistema está optimizado para no afectar el rendimiento significativamente
