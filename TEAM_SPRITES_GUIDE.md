# Guía de Configuración de Sprites por Equipo

## 🎨 Sprites Diferenciados por Equipo

Los elementos de UI ahora soportan sprites diferentes para cada equipo (Azul/Rojo).

## Configuración de Sprites

### 1. TroopUI - Iconos de Tropas

En el prefab `TroopUI.prefab`:

#### Team Sprites
- **Blue Team Icon**: Sprite para el equipo del jugador (tag: "PlayerTeam")
- **Red Team Icon**: Sprite para el equipo enemigo (tag: "AITeam")
- **Default Icon**: Sprite de respaldo si no se especifica equipo

**Pasos:**
1. Abre `Assets/Prefabs/TroopUI.prefab`
2. Selecciona el objeto raíz
3. En el componente `TroopUI`, sección "Team Sprites":
   - Arrastra tu sprite azul a **Blue Team Icon**
   - Arrastra tu sprite rojo a **Red Team Icon**
   - (Opcional) Arrastra un sprite por defecto a **Default Icon**

### 2. OperationUI - Iconos de Operaciones

En el prefab `OperationUI.prefab`:

#### Team Sprites
- **Blue Team Icon**: Sprite para operaciones del jugador
- **Red Team Icon**: Sprite para operaciones enemigas
- **Default Icon**: Sprite de respaldo

**Pasos:**
1. Abre `Assets/Prefabs/OperationUI.prefab`
2. Selecciona el objeto raíz
3. En el componente `OperationUI`, sección "Team Sprites":
   - Arrastra tu sprite azul a **Blue Team Icon**
   - Arrastra tu sprite rojo a **Red Team Icon**
   - (Opcional) Arrastra un sprite por defecto a **Default Icon**

### 3. TowerHealthBar - Barras de Vida

En el prefab `TowerHealthBar.prefab`:

#### Team Colors
- **Use Team Colors**: Activa esta opción para usar colores sólidos por equipo
- **Blue Team Color**: Color para la torre del jugador
- **Red Team Color**: Color para la torre enemiga

#### Health Colors (si Use Team Colors está desactivado)
- **Full Health Color**: Color cuando >60% vida (por defecto: Verde)
- **Half Health Color**: Color cuando 30-60% vida (por defecto: Amarillo)
- **Low Health Color**: Color cuando <30% vida (por defecto: Rojo)

**Pasos:**
1. Abre `Assets/Prefabs/TowerHealthBar.prefab`
2. Selecciona el objeto raíz
3. En el componente `TowerHealthBar`:

**Opción A - Colores por Equipo (Recomendado para diferenciación clara):**
   - Activa `Use Team Colors`
   - Ajusta `Blue Team Color` (ej: azul claro)
   - Ajusta `Red Team Color` (ej: rojo)

**Opción B - Colores por Salud (Estilo tradicional):**
   - Desactiva `Use Team Colors`
   - Los colores cambiarán según el porcentaje de vida

## Asignación Automática

El sistema detecta automáticamente el equipo basándose en el **tag** del GameObject:
- `"PlayerTeam"` → Usa sprites/colores azules
- `"AITeam"` → Usa sprites/colores rojos

**No necesitas hacer nada adicional en código**, simplemente:
1. Asegúrate de que tus tropas/torres tengan el tag correcto
2. Asigna los sprites apropiados en los prefabs
3. ¡Listo!

## Estructura de Tags en Unity

Verifica que tus GameObjects tengan estos tags:

### Tropas del Jugador
- Tag: `PlayerTeam`
- Usarán sprites azules

### Tropas Enemigas (IA)
- Tag: `AITeam`
- Usarán sprites rojos

### Torres
- Torre del jugador: `PlayerTeam`
- Torre enemiga: `AITeam`

## Ejemplo Visual

```
PlayerTeam (Azul)          AITeam (Rojo)
     1                          2
   [🔵]                       [🔴]
  Tropa                      Tropa

   2+3                        4-1
  [🔵]                       [🔴]
Operación                  Operación

   ████                       ████
  [━━━━]                     [━━━━]
  Torre                      Torre
```

## Personalización Avanzada

### Uso de Sprite Específico (Sobreescribir)

Si necesitas usar un sprite específico que no dependa del equipo, puedes hacerlo desde código:

```csharp
// Para TroopUI
troopUIInstance.Initialize(transform, value, teamTag, customSprite);

// Para OperationUI
operationUIInstance.Initialize(transform, valA, valB, op, teamTag, customSprite);
```

El parámetro `customSprite` sobreescribirá la selección automática por equipo.

### Cambiar Sprites en Runtime

Los sprites se pueden cambiar directamente:

```csharp
TroopUI troopUI = GetComponent<TroopUI>();
troopUI.iconImage.sprite = nuevoSprite;
```

## Tips de Diseño

### Para Sprites de Tropas/Operaciones:
- **Formato recomendado**: PNG con transparencia
- **Tamaño recomendado**: 256x256 px o 512x512 px
- **Forma**: Circular o cuadrada funciona mejor
- **Contraste**: Asegúrate de que el texto sea legible sobre el sprite
- **Colores**: Usa tonos azules para PlayerTeam, rojos para AITeam

### Para Colores de Torres:
- **Azul sugerido**: RGB(51, 128, 255) o #3380FF
- **Rojo sugerido**: RGB(255, 51, 51) o #FF3333
- **Contraste**: Usa colores visibles sobre el fondo del juego

## Solución de Problemas

### Los sprites no cambian según el equipo
✅ Verifica que los GameObjects tengan el tag correcto ("PlayerTeam" o "AITeam")
✅ Asegúrate de que los sprites estén asignados en el prefab
✅ Revisa que el prefab esté asignado en el Inspector del personaje

### El texto no se ve sobre el sprite
✅ Cambia el color del texto a uno que contraste (blanco o negro)
✅ Ajusta el orden de los elementos UI (el texto debe estar encima)
✅ Añade un outline al texto desde el componente TextMeshPro

### Los colores de la torre no cambian
✅ Activa/desactiva `Use Team Colors` según el comportamiento deseado
✅ Verifica que la torre tenga el tag correcto
✅ Asegúrate de que el prefab TowerHealthBar esté asignado

## Checklist de Configuración Completa

- [ ] Sprites azules y rojos importados en Unity
- [ ] TroopUI prefab creado y configurado
- [ ] OperationUI prefab creado y configurado
- [ ] TowerHealthBar prefab creado y configurado
- [ ] Sprites azules asignados en los prefabs
- [ ] Sprites rojos asignados en los prefabs
- [ ] Prefab TroopUI asignado en tropas (1-5.prefab)
- [ ] Prefab OperationUI asignado en operaciones (Suma/Resta.prefab)
- [ ] Prefab TowerHealthBar asignado en torres de la escena
- [ ] Tags correctos en todos los GameObjects
- [ ] Prueba en Play Mode con ambos equipos

¡Todo listo para tener UI diferenciado por equipos! 🎮
