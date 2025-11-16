# Implementación de Input mediante Botón UI

## Resumen de Cambios

Se ha creado un nuevo sistema de input basado en botones UI que reemplaza la funcionalidad de la tecla "T" del sistema original de Convai. Este cambio es fundamental para la versión móvil AR del proyecto GaudIA.

## Motivación

### Problema Original
El sistema de Convai por defecto usa la tecla "T" del teclado para iniciar/detener la grabación de voz:
- **Desktop**: Funcional mediante `ConvaiInputManager` que escucha `KeyCode.T`
- **Mobile**: No funcional (dispositivos móviles no tienen teclado físico)
- **AR**: Inadecuado para experiencias AR donde el usuario sostiene el dispositivo

### Solución Implementada
Creación de un componente UI especializado (`GaudiVoiceButton`) que:
- Reemplaza completamente la necesidad de presionar "T"
- Funciona mediante eventos de puntero (táctil en móvil, mouse en desktop)
- Proporciona feedback visual y auditivo
- Es completamente configurable desde el Inspector de Unity

## Archivos Creados

### 1. GaudiVoiceButton.cs
**Ubicación**: `Assets/Scripts/GaudiVoiceButton.cs`

**Descripción**: Componente principal que maneja el input de voz mediante un botón UI.

**Características**:
- Hereda de `MonoBehaviour` (NO de Button como `ConvaiTalkButtonHandler`)
- Implementa `IPointerDownHandler` y `IPointerUpHandler` para detección de toque
- Maneja automáticamente la suscripción a eventos de `ConvaiNPCManager`
- Proporciona feedback visual configurable
- Soporta feedback auditivo opcional
- Actualiza texto de estado en tiempo real

**Ventajas sobre ConvaiTalkButtonHandler original**:

| Característica | ConvaiTalkButtonHandler | GaudiVoiceButton |
|----------------|------------------------|------------------|
| Herencia | Hereda de `Button` | Usa `MonoBehaviour` + `RequireComponent` |
| Configuración | Limitada | Altamente configurable |
| Feedback visual | Solo escala y alpha | Color, escala, texto de estado |
| Feedback auditivo | No | Sí (opcional) |
| Documentación | Básica | Extensa con tooltips |
| Estado visual | Solo durante presión | Idle, Recording, Disabled |
| Métodos públicos | Limitados | API completa para control externo |

### 2. GaudiNPCRotation.cs
**Ubicación**: `Assets/Scripts/GaudiNPCRotation.cs`

**Descripción**: Componente que hace que el NPC rote suavemente hacia el jugador durante la conversación.

**Características**:
- Rotación suave y configurable
- Opción de rotar solo mientras habla
- Threshold de ángulo para evitar micro-rotaciones
- Lock de rotación en eje Y (solo horizontal)
- Sistema de gizmos para debugging
- Métodos para snap instantáneo
- Soporte para targets personalizados

## Implementación Detallada

### GaudiVoiceButton - Estructura

#### Propiedades Configurables

```csharp
[Header("Visual Feedback")]
[SerializeField] private Image buttonImage;           // Imagen del botón
[SerializeField] private Color idleColor;             // Color inactivo
[SerializeField] private Color recordingColor;        // Color grabando
[SerializeField] private float pressedScale = 1.2f;   // Escala al presionar
[SerializeField] private TMPro.TextMeshProUGUI statusText; // Texto de estado

[Header("Audio Feedback")]
[SerializeField] private AudioSource audioSource;     // Fuente de audio
[SerializeField] private AudioClip startRecordingSound; // Sonido inicio
[SerializeField] private AudioClip stopRecordingSound;  // Sonido fin
```

#### Ciclo de Vida

```csharp
Awake()
  ↓ Obtiene referencias a Button, Image, guarda escala original

Start()
  ↓ Se suscribe a ConvaiNPCManager.OnActiveNPCChanged
  ↓ Obtiene NPC activo actual
  ↓ Actualiza estado del botón

OnEnable()
  ↓ Re-suscribe a eventos (por si el botón se desactiva/reactiva)

OnDisable()
  ↓ Des-suscribe de eventos
  ↓ Detiene grabación si estaba activa

OnDestroy()
  ↓ Limpieza final de suscripciones
```

#### Flujo de Interacción

```
Usuario toca botón
  ↓
OnPointerDown(PointerEventData)
  ↓
StartVoiceRecording()
  ↓
├─ _isRecording = true
├─ _currentActiveNPC.playerInteractionManager.UpdateActionConfig()
├─ _currentActiveNPC.StartListening()
├─ UpdateVisuals() → buttonImage.color = recordingColor
├─ transform.localScale = _originalScale * pressedScale
└─ audioSource.PlayOneShot(startRecordingSound)

[Usuario habla al micrófono]

Usuario suelta botón
  ↓
OnPointerUp(PointerEventData)
  ↓
StopVoiceRecording()
  ↓
├─ _isRecording = false
├─ _currentActiveNPC.StopListening()
├─ UpdateVisuals() → buttonImage.color = idleColor
├─ transform.localScale = _originalScale
└─ audioSource.PlayOneShot(stopRecordingSound)

Convai procesa audio y responde
  ↓
[Ver flujo en 01_Convai_System_Documentation.md]
```

#### Estados Visuales

El botón tiene tres estados visuales diferentes:

1. **Disabled** (No hay NPC activo)
   - `Button.interactable = false`
   - statusText: "Acércate a Gaudí"
   - Color: Gris (por configuración de Button)

2. **Idle** (Listo para grabar)
   - `Button.interactable = true`
   - `_isRecording = false`
   - buttonImage.color = idleColor (blanco por defecto)
   - statusText: "Mantén para hablar"
   - Escala: original

3. **Recording** (Grabando voz)
   - `Button.interactable = true`
   - `_isRecording = true`
   - buttonImage.color = recordingColor (rojo por defecto)
   - statusText: "Escuchando..."
   - Escala: original * pressedScale (1.2x por defecto)

### GaudiNPCRotation - Estructura

#### Propiedades Configurables

```csharp
[Header("Rotation Settings")]
[SerializeField] private float rotationSpeed = 90f;          // Grados por segundo
[SerializeField] private bool onlyRotateWhileTalking = true; // Solo al hablar
[SerializeField] private float rotationThreshold = 5f;       // Ángulo mínimo
[SerializeField] private float lookHeightOffset = 0f;        // Offset altura

[Header("References")]
[SerializeField] private Transform playerTransform;          // Target (auto-detecta)
[SerializeField] private bool lockYAxisRotation = true;      // Solo Y axis

[Header("Debug")]
[SerializeField] private bool showDebugGizmos = true;        // Gizmos en Scene view
```

#### Algoritmo de Rotación

```csharp
Update()
  ↓
  ¿NPC activo?
  ↓ Sí
  ¿Solo rotar al hablar?
  ↓ Sí
  ¿NPC está hablando?
  ↓ Sí
  RotateTowardsPlayer()
    ↓
    1. Calcular lookPosition = playerTransform.position + lookHeightOffset
    2. Calcular direction = lookPosition - npcPosition
    3. Si lockYAxisRotation: direction.y = 0
    4. Calcular targetRotation = Quaternion.LookRotation(direction)
    5. Calcular angleDifference = Quaternion.Angle(current, target)
    6. Si angleDifference > rotationThreshold:
       ├─ _isRotating = true
       └─ Quaternion.RotateTowards(current, target, rotationSpeed * deltaTime)
    7. Si no:
       └─ _isRotating = false
```

#### Métodos Públicos Útiles

```csharp
SnapToPlayer()
  └─ Rotación instantánea sin smooth (útil para inicialización)

SetLookTarget(Transform target)
  └─ Cambiar el objetivo a mirar (útil para múltiples jugadores)

ResetToDefaultTarget()
  └─ Volver a mirar a la cámara principal

// Propiedades públicas
RotationSpeed { get; set; }
OnlyRotateWhileTalking { get; set; }
RotationThreshold { get; set; }
IsCurrentlyRotating { get; } // Read-only
```

## Configuración en Unity

### Paso 1: Preparar la Escena

1. Asegurarse de que la escena tiene:
   - `ConvaiNPCManager` (singleton)
   - `ConvaiInputManager` (singleton) - aunque ya no usa tecla T
   - Al menos un GameObject con `ConvaiNPC`

### Paso 2: Crear UI Button

1. Crear un Canvas si no existe:
   - GameObject > UI > Canvas
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080 (o según target)

2. Crear el botón de voz:
   - Clic derecho en Canvas > UI > Button
   - Renombrar a "VoiceButton"
   - Posicionar (ej: esquina inferior derecha)
   - Ajustar tamaño (ej: 150x150 para móvil)

3. Customizar visualmente:
   - Cambiar sprite del Image por un icono de micrófono
   - Opcional: Añadir outline/shadow para mejor visibilidad

4. Añadir texto de estado (opcional):
   - Clic derecho en VoiceButton > UI > Text - TextMeshPro
   - Renombrar a "StatusText"
   - Posicionar debajo o dentro del botón
   - Configurar font, size, alignment

### Paso 3: Añadir Componente GaudiVoiceButton

1. Seleccionar el VoiceButton
2. Add Component > GaudiVoiceButton
3. Configurar en el Inspector:

```
GaudiVoiceButton
├─ Visual Feedback
│  ├─ Button Image: [arrastrar Image del botón] (auto-detecta si está vacío)
│  ├─ Idle Color: RGB(255, 255, 255) - Blanco
│  ├─ Recording Color: RGB(255, 76, 76) - Rojo
│  ├─ Pressed Scale: 1.2
│  └─ Status Text: [arrastrar StatusText] (opcional)
│
└─ Audio Feedback (opcional)
   ├─ Audio Source: [añadir AudioSource y arrastrar]
   ├─ Start Recording Sound: [arrastrar AudioClip]
   └─ Stop Recording Sound: [arrastrar AudioClip]
```

4. **IMPORTANTE**: Eliminar cualquier función en "Button > On Click()" ya que usamos PointerDown/Up

### Paso 4: Añadir GaudiNPCRotation al NPC

1. Seleccionar el GameObject del NPC (que tiene ConvaiNPC)
2. Add Component > GaudiNPCRotation
3. Configurar en el Inspector:

```
GaudiNPCRotation
├─ Rotation Settings
│  ├─ Rotation Speed: 90 (grados/segundo)
│  ├─ Only Rotate While Talking: ✓ (marcado)
│  ├─ Rotation Threshold: 5 (grados)
│  └─ Look Height Offset: 0 (o 1.6 para altura de ojos humanos)
│
├─ References
│  ├─ Player Transform: [vacío - auto-detecta Main Camera]
│  └─ Lock Y Axis Rotation: ✓ (marcado para rotación solo horizontal)
│
└─ Debug
   └─ Show Debug Gizmos: ✓ (para ver en Scene view)
```

### Paso 5: Testing

1. **Modo Play en Editor**:
   - Mirar al NPC
   - Esperar a que se active (crosshair/indicador)
   - Click en botón de voz
   - Hablar al micrófono
   - Soltar botón
   - Verificar que NPC responde y rota hacia cámara

2. **Debug Visual**:
   - En Scene view, con Gizmos activos
   - Línea verde/amarilla desde NPC a cámara
   - Rayo azul = dirección forward del NPC
   - Esfera amarilla = target de mirada (cuando rota)

## Comparación con Sistema Original

### Convai Original (Tecla T)

```
Usuario presiona T
  ↓
ConvaiInputManager.Update()
  ↓ Input.GetKeyDown(TalkKey)
  ↓
ConvaiInputManager.talkKeyInteract.Invoke(true)
  ↓
ConvaiPlayerInteractionManager.HandleVoiceInput(true)
  ↓
ConvaiNPC.StartListening()
```

### Sistema GaudIA (Botón UI)

```
Usuario toca botón UI
  ↓
GaudiVoiceButton.OnPointerDown()
  ↓
GaudiVoiceButton.StartVoiceRecording()
  ↓
_currentActiveNPC.StartListening()
```

**Ventajas**:
- ✅ Funciona en móvil (táctil)
- ✅ Funciona en desktop (mouse)
- ✅ Funciona en AR
- ✅ Feedback visual claro
- ✅ Más directo (menos intermediarios)
- ✅ No depende de ConvaiInputManager (aunque puede coexistir)

## Coexistencia con Sistema Original

Ambos sistemas pueden funcionar simultáneamente:
- **Desktop**: Tecla T + Botón UI (elige el usuario)
- **Mobile/AR**: Solo botón UI

Para desactivar completamente el input de teclado (opcional):
1. En `ConvaiInputManager`:
   - Comentar líneas 169-179 (HandleVoiceInput del teclado)
2. O simplemente ignorar, no interfieren entre sí

## Troubleshooting

### Problema: Botón no responde

**Verificar**:
1. ¿Hay un EventSystem en la escena? (Canvas lo crea automáticamente)
2. ¿El botón tiene el componente `GaudiVoiceButton` añadido?
3. ¿Hay un `ConvaiNPCManager` activo en la escena?
4. ¿Hay al menos un NPC activo (mirándolo)?

**Logs a revisar**:
```
ConvaiLogger: "GaudiVoiceButton: Listening to OnActiveNPCChanged event."
ConvaiLogger: "GaudiVoiceButton: Active NPC changed to [nombre]"
```

### Problema: NPC no rota hacia player

**Verificar**:
1. ¿El NPC tiene `GaudiNPCRotation` componente?
2. ¿`onlyRotateWhileTalking` está marcado pero NPC no está hablando?
3. ¿`playerTransform` es null? (debería auto-detectar)
4. ¿`rotationSpeed` es muy bajo?

**Debug**:
- Activar `showDebugGizmos` y revisar en Scene view
- Verificar que línea verde/amarilla apunta al jugador
- Verificar valor de `IsCurrentlyRotating` en Inspector durante play

### Problema: Rotación demasiado rápida/lenta

**Solución**:
- Aumentar/disminuir `rotationSpeed` (default: 90°/s)
- 45°/s = muy lento, natural
- 90°/s = medio, recomendado
- 180°/s = rápido
- 360°/s = muy rápido, robótico

### Problema: NPC rota constantemente (jitter)

**Solución**:
- Aumentar `rotationThreshold` (default: 5°)
- Si threshold es muy bajo (1-2°), puede causar micro-rotaciones
- Recomendado: 5-10° para movimiento smooth

## Optimizaciones

### Performance

1. **GaudiVoiceButton**:
   - No usa `Update()` (solo eventos)
   - Caché de referencias en `Awake()`
   - Desuscripción limpia en `OnDisable()`

2. **GaudiNPCRotation**:
   - Usa `sqrMagnitude` para checks de distancia (evita `sqrt`)
   - Solo calcula rotación si NPC está activo
   - Threshold evita cálculos innecesarios

### Memoria

- No hay allocations en runtime (todo pre-cached)
- Strings de log solo en Development builds (ConvaiLogger)
- Gizmos solo se dibujan en Editor

## Testing en Móvil

### Build Settings

1. File > Build Settings
2. Platform: Android (o iOS)
3. Add Open Scenes
4. Player Settings:
   - Company Name: [tu nombre]
   - Product Name: GaudIA
   - Minimum API Level: Android 7.0 (API 24)
   - Target API Level: Highest installed

### Testing Workflow

1. **Unity Remote** (testing rápido):
   - No testing de AR, pero sí de UI táctil
   - Edit > Project Settings > Editor > Unity Remote

2. **Build & Run** (testing completo):
   - Conectar dispositivo Android via USB
   - Build and Run
   - Permitir permisos de micrófono cuando se solicite

### Logs en Dispositivo

- Usar ADB Logcat:
  ```bash
  adb logcat -s Unity
  ```
- Buscar líneas de ConvaiLogger con tag "GaudIA"

## Próximos Pasos

Una vez que el sistema UI está funcionando:

1. **Adaptar para AR** (ver `04_AR_Foundation_Integration_Guide.md`)
   - Reemplazar FPS camera por AR camera
   - Añadir plane detection
   - Sistema de placement de NPC

2. **Mejorar UI**:
   - Animaciones de transición
   - Partículas durante grabación
   - Indicador de volumen de micrófono
   - Wave form visual

3. **Añadir Features**:
   - Botón de reposicionamiento de NPC
   - Historial de conversación
   - Preguntas sugeridas
   - Botón de interrupción (stop talking)

## Referencias de Código

### Archivos Modificados
Ninguno - el sistema es completamente aditivo.

### Archivos Nuevos
1. `/Assets/Scripts/GaudiVoiceButton.cs` - Componente de botón de voz
2. `/Assets/Scripts/GaudiNPCRotation.cs` - Componente de rotación de NPC

### Dependencias
- `Convai.Scripts.Runtime.Core.ConvaiNPC`
- `Convai.Scripts.Runtime.Core.ConvaiNPCManager`
- `Convai.Scripts.Runtime.LoggerSystem.ConvaiLogger`
- `UnityEngine.UI.Button`
- `UnityEngine.EventSystems.IPointerDownHandler`
- `UnityEngine.EventSystems.IPointerUpHandler`
- `TMPro.TextMeshProUGUI` (opcional)

## Conclusión

El sistema de input mediante botón UI está completamente implementado y documentado. Proporciona una base sólida para la versión móvil AR del proyecto, manteniendo compatibilidad con el sistema desktop original.

Los dos componentes (`GaudiVoiceButton` y `GaudiNPCRotation`) son modulares, reutilizables, y altamente configurables, siguiendo las mejores prácticas de Unity y C#.
