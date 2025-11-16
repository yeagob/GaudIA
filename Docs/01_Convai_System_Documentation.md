# Documentación del Sistema Convai NPC AI

## Introducción

Convai es un sistema de Inteligencia Artificial para crear NPCs (Non-Player Characters) conversacionales en Unity. Este asset permite que los personajes virtuales mantengan conversaciones naturales mediante voz y texto, con soporte para sincronización labial, acciones, y gestión de memoria a largo plazo.

## Arquitectura del Sistema

### Componentes Principales

#### 1. ConvaiNPC (Core)
**Ubicación**: `Assets/Convai/Scripts/Runtime/Core/ConvaiNPC.cs`

El componente central que gestiona toda la interacción con la API de Convai. Cada NPC en la escena debe tener este componente.

**Responsabilidades**:
- Gestión de la comunicación gRPC con el servidor de Convai
- Control del estado de activación del personaje
- Procesamiento de respuestas de audio y texto
- Coordinación de componentes auxiliares (lip sync, acciones, etc.)
- Gestión de la sesión de conversación

**Propiedades Clave**:
- `characterName`: Nombre del personaje
- `characterID`: ID único del personaje en la plataforma Convai
- `sessionID`: ID de sesión actual de la conversación
- `isCharacterActive`: Indica si el personaje está actualmente activo
- `isCharacterTalking`: Indica si el personaje está hablando

**Métodos Principales**:
- `StartListening()`: Inicia la grabación de audio del micrófono
- `StopListening()`: Detiene la grabación de audio
- `SendTextDataAsync(string text)`: Envía un mensaje de texto al NPC
- `InterruptCharacterSpeech()`: Interrumpe el habla del personaje
- `TriggerEvent(string triggerName)`: Dispara un evento narrativo
- `TriggerSpeech(string triggerMessage)`: Hace que el NPC diga algo específico

#### 2. ConvaiNPCManager (Gestión de NPCs)
**Ubicación**: `Assets/Convai/Scripts/Runtime/Core/ConvaiNPCManager.cs`

Singleton que gestiona qué NPC está actualmente activo según la mirada del jugador.

**Funcionamiento**:
- Usa raycast desde la cámara principal para detectar NPCs
- Define un cono de visión mediante `visionConeAngle` (45° por defecto)
- Activa/desactiva NPCs según el foco del jugador
- Notifica cambios mediante el evento `OnActiveNPCChanged`

**Propiedades**:
- `rayLength`: Distancia del raycast (2.0 metros por defecto)
- `visionConeAngle`: Ángulo del cono de visión
- `activeConvaiNPC`: Referencia al NPC actualmente activo
- `nearbyNPC`: Referencia al NPC cercano al jugador

#### 3. ConvaiInputManager (Gestión de Input)
**Ubicación**: `Assets/Convai/Scripts/Runtime/Core/ConvaiInputManager.cs`

Singleton que centraliza toda la gestión de entrada del jugador. Soporta tanto el nuevo Input System de Unity como el sistema legacy.

**Sistema de Input Dual**:

**a) Nuevo Input System (ENABLE_INPUT_SYSTEM)**:
- Usa el archivo `Controls.inputactions`
- Implementa la interfaz `Controls.IPlayerActions`
- Proporciona callbacks para todas las acciones

**b) Legacy Input Manager (ENABLE_LEGACY_INPUT_MANAGER)**:
- `TalkKey = KeyCode.T`: Tecla para hablar por voz
- `TextSendKey = KeyCode.Return`: Enviar mensaje de texto
- `OpenSettingPanelKey = KeyCode.F10`: Abrir panel de configuración

**Eventos Principales**:
- `talkKeyInteract`: Action<bool> que notifica inicio/fin de input de voz
- `sendText`: Action para enviar texto
- `toggleChat`: Action para abrir/cerrar chat
- `jumping`, `toggleSettings`: Otras acciones del jugador

**Propiedad Importante**:
- `IsTalkKeyHeld`: Bool que indica si la tecla/botón de habla está presionado

#### 4. ConvaiPlayerInteractionManager
**Ubicación**: `Assets/Convai/Scripts/Runtime/Core/ConvaiPlayerInteractionManager.cs`

Intermediario entre el input del jugador y las acciones del NPC. Se añade automáticamente a cada ConvaiNPC.

**Responsabilidades**:
- Escuchar eventos de `ConvaiInputManager`
- Procesar entrada de voz: `HandleVoiceInput(bool listenState)`
- Procesar entrada de texto: `HandleSendText()`
- Gestionar toggle del chat: `HandleToggleChat()`
- Actualizar configuración de acciones según el objeto de atención

**Flujo de Input de Voz**:
```
Usuario presiona T
  ↓
ConvaiInputManager.talkKeyInteract(true)
  ↓
ConvaiPlayerInteractionManager.HandleVoiceInput(true)
  ↓
ConvaiNPC.InterruptCharacterSpeech() + ConvaiNPC.StartListening()
  ↓
Usuario suelta T
  ↓
ConvaiInputManager.talkKeyInteract(false)
  ↓
ConvaiPlayerInteractionManager.HandleVoiceInput(false)
  ↓
ConvaiNPC.StopListening()
```

#### 5. ConvaiTalkButtonHandler (UI Button)
**Ubicación**: `Assets/Convai/Scripts/Runtime/UI/TalkButton/ConvaiTalkButtonHandler.cs`

Componente alternativo para input mediante botón de UI. Extiende `UnityEngine.UI.Button`.

**Funcionamiento**:
- Se suscribe a `ConvaiNPCManager.OnActiveNPCChanged`
- Detecta eventos `OnPointerDown` y `OnPointerUp`
- Llama directamente a `StartListening()` y `StopListening()` del NPC activo
- Proporciona feedback visual (cambio de escala y alpha)

**Ventajas**:
- Funciona en dispositivos móviles
- Proporciona feedback visual claro
- No depende del sistema de input del teclado

#### 6. ConvaiGRPCAPI (Comunicación con Servidor)
**Ubicación**: `Assets/Convai/Scripts/Runtime/Core/ConvaiGRPCAPI.cs`

Singleton que gestiona toda la comunicación gRPC con el servidor de Convai.

**Responsabilidades**:
- Grabación y streaming de audio desde el micrófono
- Envío de datos de texto y audio al servidor
- Recepción y encolado de respuestas
- Gestión de interrupciones de habla
- Configuración de acciones y lip sync

**Endpoint**: `stream.convai.com`

#### 7. ConvaiNPCAudioManager
**Ubicación**: `Assets/Convai/Scripts/Runtime/Core/ConvaiNPCAudioManager.cs`

Gestiona la reproducción de audio del NPC, incluyendo el buffering y secuenciación de clips.

**Funcionalidades**:
- Cola de reproducción de audio
- Conversión de bytes a AudioClip
- Eventos de inicio/fin de habla
- Notificación de transcripciones disponibles

#### 8. ConvaiLipSync (Sincronización Labial)
**Ubicación**: `Assets/Convai/Scripts/Runtime/Features/LipSync/ConvaiLipSync.cs`

Maneja la sincronización labial mediante blend shapes o visemas.

**Tipos Soportados**:
- `ConvaiBlendShapeLipSync`: Usa ARKit blend shapes
- `ConvaiVisemesLipSync`: Usa visemas estándar

#### 9. ConvaiActionsHandler (Sistema de Acciones)
**Ubicación**: `Assets/Convai/Scripts/Runtime/Features/Actions/ConvaiActionsHandler.cs`

Permite que el NPC ejecute acciones sobre objetos del mundo (abrir puertas, recoger objetos, etc.).

#### 10. NarrativeDesignManager (Diseño Narrativo)
**Ubicación**: `Assets/Convai/Scripts/Runtime/Features/NarrativeDesign/Runtime/NarrativeDesignManager.cs`

Gestiona secciones narrativas, triggers y eventos para crear experiencias guiadas.

## Flujo de Comunicación Completo

### Conversación por Voz

```
1. Usuario mira al NPC
   ↓
2. ConvaiNPCManager detecta NPC mediante raycast
   ↓
3. ConvaiNPCManager.UpdateActiveNPC(npc)
   ↓
4. npc.isCharacterActive = true
   ↓
5. UI muestra indicador de NPC activo
   ↓
6. Usuario presiona T (o botón UI)
   ↓
7. ConvaiInputManager.talkKeyInteract(true)
   ↓
8. ConvaiPlayerInteractionManager.HandleVoiceInput(true)
   ↓
9. ConvaiNPC.StartListening()
   ↓
10. ConvaiGRPCAPI.StartRecordAudio()
    ↓
11. Grabación de micrófono y streaming a servidor
    ↓
12. Usuario suelta T
    ↓
13. ConvaiInputManager.talkKeyInteract(false)
    ↓
14. ConvaiNPC.StopListening()
    ↓
15. ConvaiGRPCAPI.StopRecordAudio()
    ↓
16. Servidor procesa audio y genera respuesta
    ↓
17. ConvaiGRPCAPI recibe GetResponseResponse
    ↓
18. ConvaiNPC.EnqueueResponse()
    ↓
19. ConvaiNPC.ProcessResponse() - Loop continuo
    ↓
20. Buffering de audio samples
    ↓
21. ConvaiNPCAudioManager.AddResponseAudio()
    ↓
22. Reproducción de AudioClip
    ↓
23. ConvaiLipSync procesa frames de sincronización labial
    ↓
24. Animator.SetBool("Talk", true)
    ↓
25. NPC habla y se anima
    ↓
26. Fin de audio
    ↓
27. Animator.SetBool("Talk", false)
```

### Conversación por Texto

```
1. Usuario presiona Enter con chat enfocado
   ↓
2. ConvaiInputManager.sendText()
   ↓
3. ConvaiPlayerInteractionManager.HandleSendText()
   ↓
4. ConvaiNPC.SendTextDataAsync(text)
   ↓
5. ConvaiGRPCAPI.SendTextData()
   ↓
6. Servidor procesa y responde
   ↓
7. Flujo de recepción similar a voz (pasos 17-27)
```

## Sistema de Buffering de Audio

El sistema usa un buffer inteligente para optimizar la reproducción de audio:

**Constantes**:
- `SAMPLE_BUFFER_SIZE = 44100` (1 segundo de audio a 44.1kHz)
- `BUFFER_TIMEOUT = 1.5f` segundos

**Lógica de Buffering**:
- Acumula samples recibidos del servidor
- Crea AudioClip cuando:
  - Buffer alcanza 3x `SAMPLE_BUFFER_SIZE` (3 segundos), O
  - Han pasado 1.5 segundos sin recibir nuevos datos
- Combina transcripciones parciales en una sola

**Ventajas**:
- Reduce fragmentación de audio
- Mejora sincronización labial
- Optimiza uso de memoria

## Estructura de Directorios del Asset

```
Assets/Convai/
├── Art/                          # Recursos visuales (sprites, icons, UI)
├── Demo/                         # Escenas de demostración
│   ├── Scenes/
│   │   ├── Full Features/       # Escena demo completa
│   │   ├── Mobile/              # Demo optimizada para móvil
│   │   └── Features/            # Demos de características específicas
│   └── Scripts/                 # Scripts específicos de demos
├── Plugins/                      # Librerías nativas (gRPC, etc.)
├── Prefabs/                      # Prefabs de NPCs y UI
├── Resources/                    # Recursos cargables dinámicamente
│   └── Controls.inputactions    # Configuración del Input System
├── Scene Templates/              # Templates para nuevas escenas
├── Scripts/
│   ├── Editor/                  # Scripts de editor (inspectors, windows)
│   └── Runtime/
│       ├── Addons/              # Sistemas auxiliares (notificaciones, player)
│       ├── Core/                # Scripts fundamentales (NPC, GRPC, Input)
│       ├── Features/            # Características opcionales
│       │   ├── Actions/         # Sistema de acciones
│       │   ├── LipSync/         # Sincronización labial
│       │   ├── LongTermMemory/  # Memoria a largo plazo
│       │   ├── NPC2NPC/         # Conversaciones entre NPCs
│       │   └── NarrativeDesign/ # Diseño narrativo
│       └── UI/                  # Componentes de interfaz
└── Tutorials/                    # Tutoriales integrados
```

## Configuración de un NPC

### Requisitos Mínimos

1. **GameObject con**:
   - `Animator`: Para animaciones de habla
   - `AudioSource`: Para reproducción de voz
   - `ConvaiNPC`: Componente principal

2. **Configuración en ConvaiNPC**:
   - Character Name
   - Character ID (de la plataforma Convai)

3. **En la Escena**:
   - `ConvaiNPCManager`: Para gestión de NPCs activos
   - `ConvaiInputManager`: Para input del jugador
   - `ConvaiGRPCAPI`: Para comunicación con servidor
   - Camera con tag "MainCamera"

### Componentes Opcionales

- `ConvaiLipSync`: Sincronización labial
- `ConvaiActionsHandler`: Sistema de acciones
- `ConvaiHeadTracking`: Seguimiento de cabeza
- `ConvaiBlinkingHandler`: Parpadeo de ojos
- `ConvaiGroupNPCController`: Para conversaciones NPC-NPC
- `NarrativeDesignManager`: Control narrativo
- `ConvaiLTMController`: Memoria a largo plazo

## API Key y Configuración

El sistema requiere una API Key de Convai que se configura en:
- `Window > Convai > Setup`

La API Key se almacena en un ScriptableObject de tipo `ConvaiPlayerDataSO`.

## Características Avanzadas

### 1. Memoria a Largo Plazo (LTM)
Permite que los NPCs recuerden conversaciones previas entre sesiones.

### 2. Conversaciones NPC-NPC
Los NPCs pueden conversar entre ellos sin intervención del jugador.

### 3. Sistema de Acciones
NPCs pueden interactuar con objetos marcados como `ConvaiInteractablesData`.

### 4. Diseño Narrativo
Sistema de triggers y secciones para crear experiencias narrativas lineales o ramificadas.

### 5. Notificaciones
Sistema de notificaciones para advertir sobre problemas (sin micrófono, sin conexión, etc.).

## Optimizaciones

### Mobile
- Demo específica con UI adaptada
- Configuración de calidad de audio ajustada
- Rate limiting de requests

### Performance
- Cache de componentes en `ConvaiNPCManager`
- Pool de RaycastHits
- Buffering inteligente de audio
- Coroutines para procesamiento asíncrono

## Debugging

### Logger System
**Ubicación**: `Assets/Convai/Scripts/Runtime/LoggerSystem/`

Categorías de log:
- `Character`: Operaciones del NPC
- `LipSync`: Sincronización labial
- `Actions`: Sistema de acciones
- `UI`: Interfaz de usuario
- Etc.

Configuración en `Window > Convai > Setup > Logger Settings`

### Gizmos
`ConvaiNPCManager` dibuja gizmos en Scene view:
- Rayo azul: Dirección de la cámara
- Rayo amarillo: Límites del cono de visión
- Arco: Área de detección activa

## Limitaciones Conocidas

1. **Input Keyboard-Centric**: El sistema por defecto está orientado a teclado (tecla T)
2. **Single Active NPC**: Solo un NPC puede estar activo a la vez (excepto modo NPC2NPC)
3. **Raycast-Based Detection**: La detección de NPCs depende de raycast directo
4. **Microphone Required**: Para input de voz se requiere micrófono
5. **Internet Connection**: Requiere conexión estable con `stream.convai.com`

## Referencias Externas

- [Convai Asset Store](https://assetstore.unity.com/packages/tools/behavior-ai/npc-ai-engine-dialog-actions-voice-and-lipsync-convai-235621)
- [Documentación Oficial Convai](https://docs.convai.com/)
- Documentación PDF incluida: `Assets/Convai/Unity SDK Documentation.pdf`

## Conclusión

El sistema Convai NPC AI es una solución completa y robusta para crear NPCs conversacionales. Su arquitectura modular permite gran flexibilidad mientras mantiene la facilidad de uso. Los componentes están bien separados siguiendo principios SOLID, facilitando extensiones y customizaciones.
