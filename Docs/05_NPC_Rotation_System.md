# Sistema de Rotación del NPC - GaudiNPCRotation

## Introducción

El componente `GaudiNPCRotation` implementa un sistema de rotación suave y natural que hace que el NPC (Gaudí) se gire hacia el jugador durante las conversaciones, creando una experiencia más inmersiva y realista.

## Concepto

En la vida real, cuando hablamos con alguien, naturalmente nos giramos para mantener contacto visual. Este componente replica ese comportamiento en el NPC, haciendo que Gaudí:

- Se gire suavemente hacia la cámara del jugador
- Mantenga al interlocutor en frente durante la conversación
- Use rotación smooth (no instantánea) para naturalidad
- Opcionalmente, solo rote mientras está hablando

## Características Principales

### 1. Rotación Suave (Smooth Rotation)

La rotación usa `Quaternion.RotateTowards()` para interpolar suavemente:

```csharp
transform.rotation = Quaternion.RotateTowards(
    transform.rotation,      // Rotación actual
    targetRotation,          // Rotación objetivo
    rotationSpeed * Time.deltaTime  // Velocidad (grados/segundo)
);
```

**Ventajas**:
- No hay rotación brusca o "snap"
- El movimiento es predecible y natural
- Se adapta automáticamente al framerate

### 2. Threshold de Rotación

Para evitar micro-rotaciones constantes (jitter), solo se activa la rotación si el ángulo de diferencia supera un threshold:

```csharp
float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

if (angleDifference > rotationThreshold)  // Por defecto: 5°
{
    // Rotar
}
else
{
    // Mantener quieto
}
```

**Beneficios**:
- Reduce cálculos innecesarios
- Evita movimiento jittery
- Mejora performance

### 3. Lock de Eje Y (Horizontal Only)

Por defecto, la rotación está limitada al eje Y (horizontal):

```csharp
if (lockYAxisRotation)
{
    direction.y = 0f;  // Ignorar diferencia de altura
}
```

**Por qué es importante**:
- En AR, el usuario puede estar más alto o más bajo que el NPC
- Sin lock, el NPC inclinaría la cabeza hacia arriba/abajo constantemente
- Con lock, solo gira horizontalmente (más natural para conversación casual)

### 4. Modo "Solo al Hablar"

Opción para que el NPC solo rote cuando está hablando:

```csharp
if (onlyRotateWhileTalking)
{
    shouldRotate = shouldRotate && _convaiNPC.IsCharacterTalking;
}
```

**Casos de uso**:
- **Activado**: NPC parece más "vivo" solo durante respuestas
- **Desactivado**: NPC siempre mira al jugador (más "atento")

## Configuración en Unity

### Parámetros del Inspector

#### Rotation Settings

```
Rotation Speed: 90
```
- **Rango sugerido**: 45-180 grados/segundo
- **45°/s**: Muy lento, anciano o pensativo
- **90°/s**: Natural, recomendado para Gaudí
- **180°/s**: Rápido, personaje joven o nervioso
- **360°/s**: Muy rápido, casi instantáneo (no recomendado)

```
Only Rotate While Talking: ✓
```
- **Marcado**: Solo gira mientras responde (recomendado)
- **Desmarcado**: Siempre mira al jugador

```
Rotation Threshold: 5
```
- **Rango sugerido**: 3-15 grados
- **3°**: Muy sensible, puede causar jitter
- **5°**: Equilibrado (recomendado)
- **10°**: Menos rotaciones, más estable
- **15+**: Solo rotaciones grandes

```
Look Height Offset: 0
```
- **0**: Mira al centro de la cámara
- **1.6**: Mira a "altura de ojos" humanos
- **-0.5**: Mira más bajo (niños)
- Útil para ajustar donde "parece" que mira el NPC

#### References

```
Player Transform: [null]
```
- **Null**: Auto-detecta Main Camera (recomendado)
- **Asignado**: Usa transform específico
- Útil para multiplayer o targets personalizados

```
Lock Y Axis Rotation: ✓
```
- **Marcado**: Solo rotación horizontal (recomendado para AR)
- **Desmarcado**: Rotación completa (3 ejes)

#### Debug

```
Show Debug Gizmos: ✓
```
- Muestra líneas y esferas en Scene view
- Solo visible en Editor
- No afecta rendimiento en build

## Gizmos de Debug

Cuando `showDebugGizmos` está activo, se dibujan en Scene view:

### Línea desde NPC a Player
- **Verde**: NPC no está rotando (dentro de threshold)
- **Amarilla**: NPC está rotando activamente

### Rayo Azul
- Indica la dirección forward actual del NPC
- Permite verificar hacia dónde "mira"

### Esfera Amarilla
- Aparece en el punto target cuando está rotando
- Indica el objetivo de la mirada

## Algoritmo Detallado

### Flujo de Ejecución

```
Update() llamado cada frame
  ↓
1. Verificar condiciones
   ├─ ¿NPC existe? → No: salir
   ├─ ¿Player Transform existe? → No: salir
   └─ ¿NPC está activo? → No: salir
  ↓
2. Si "Only Rotate While Talking"
   └─ ¿NPC está hablando? → No: salir
  ↓
3. RotateTowardsPlayer()
   ↓
   a. Calcular posición objetivo
      lookPosition = playerTransform.position + (0, lookHeightOffset, 0)
   ↓
   b. Calcular dirección
      direction = lookPosition - npcPosition
   ↓
   c. Si Lock Y Axis
      direction.y = 0
   ↓
   d. Validar dirección
      if (direction.sqrMagnitude < 0.001) → salir
   ↓
   e. Calcular rotación objetivo
      targetRotation = Quaternion.LookRotation(direction)
   ↓
   f. Calcular diferencia angular
      angleDifference = Quaternion.Angle(currentRotation, targetRotation)
   ↓
   g. Si angleDifference > rotationThreshold
      ├─ _isRotating = true
      └─ Quaternion.RotateTowards(...)
   ↓
   h. Si no
      └─ _isRotating = false
```

### Matemáticas

#### Cálculo de Dirección

```csharp
Vector3 lookPosition = playerTransform.position;
lookPosition.y += lookHeightOffset;

Vector3 direction = lookPosition - transform.position;

if (lockYAxisRotation)
{
    direction.y = 0f;  // Proyección en plano XZ
}
```

Esto crea un vector que apunta del NPC al jugador. Si `lockYAxisRotation` está activo, se proyecta al plano horizontal.

#### Validación de Dirección

```csharp
if (direction.sqrMagnitude < 0.001f)
    return;
```

Evita división por cero en `LookRotation` si NPC y jugador están en exactamente la misma posición.

**Nota**: Usa `sqrMagnitude` (más rápido) en vez de `magnitude` (requiere `sqrt`).

#### Rotación Objetivo

```csharp
Quaternion targetRotation = Quaternion.LookRotation(direction);
```

Crea un quaternion que representa mirar en la dirección calculada.

#### Interpolación Suave

```csharp
transform.rotation = Quaternion.RotateTowards(
    transform.rotation,
    targetRotation,
    rotationSpeed * Time.deltaTime
);
```

- `rotationSpeed`: grados por segundo
- `Time.deltaTime`: tiempo transcurrido desde último frame
- Resultado: movimiento independiente del framerate

## Métodos Públicos

### SnapToPlayer()

Rotación instantánea (sin smooth) hacia el jugador.

```csharp
npcRotation.SnapToPlayer();
```

**Usos**:
- Inicialización (cuando se coloca el NPC)
- Después de teleport/reposición
- Corrección de orientación en momentos específicos

**Ejemplo de uso**:
```csharp
// En ARNPCPlacementController después de colocar NPC
if (_spawnedNPC.TryGetComponent(out GaudiNPCRotation rotation))
{
    rotation.SnapToPlayer();  // Mira inmediatamente al jugador
}
```

### SetLookTarget(Transform target)

Cambia el objetivo a mirar.

```csharp
npcRotation.SetLookTarget(otherPlayer.transform);
```

**Usos**:
- Multiplayer (mirar a jugador específico)
- Cutscenes (mirar a objeto específico)
- Cambio de atención durante conversación

### ResetToDefaultTarget()

Vuelve a mirar a Main Camera.

```csharp
npcRotation.ResetToDefaultTarget();
```

**Usos**:
- Después de mirar a objeto temporal
- Reset de comportamiento

## Propiedades Públicas (Runtime)

### RotationSpeed (get/set)

```csharp
// Obtener velocidad actual
float speed = npcRotation.RotationSpeed;

// Cambiar velocidad en runtime
npcRotation.RotationSpeed = 120f;  // Más rápido
```

**Validación**: No acepta valores negativos (se clampea a 0).

### OnlyRotateWhileTalking (get/set)

```csharp
// Cambiar comportamiento en runtime
npcRotation.OnlyRotateWhileTalking = false;  // Siempre mirar
```

### RotationThreshold (get/set)

```csharp
// Ajustar sensibilidad
npcRotation.RotationThreshold = 10f;  // Menos sensible
```

**Validación**: No acepta valores negativos.

### IsCurrentlyRotating (get only)

```csharp
if (npcRotation.IsCurrentlyRotating)
{
    Debug.Log("El NPC se está girando ahora");
}
```

**Usos**:
- Triggers de animación
- Efectos visuales durante rotación
- Analytics

## Integración con Otros Sistemas

### Con ConvaiNPC

El componente depende de `ConvaiNPC`:

```csharp
[RequireComponent(typeof(ConvaiNPC))]
```

Accede a:
- `isCharacterActive`: Para saber si debe rotar
- `IsCharacterTalking`: Para modo "solo al hablar"

### Con AR Camera

Auto-detecta Main Camera:

```csharp
private void Awake()
{
    _mainCamera = Camera.main;

    if (playerTransform == null && _mainCamera != null)
    {
        playerTransform = _mainCamera.transform;
    }
}
```

En AR, la AR Camera debe tener tag "MainCamera".

### Con GaudiVoiceButton

Aunque no hay dependencia directa, trabajan juntos:

1. Usuario presiona botón de voz
2. ConvaiNPC.StartListening()
3. Usuario habla
4. ConvaiNPC responde (IsCharacterTalking = true)
5. GaudiNPCRotation empieza a rotar (si onlyRotateWhileTalking = true)

## Casos de Uso Avanzados

### Caso 1: Conversación en Grupo

Múltiples jugadores hablando con el NPC:

```csharp
public class GroupConversationController : MonoBehaviour
{
    [SerializeField] private GaudiNPCRotation npcRotation;
    [SerializeField] private Transform[] players;
    private int currentSpeaker = 0;

    public void OnPlayerStartsSpeaking(int playerIndex)
    {
        currentSpeaker = playerIndex;
        npcRotation.SetLookTarget(players[playerIndex]);
    }
}
```

### Caso 2: Atención Dividida

NPC alterna entre jugador y objeto de interés:

```csharp
public class AttentionController : MonoBehaviour
{
    [SerializeField] private GaudiNPCRotation npcRotation;
    [SerializeField] private Transform interestPoint;

    public IEnumerator ShowAndTell()
    {
        // Mirar al objeto
        npcRotation.SetLookTarget(interestPoint);
        yield return new WaitForSeconds(3f);

        // Volver a mirar al jugador
        npcRotation.ResetToDefaultTarget();
    }
}
```

### Caso 3: Velocidad Dinámica

Rotación más rápida cuando el jugador se mueve:

```csharp
public class DynamicRotationSpeed : MonoBehaviour
{
    [SerializeField] private GaudiNPCRotation npcRotation;
    [SerializeField] private Transform player;
    private Vector3 lastPlayerPos;

    void Update()
    {
        float playerSpeed = (player.position - lastPlayerPos).magnitude / Time.deltaTime;
        lastPlayerPos = player.position;

        // Rotar más rápido si jugador se mueve
        npcRotation.RotationSpeed = 90f + (playerSpeed * 10f);
    }
}
```

## Performance

### Optimizaciones Implementadas

1. **Uso de sqrMagnitude**:
   ```csharp
   if (direction.sqrMagnitude < 0.001f)  // No sqrt!
   ```

2. **Early Exit**:
   - Si NPC no está activo → salir inmediatamente
   - Si no hay player transform → salir
   - Si dentro de threshold → no calcular rotación

3. **Cache de Referencias**:
   ```csharp
   private ConvaiNPC _convaiNPC;  // Cached en Awake()
   ```

4. **No Allocations**:
   - No se crean objetos nuevos en Update()
   - Todo es pase por valor o referencias cacheadas

### Costo de Performance

En un dispositivo móvil medio:
- **Update vacío** (early exit): ~0.01ms
- **Update con rotación activa**: ~0.05ms

Para 1 NPC, impacto despreciable. Con 10+ NPCs, considerar:

```csharp
// Throttling de update
private float _updateInterval = 0.1f;  // 10 Hz en vez de 60 Hz
private float _nextUpdate;

void Update()
{
    if (Time.time < _nextUpdate)
        return;

    _nextUpdate = Time.time + _updateInterval;
    // ... resto del código
}
```

## Limitaciones Conocidas

### 1. Solo Rotación, No Inclinación de Cabeza

Actualmente rota todo el cuerpo. Para rotación solo de cabeza:

```csharp
// Requeriría modificación
[SerializeField] private Transform headBone;
// Y aplicar rotación a headBone en vez de transform
```

### 2. No Considera Obstáculos

Si hay pared entre NPC y jugador, igual rota hacia el jugador:

```csharp
// Posible mejora: raycast para detectar obstrucciones
if (Physics.Linecast(transform.position, playerPosition, out hit))
{
    // No rotar si hay obstáculo
}
```

### 3. No Animación de "Mirar Alrededor"

El NPC solo mira al jugador, no tiene comportamiento idle de mirar alrededor:

```csharp
// Posible mejora: sistema de "look at points" cuando idle
```

## Troubleshooting

### Problema: NPC no rota

**Checklist**:
1. ✓ Componente GaudiNPCRotation está añadido
2. ✓ ConvaiNPC existe en el mismo GameObject
3. ✓ `isCharacterActive` es true
4. ✓ `onlyRotateWhileTalking` = false O `IsCharacterTalking` = true
5. ✓ `playerTransform` no es null
6. ✓ `rotationSpeed` > 0

**Debug**:
```csharp
Debug.Log($"Active: {_convaiNPC.isCharacterActive}");
Debug.Log($"Talking: {_convaiNPC.IsCharacterTalking}");
Debug.Log($"Should Rotate: {shouldRotate}");
```

### Problema: Rotación jittery (nerviosa)

**Causas**:
- Threshold muy bajo
- Player transform se mueve en círculos pequeños
- Framerate bajo

**Soluciones**:
1. Aumentar `rotationThreshold` a 8-10°
2. Añadir smoothing al player position
3. Throttle update frequency

### Problema: Rotación demasiado lenta

**Solución**:
```csharp
npcRotation.RotationSpeed = 180f;  // O más
```

### Problema: NPC inclina hacia arriba/abajo

**Causa**: `lockYAxisRotation` está desmarcado

**Solución**:
```csharp
npcRotation component > Lock Y Axis Rotation: ✓
```

## Testing

### Test 1: Rotación Básica

```
1. Colocar NPC en escena
2. Añadir GaudiNPCRotation
3. Play
4. Mover cámara alrededor del NPC en círculo
5. Verificar que NPC rota para seguir cámara
```

**Esperado**: Rotación suave, sin jitter

### Test 2: Threshold

```
1. Configurar rotationThreshold = 30°
2. Play
3. Mover cámara solo 20°
```

**Esperado**: NPC NO rota (bajo threshold)

```
4. Mover cámara 40° total
```

**Esperado**: NPC rota (sobre threshold)

### Test 3: Only While Talking

```
1. Configurar onlyRotateWhileTalking = true
2. Play
3. Mover cámara (NPC idle)
```

**Esperado**: NPC NO rota

```
4. Hablar con NPC
5. Mover cámara mientras NPC responde
```

**Esperado**: NPC rota durante respuesta

### Test 4: Snap vs Smooth

```
1. Play
2. Llamar npcRotation.SnapToPlayer()
```

**Esperado**: Rotación instantánea

```
3. Mover cámara 90°
4. Dejar que rote normalmente
```

**Esperado**: Rotación suave y gradual

## Mejoras Futuras

### 1. Rotación de Cabeza Separada

Usar IK o bone manipulation para rotar solo cabeza:

```csharp
// Pseudocódigo
headBone.rotation = Quaternion.Slerp(
    headBone.rotation,
    lookRotation,
    headRotationSpeed * Time.deltaTime
);
```

### 2. Look At Smoothing

Añadir smoothing adicional con Lerp:

```csharp
Vector3 smoothedDirection = Vector3.Lerp(
    currentLookDirection,
    targetDirection,
    smoothingFactor * Time.deltaTime
);
```

### 3. Anticipación (Overshoot)

Rotar ligeramente más allá del target y volver (más natural):

```csharp
// Animation curve para overshoot
targetRotation += Quaternion.Euler(0, overshootAmount, 0);
```

### 4. Eye Tracking

Combinar con animación de ojos:

```csharp
leftEye.LookAt(playerTransform);
rightEye.LookAt(playerTransform);
```

### 5. Contexto de Conversación

Diferentes velocidades según emoción:

```csharp
if (npc.CurrentEmotion == Emotion.Excited)
    rotationSpeed = 150f;
else if (npc.CurrentEmotion == Emotion.Thoughtful)
    rotationSpeed = 45f;
```

## Conclusión

`GaudiNPCRotation` es un componente robusto y flexible que añade un nivel significativo de inmersión a las conversaciones con el NPC. Su diseño modular permite fácil integración y customización, mientras que las optimizaciones aseguran buen rendimiento incluso en dispositivos móviles.

La combinación con `GaudiVoiceButton` y el sistema Convai completo crea una experiencia conversacional natural y convincente, donde el NPC realmente parece prestar atención al jugador.

## Referencias

### Archivos Relacionados
- `/Assets/Scripts/GaudiNPCRotation.cs` - Implementación
- `/Assets/Scripts/GaudiVoiceButton.cs` - Input de voz
- `/Assets/Convai/Scripts/Runtime/Core/ConvaiNPC.cs` - NPC core

### Unity Documentation
- [Quaternion.RotateTowards](https://docs.unity3d.com/ScriptReference/Quaternion.RotateTowards.html)
- [Quaternion.LookRotation](https://docs.unity3d.com/ScriptReference/Quaternion.LookRotation.html)
- [Quaternion.Angle](https://docs.unity3d.com/ScriptReference/Quaternion.Angle.html)
