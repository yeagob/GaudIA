# Guía de Integración de AR Foundation para GaudIA

## Índice
1. [Introducción](#introducción)
2. [Requisitos Previos](#requisitos-previos)
3. [Instalación de AR Foundation](#instalación-de-ar-foundation)
4. [Configuración del Proyecto](#configuración-del-proyecto)
5. [Migración de Escena FPS a AR](#migración-de-escena-fps-a-ar)
6. [Sistema de Detección de Planos](#sistema-de-detección-de-planos)
7. [Sistema de Placement de NPC](#sistema-de-placement-de-npc)
8. [Adaptación de UI para AR](#adaptación-de-ui-para-ar)
9. [Optimizaciones para Móvil](#optimizaciones-para-móvil)
10. [Testing y Debugging](#testing-y-debugging)
11. [Troubleshooting](#troubleshooting)

## Introducción

Esta guía cubre paso a paso cómo integrar AR Foundation en el proyecto GaudIA, transformando la experiencia de FPS desktop en una aplicación móvil de Realidad Aumentada.

### Objetivos de la Integración

- ✅ Reemplazar FPS camera por AR camera
- ✅ Implementar detección de planos horizontales (suelo)
- ✅ Permitir al usuario colocar a Gaudí en el espacio real
- ✅ Mantener toda la funcionalidad de Convai NPC
- ✅ Adaptar UI para experiencia AR móvil

### Arquitectura Final

```
┌─────────────────────────────────────┐
│   AR Session (Tracking espacial)   │
├─────────────────────────────────────┤
│   AR Session Origin                 │
│   ├─ AR Camera (reemplaza FPS cam) │
│   └─ ConvaiInputManager             │
├─────────────────────────────────────┤
│   AR Plane Manager                  │
│   └─ Plane Prefab (visualización)  │
├─────────────────────────────────────┤
│   AR Raycast Manager                │
│   (detección de toques en planos)  │
├─────────────────────────────────────┤
│   Placement Controller (custom)     │
│   └─ Instancia NPC en superficie    │
├─────────────────────────────────────┤
│   Convai NPC con GaudiNPCRotation   │
│   (anclado a AR Anchor)             │
├─────────────────────────────────────┤
│   UI Canvas (Screen Space - Camera) │
│   ├─ GaudiVoiceButton               │
│   ├─ Placement Instructions         │
│   └─ AR Status Indicator            │
└─────────────────────────────────────┘
```

## Requisitos Previos

### Versiones Mínimas Requeridas

- **Unity**: 2021.3 LTS o superior
- **AR Foundation**: 4.2.x o superior
- **ARCore XR Plugin** (Android): 4.2.x o superior
- **ARKit XR Plugin** (iOS): 4.2.x o superior

### Dispositivos Compatibles

#### Android (ARCore)
- Android 7.0 (API level 24) o superior
- Lista completa: [ARCore supported devices](https://developers.google.com/ar/devices)
- Ejemplos: Samsung Galaxy S8+, Google Pixel 2+, OnePlus 5+

#### iOS (ARKit)
- iOS 11.0 o superior
- iPhone 6S o superior
- iPad Pro, iPad (5th gen) o superior

### Permisos Necesarios

**Android** (`AndroidManifest.xml`):
```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-feature android:name="android.hardware.camera.ar" android:required="true"/>
```

**iOS** (`Info.plist`):
```xml
<key>NSCameraUsageDescription</key>
<string>Necesitamos acceso a la cámara para mostrar a Gaudí en realidad aumentada</string>
<key>NSMicrophoneUsageDescription</key>
<string>Necesitamos acceso al micrófono para hablar con Gaudí</string>
```

## Instalación de AR Foundation

### Paso 1: Abrir Package Manager

1. Abrir Unity
2. Window > Package Manager
3. En la esquina superior izquierda, cambiar dropdown a "Unity Registry"

### Paso 2: Instalar Paquetes Core

Instalar en este orden:

1. **AR Foundation**
   - Buscar "AR Foundation"
   - Versión recomendada: 4.2.7 (o la última estable)
   - Click en "Install"

2. **ARCore XR Plugin** (para Android)
   - Buscar "ARCore XR Plugin"
   - Misma versión que AR Foundation
   - Click en "Install"

3. **ARKit XR Plugin** (para iOS)
   - Buscar "ARKit XR Plugin"
   - Misma versión que AR Foundation
   - Click en "Install"

### Paso 3: Verificar Instalación

Debe haber una nueva entrada en el menú:
- GameObject > XR > [varias opciones de AR]

Si aparece, la instalación fue exitosa.

## Configuración del Proyecto

### Configuración de Android

1. **Build Settings**:
   - File > Build Settings
   - Platform: Android
   - Click "Switch Platform"

2. **Player Settings**:
   ```
   Edit > Project Settings > Player > Android

   Other Settings:
   ├─ Graphics APIs: OpenGLES3 (remover Vulkan si está)
   ├─ Minimum API Level: Android 7.0 'Nougat' (API 24)
   ├─ Target API Level: Automatic (highest installed)
   ├─ Scripting Backend: IL2CPP
   └─ Target Architectures: ARM64 ✓, ARMv7 ✗

   Identification:
   └─ Package Name: com.tucompany.gaudia (cambiar "tucompany")
   ```

3. **XR Plug-in Management**:
   ```
   Edit > Project Settings > XR Plug-in Management

   Android tab:
   └─ ARCore ✓ (marcar)
   ```

### Configuración de iOS

1. **Build Settings**:
   - File > Build Settings
   - Platform: iOS
   - Click "Switch Platform"

2. **Player Settings**:
   ```
   Edit > Project Settings > Player > iOS

   Other Settings:
   ├─ Target minimum iOS Version: 11.0
   ├─ Architecture: ARM64
   └─ Camera Usage Description: "AR y conversación con Gaudí"
   └─ Microphone Usage Description: "Hablar con Gaudí"

   Identification:
   └─ Bundle Identifier: com.tucompany.gaudia
   ```

3. **XR Plug-in Management**:
   ```
   Edit > Project Settings > XR Plug-in Management

   iOS tab:
   └─ ARKit ✓ (marcar)
   ```

### Configuración General

1. **Graphics**:
   ```
   Edit > Project Settings > Graphics

   ├─ Scriptable Render Pipeline Settings: None (o URP si usas)
   └─ Tier Settings: Medium como mínimo
   ```

2. **Quality**:
   ```
   Edit > Project Settings > Quality

   Para Android/iOS:
   ├─ Levels: Medium o High
   ├─ VSync Count: Every V Blank (60 FPS)
   └─ Shadow Distance: 30-50 (para mejor rendimiento)
   ```

## Migración de Escena FPS a AR

### Paso 1: Backup de la Escena Original

1. Duplicar la escena actual:
   - Assets > Scenes > [tu escena]
   - Ctrl+D para duplicar
   - Renombrar a "GaudIA_AR"

### Paso 2: Eliminar FPS Controller

En la escena GaudIA_AR:

1. **Localizar el FPS Player**:
   - Generalmente nombrado "Player", "FPSController", etc.
   - Contiene:
     - Character Controller
     - ConvaiPlayerMovement (si existe)
     - Main Camera (child)

2. **Preservar Componentes Necesarios**:
   - `ConvaiInputManager` → Mover al nivel raíz de la jerarquía
   - `ConvaiNPCManager` → Mover al nivel raíz si está en Player
   - Main Camera → NO eliminar todavía

3. **Eliminar el resto del Player**:
   - Seleccionar GameObject del Player
   - Click derecho > Delete

### Paso 3: Configurar AR Session

1. **Crear AR Session**:
   - GameObject > XR > AR Session
   - Debe quedar en la raíz de la jerarquía
   - No necesita configuración adicional

2. **Crear AR Session Origin**:
   - GameObject > XR > AR Session Origin
   - Esto crea automáticamente una "AR Camera" como child
   - Tag "MainCamera" se asigna automáticamente a AR Camera

3. **Configurar AR Camera**:
   ```
   AR Session Origin > AR Camera

   Inspector:
   ├─ Tag: MainCamera ✓
   ├─ Camera Component:
   │  ├─ Clear Flags: Solid Color
   │  ├─ Background: Black (R:0, G:0, B:0, A:0)
   │  └─ Culling Mask: Everything
   └─ AR Camera Manager:
      └─ (configuración por defecto está bien)
   ```

4. **Eliminar Old Main Camera**:
   - Buscar la vieja Main Camera (del FPS)
   - Verificar que AR Camera tiene tag "MainCamera"
   - Eliminar la vieja cámara

### Paso 4: Mover ConvaiInputManager

Si `ConvaiInputManager` estaba en el Player:

1. Crear GameObject vacío en raíz:
   - GameObject > Create Empty
   - Renombrar a "GameManagers"

2. Mover ConvaiInputManager:
   - Drag & drop a GameManagers
   - O crear nuevo: Add Component > ConvaiInputManager

**IMPORTANTE**: ConvaiInputManager debe estar en la escena para que `GaudiVoiceButton` funcione.

### Paso 5: Verificar ConvaiNPCManager

1. Asegurarse de que existe en la escena
2. Si no existe, crear:
   - GameObject > Create Empty
   - Renombrar a "NPCManager"
   - Add Component > ConvaiNPCManager

3. Configurar:
   ```
   Inspector:
   ├─ Ray Length: 5.0 (más largo para AR)
   └─ Vision Cone Angle: 60 (más amplio para AR)
   ```

## Sistema de Detección de Planos

### Paso 1: Añadir AR Plane Manager

1. Seleccionar "AR Session Origin"
2. Add Component > AR Plane Manager
3. Configurar:
   ```
   AR Plane Manager:
   ├─ Plane Prefab: [crear en siguiente paso]
   └─ Detection Mode: Horizontal (solo suelos)
   ```

### Paso 2: Crear Plane Prefab

1. **Crear Prefab de Visualización**:
   - GameObject > 3D Object > Plane
   - Renombrar a "ARPlanePrefab"
   - Escalar: (0.1, 0.1, 0.1) - será escalado automáticamente

2. **Configurar Componentes**:
   ```
   ARPlanePrefab:
   ├─ Transform:
   │  └─ Rotation: (90, 0, 0) - para que quede horizontal
   ├─ Mesh Renderer:
   │  ├─ Material: [crear material semi-transparente]
   │  └─ Lighting: Sin sombras
   └─ AR Plane (añadir):
      └─ (configuración por defecto)
   ```

3. **Crear Material para Plano**:
   - Assets > Create > Material
   - Renombrar a "AR_PlaneMaterial"
   - Configurar:
     ```
     Shader: Universal Render Pipeline/Unlit (o Unlit/Transparent)
     Rendering Mode: Transparent
     Color: Azul claro con alpha bajo (R:0, G:150, B:255, A:50)
     ```

4. **Convertir a Prefab**:
   - Drag ARPlanePrefab de Hierarchy a Assets/Prefabs/
   - Eliminar de la escena
   - Asignar en AR Plane Manager > Plane Prefab

### Paso 3: Añadir AR Raycast Manager

1. Seleccionar "AR Session Origin"
2. Add Component > AR Raycast Manager
3. No requiere configuración adicional

### Paso 4: Testing de Detección

En este punto deberías poder:
1. Build & Run en dispositivo Android/iOS
2. Mover el dispositivo apuntando al suelo
3. Ver planos azules semi-transparentes aparecer

## Sistema de Placement de NPC

### Crear Script de Placement Controller

1. **Crear Script**:
   - Assets > Scripts > Create > C# Script
   - Nombrar: "ARNPCPlacementController"

2. **Código Completo**:

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

namespace GaudIA
{
    /// <summary>
    /// Controla la colocación del NPC en superficies AR detectadas.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARNPCPlacementController : MonoBehaviour
    {
        [Header("NPC Settings")]
        [Tooltip("Prefab del NPC a instanciar (debe tener ConvaiNPC)")]
        [SerializeField] private GameObject npcPrefab;

        [Tooltip("Permitir reubicar el NPC después de colocado")]
        [SerializeField] private bool allowReposition = true;

        [Header("Visual Feedback")]
        [Tooltip("Prefab de indicador de posición (opcional)")]
        [SerializeField] private GameObject placementIndicator;

        [Tooltip("Offset del NPC respecto al punto de toque")]
        [SerializeField] private Vector3 npcOffset = Vector3.zero;

        [Header("UI References")]
        [Tooltip("Texto de instrucciones para el usuario")]
        [SerializeField] private TMPro.TextMeshProUGUI instructionText;

        private ARRaycastManager _arRaycastManager;
        private GameObject _spawnedNPC;
        private GameObject _indicatorInstance;
        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private bool _npcPlaced = false;

        private void Awake()
        {
            _arRaycastManager = GetComponent<ARRaycastManager>();
        }

        private void Start()
        {
            if (placementIndicator != null)
            {
                _indicatorInstance = Instantiate(placementIndicator);
                _indicatorInstance.SetActive(false);
            }

            UpdateInstructionText("Escanea el suelo apuntando con la cámara...");
        }

        private void Update()
        {
            // Si ya hay NPC y no se permite reposicionar, salir
            if (_npcPlaced && !allowReposition)
                return;

            // Detectar toque en pantalla
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // Ignorar si se toca sobre UI
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;

                // Solo procesar al levantar el dedo
                if (touch.phase == TouchPhase.Ended)
                {
                    if (TryPlaceNPC(touch.position))
                    {
                        if (!_npcPlaced)
                        {
                            _npcPlaced = true;
                            UpdateInstructionText("Gaudí colocado. Acércate para hablar.");
                        }
                        else if (allowReposition)
                        {
                            UpdateInstructionText("Gaudí reposicionado.");
                        }
                    }
                }
            }

            // Actualizar indicador de posición
            UpdatePlacementIndicator();
        }

        private void UpdatePlacementIndicator()
        {
            if (_indicatorInstance == null || _npcPlaced)
            {
                if (_indicatorInstance != null)
                    _indicatorInstance.SetActive(false);
                return;
            }

            // Raycast desde el centro de la pantalla
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            if (_arRaycastManager.Raycast(screenCenter, _hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = _hits[0].pose;
                _indicatorInstance.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                _indicatorInstance.SetActive(true);

                if (!_npcPlaced)
                {
                    UpdateInstructionText("Toca la pantalla para colocar a Gaudí");
                }
            }
            else
            {
                _indicatorInstance.SetActive(false);

                if (!_npcPlaced)
                {
                    UpdateInstructionText("Escanea el suelo apuntando con la cámara...");
                }
            }
        }

        private bool TryPlaceNPC(Vector2 touchPosition)
        {
            if (_arRaycastManager.Raycast(touchPosition, _hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = _hits[0].pose;

                if (_spawnedNPC == null)
                {
                    // Primera vez: instanciar NPC
                    _spawnedNPC = Instantiate(npcPrefab, hitPose.position + npcOffset, hitPose.rotation);

                    // Opcional: crear anchor para estabilidad
                    ARRaycastHit hit = _hits[0];
                    if (hit.trackable is ARPlane plane)
                    {
                        var anchor = plane.GetComponent<ARAnchorManager>()?.AttachAnchor(plane, hitPose);
                        if (anchor != null)
                        {
                            _spawnedNPC.transform.SetParent(anchor.transform);
                        }
                    }

                    Debug.Log($"NPC instanciado en {hitPose.position}");
                }
                else if (allowReposition)
                {
                    // Reposicionar NPC existente
                    _spawnedNPC.transform.SetPositionAndRotation(hitPose.position + npcOffset, hitPose.rotation);
                    Debug.Log($"NPC reposicionado a {hitPose.position}");
                }

                return true;
            }

            return false;
        }

        private void UpdateInstructionText(string message)
        {
            if (instructionText != null)
            {
                instructionText.text = message;
            }
        }

        /// <summary>
        /// Remueve el NPC de la escena
        /// </summary>
        public void RemoveNPC()
        {
            if (_spawnedNPC != null)
            {
                Destroy(_spawnedNPC);
                _spawnedNPC = null;
                _npcPlaced = false;
                UpdateInstructionText("Escanea el suelo para colocar a Gaudí...");
            }
        }

        /// <summary>
        /// Retorna si hay un NPC colocado
        /// </summary>
        public bool IsNPCPlaced => _npcPlaced;

        /// <summary>
        /// Retorna el NPC instanciado
        /// </summary>
        public GameObject SpawnedNPC => _spawnedNPC;
    }
}
```

### Configurar Placement Controller en Escena

1. **Seleccionar AR Session Origin**
2. **Add Component** > ARNPCPlacementController
3. **Configurar**:
   ```
   ARNPCPlacementController:
   ├─ NPC Settings
   │  ├─ NPC Prefab: [arrastrar prefab del NPC con ConvaiNPC]
   │  └─ Allow Reposition: ✓
   ├─ Visual Feedback
   │  ├─ Placement Indicator: [crear en siguiente paso]
   │  └─ NPC Offset: (0, 0, 0)
   └─ UI References
      └─ Instruction Text: [arrastrar Text UI]
   ```

### Crear Placement Indicator

1. **Crear GameObject**:
   - GameObject > Create Empty
   - Renombrar a "PlacementIndicator"

2. **Añadir Visual**:
   - Agregar Child: GameObject > 3D Object > Cylinder
   - Escalar: (0.5, 0.01, 0.5) - disco plano
   - Rotation: (0, 0, 0)

3. **Material**:
   - Crear material "PlacementIndicator_Mat"
   - Color: Verde brillante con alpha (R:0, G:255, B:0, A:180)
   - Shader: Unlit/Transparent

4. **Animación Opcional**:
   - Añadir componente que haga rotar o pulsar
   - O usar Animation simple

5. **Convertir a Prefab**:
   - Drag a Assets/Prefabs/
   - Eliminar de escena
   - Asignar en ARNPCPlacementController

### Crear NPC Prefab

1. **Preparar NPC en Escena**:
   - Arrastrar tu modelo de Gaudí a la escena
   - Añadir componentes:
     - ConvaiNPC (configurado con Character ID)
     - Animator
     - AudioSource
     - GaudiNPCRotation
     - Cualquier otro componente necesario

2. **Convertir a Prefab**:
   - Drag a Assets/Prefabs/
   - Nombrar "GaudiNPC_Prefab"
   - Eliminar de escena

3. **Asignar en Placement Controller**:
   - ARNPCPlacementController > NPC Prefab

## Adaptación de UI para AR

### Configuración de Canvas

El Canvas debe adaptarse a la experiencia AR:

1. **Seleccionar Canvas**
2. **Configurar**:
   ```
   Canvas:
   ├─ Render Mode: Screen Space - Camera
   ├─ Render Camera: [arrastrar AR Camera]
   ├─ Plane Distance: 1.0
   └─ Canvas Scaler:
      ├─ UI Scale Mode: Scale With Screen Size
      ├─ Reference Resolution: 1080x1920 (portrait)
      └─ Match: 0.5 (balance width/height)
   ```

### Layout de UI AR

```
Canvas (Screen Space - Camera)
├─ InstructionPanel (Top)
│  └─ InstructionText (TMPro)
├─ VoiceButton (Bottom Center)
│  ├─ GaudiVoiceButton
│  └─ StatusText
├─ RepositionButton (Top Right - opcional)
└─ DebugPanel (Development only)
```

### Crear Instruction Panel

1. **Panel Superior**:
   ```
   GameObject > UI > Panel
   Renombrar: InstructionPanel

   Rect Transform:
   ├─ Anchor: Top-Stretch
   ├─ Pos Y: -100
   └─ Height: 150

   Image:
   ├─ Color: Negro con alpha 150
   ```

2. **Texto de Instrucciones**:
   ```
   Add child: UI > Text - TextMeshPro
   Renombrar: InstructionText

   TMP_Text:
   ├─ Text: "Escanea el suelo..."
   ├─ Font Size: 28
   ├─ Alignment: Center
   ├─ Color: Blanco
   └─ Auto Size: Sí
   ```

3. **Asignar en ARNPCPlacementController**:
   - Instruction Text: arrastrar InstructionText

### Posicionar Voice Button

```
VoiceButton:
Rect Transform:
├─ Anchor: Bottom-Center
├─ Pos Y: 150
├─ Width: 200
└─ Height: 200
```

### Botón de Reposicionamiento (Opcional)

```csharp
// Crear script simple para botón
public class RepositionButton : MonoBehaviour
{
    [SerializeField] private ARNPCPlacementController placementController;

    public void OnRepositionClick()
    {
        if (placementController != null && placementController.IsNPCPlaced)
        {
            placementController.RemoveNPC();
        }
    }
}
```

Añadir en UI:
```
GameObject > UI > Button
Renombrar: RepositionButton
Posición: Top-Right
On Click(): RepositionButton.OnRepositionClick()
```

## Optimizaciones para Móvil

### Graphics y Rendering

1. **Reducir Complejidad Visual**:
   ```
   Edit > Project Settings > Quality

   Mobile:
   ├─ Pixel Light Count: 1
   ├─ Texture Quality: Half Res
   ├─ Anisotropic Textures: Disabled
   ├─ Soft Particles: No
   ├─ Shadow Distance: 20
   └─ Shadow Cascades: No Cascades
   ```

2. **LODs en Modelo de Gaudí**:
   - LOD0: Modelo completo (0-10m)
   - LOD1: Reducido 50% (10-20m)
   - LOD2: Simplificado (20m+)

3. **Bake Lighting**:
   - Window > Rendering > Lighting
   - Marcar objetos estáticos
   - Generate Lighting

### Audio

1. **Comprimir Audio de Convai**:
   ```
   Audio Settings:
   ├─ Compression Format: Vorbis
   ├─ Quality: 70 (balance calidad/tamaño)
   └─ Load Type: Streaming (para clips largos)
   ```

### Scripts

1. **Reducir Update Frequency**:
   ```csharp
   // En GaudiNPCRotation, añadir throttling
   private float _updateInterval = 0.1f; // 10 FPS para rotación
   private float _lastUpdateTime;

   void Update()
   {
       if (Time.time - _lastUpdateTime < _updateInterval)
           return;

       _lastUpdateTime = Time.time;
       // ... resto del código
   }
   ```

### Build Size

1. **Reducir APK**:
   ```
   Build Settings:
   ├─ Compression Method: LZ4 (más rápido) o LZ4HC (más pequeño)
   └─ Split Application Binary: Sí (para APKs grandes)

   Player Settings:
   ├─ Strip Engine Code: Sí
   ├─ Managed Stripping Level: High
   └─ Optimize Mesh Data: Sí
   ```

## Testing y Debugging

### Testing en Editor (Simulación AR)

Unity no puede simular AR completamente, pero podemos aproximar:

1. **AR Simulation** (requiere paquete adicional):
   ```
   Package Manager > AR Foundation
   Samples > Import "AR Foundation Samples"
   ```

2. **Usar Unity Remote**:
   - Conectar dispositivo via USB
   - Edit > Project Settings > Editor > Unity Remote
   - Device: seleccionar tu dispositivo

### Testing en Dispositivo Real

1. **Build & Run**:
   ```
   File > Build Settings
   ├─ Escena: GaudIA_AR ✓
   ├─ Platform: Android/iOS
   └─ Build and Run
   ```

2. **Verificar Permisos**:
   - Primera ejecución: aceptar cámara y micrófono
   - Si se niegan, app no funcionará

### Logs en Dispositivo

#### Android (ADB Logcat)

```bash
# Terminal/CMD
adb logcat -s Unity

# Filtrar por errores AR
adb logcat -s Unity | grep "AR"

# Filtrar por logs de GaudIA
adb logcat -s Unity | grep "Gaudi"
```

#### iOS (Xcode)

```
1. Build en Unity (sin Run)
2. Abrir proyecto .xcodeproj en Xcode
3. Window > Devices and Simulators
4. Seleccionar dispositivo
5. View Device Logs
```

### Debug Visual en AR

Crear script para mostrar info en pantalla:

```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDebugDisplay : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI debugText;
    private ARSession _arSession;
    private ARPlaneManager _planeManager;

    void Start()
    {
        _arSession = FindObjectOfType<ARSession>();
        _planeManager = FindObjectOfType<ARPlaneManager>();
    }

    void Update()
    {
        if (debugText == null) return;

        string info = $"AR State: {ARSession.state}\n";
        info += $"Planos detectados: {_planeManager?.trackables.count ?? 0}\n";
        info += $"FPS: {1f / Time.deltaTime:F0}\n";

        debugText.text = info;
    }
}
```

## Troubleshooting

### Problema: "ARSession state is None/Unsupported"

**Causas**:
- ARCore/ARKit no instalado en dispositivo
- Permisos de cámara denegados
- Dispositivo no compatible

**Soluciones**:
1. Verificar dispositivo en lista de compatibilidad
2. Reinstalar app y aceptar permisos
3. Verificar que ARCore XR Plugin esté activo en Project Settings

### Problema: No se detectan planos

**Causas**:
- Poca iluminación
- Superficies sin textura (ej: pared blanca lisa)
- Movimiento muy rápido de la cámara

**Soluciones**:
1. Mover dispositivo lentamente en patrón circular
2. Apuntar a superficies con textura
3. Mejorar iluminación del ambiente
4. Aumentar Detection Mode a "Everything" temporalmente para testing

### Problema: NPC no aparece al tocar

**Verificar**:
1. ¿ARNPCPlacementController está en AR Session Origin?
2. ¿NPC Prefab está asignado?
3. ¿Texto de instrucciones dice "Toca para colocar"?
4. ¿Se toca sobre UI por error? (EventSystem check)

**Debug**:
```csharp
// Añadir logs en ARNPCPlacementController.TryPlaceNPC()
Debug.Log($"Touch detected at {touchPosition}");
Debug.Log($"Raycast hits: {_hits.Count}");
```

### Problema: Convai NPC no responde en AR

**Causas**:
- NPC prefab no tiene todos los componentes
- ConvaiNPCManager no detecta el NPC
- Micrófono no accesible

**Verificar**:
1. NPC Prefab tiene:
   - ConvaiNPC (con Character ID)
   - Animator
   - AudioSource
2. ConvaiNPCManager está en escena
3. Permisos de micrófono aceptados
4. UI Button tiene GaudiVoiceButton

### Problema: Rotación del NPC no funciona

**Verificar**:
1. NPC tiene GaudiNPCRotation component
2. Player Transform está asignado (o auto-detecta AR Camera)
3. "Only Rotate While Talking" está configurado correctamente
4. NPC está activo (ConvaiNPCManager lo detecta)

### Problema: Performance bajo en móvil

**Soluciones**:
1. Reducir Quality settings (ver Optimizaciones)
2. Verificar que no hay múltiples cámaras activas
3. Desactivar post-processing si existe
4. Reducir sombras o desactivarlas
5. Usar LODs en modelo de NPC

## Próximos Pasos

Una vez que AR Foundation está integrado y funcionando:

1. **Mejorar Experiencia**:
   - Añadir efectos de aparición del NPC (fade in, partículas)
   - Sonido ambiental reactivo a la conversación
   - Gestos del NPC más expresivos

2. **Features Adicionales**:
   - Múltiples ubicaciones de NPC (guardar/cargar)
   - Compartir experiencia entre usuarios (multiplayer)
   - Screenshots con NPC en AR

3. **Pulido**:
   - Tutorial interactivo first-time
   - Feedback háptico (vibración) al colocar NPC
   - Transiciones de UI suaves

4. **Testing Extensivo**:
   - Diferentes dispositivos
   - Diferentes condiciones de luz
   - Diferentes superficies

5. **Preparar para Distribución**:
   - Implementar analytics
   - Sistema de feedback de usuarios
   - Preparar assets para stores

## Recursos Adicionales

- [AR Foundation Documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest)
- [ARCore Developer Guide](https://developers.google.com/ar)
- [ARKit Developer Guide](https://developer.apple.com/augmented-reality/)
- [Unity Mobile Optimization](https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html)

## Conclusión

La integración de AR Foundation transforma completamente la experiencia de GaudIA, llevándola de un prototipo desktop a una aplicación móvil inmersiva. Siguiendo esta guía paso a paso, tendrás una base sólida para construir y expandir la experiencia AR.

Recuerda que AR es una tecnología que depende fuertemente del hardware y condiciones ambientales. El testing en dispositivos reales es crucial, y la experiencia debe diseñarse siendo tolerante a diferentes calidades de tracking y detección.
