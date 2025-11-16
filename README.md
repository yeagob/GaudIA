# GaudIA - Conversaciones AR con Antonio Gaudí

![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS-green)
![Status](https://img.shields.io/badge/Status-Prototype-yellow)

## Descripción

GaudIA es una aplicación de Realidad Aumentada que permite a los visitantes de museos conversar con una representación virtual de Antonio Gaudí mediante Inteligencia Artificial conversacional. Los usuarios pueden colocar a Gaudí en cualquier espacio mediante AR y mantener conversaciones naturales por voz sobre su vida, obra y filosofía arquitectónica.

## Características

- 🎙️ **Conversación por Voz**: Habla naturalmente con Gaudí usando IA conversacional (Convai)
- 📱 **Realidad Aumentada**: Coloca a Gaudí en cualquier superficie plana del museo
- 🗣️ **Sincronización Labial**: Animación facial realista durante las respuestas
- 🔄 **Rotación Inteligente**: El NPC se gira hacia ti durante la conversación
- 🎨 **Interfaz Mobile-First**: UI optimizada para dispositivos táctiles
- 📚 **Educativo**: Contenido histórico y arquitectónico preciso

## Tecnologías

- **Unity 2021.3 LTS**
- **AR Foundation 4.2+**
- **Convai NPC AI SDK**
- **ARCore (Android) / ARKit (iOS)**
- **TextMeshPro**
- **New Input System**

## Estructura del Proyecto

```
GaudIA/
├── Assets/
│   ├── Convai/              # Asset de Convai SDK
│   ├── Scripts/             # Scripts custom del proyecto
│   │   ├── GaudiVoiceButton.cs
│   │   ├── GaudiNPCRotation.cs
│   │   └── ARNPCPlacementController.cs (futuro)
│   ├── Scenes/
│   │   └── GaudIA_AR.unity
│   └── Prefabs/
│       └── GaudiNPC_Prefab.prefab
├── Docs/                    # Documentación completa
│   ├── 01_Convai_System_Documentation.md
│   ├── 02_Project_Vision_And_Objectives.md
│   ├── 03_UI_Button_Input_Implementation.md
│   ├── 04_AR_Foundation_Integration_Guide.md
│   └── 05_NPC_Rotation_System.md
└── README.md
```

## Documentación

### 📖 Guías Disponibles

1. **[Documentación del Sistema Convai](Docs/01_Convai_System_Documentation.md)**
   - Arquitectura del sistema
   - Componentes principales
   - Flujos de comunicación
   - API y configuración

2. **[Visión y Objetivos del Proyecto](Docs/02_Project_Vision_And_Objectives.md)**
   - Concepto y objetivos
   - Roadmap de desarrollo
   - Casos de uso en museos
   - Métricas de éxito

3. **[Implementación de Input UI](Docs/03_UI_Button_Input_Implementation.md)**
   - Sistema de botón de voz
   - Migración de tecla T a UI
   - Configuración paso a paso
   - Troubleshooting

4. **[Guía de Integración AR Foundation](Docs/04_AR_Foundation_Integration_Guide.md)**
   - Instalación de AR Foundation
   - Configuración de proyecto
   - Sistema de detección de planos
   - Placement de NPC
   - Optimizaciones móvil

5. **[Sistema de Rotación del NPC](Docs/05_NPC_Rotation_System.md)**
   - Algoritmo de rotación suave
   - Configuración y parámetros
   - Casos de uso avanzados
   - Performance y optimizaciones

## Inicio Rápido

### Requisitos Previos

- Unity 2021.3 LTS o superior
- Cuenta en [Convai](https://www.convai.com/)
- Dispositivo Android (ARCore) o iOS (ARKit) para testing AR

### Configuración Inicial

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/tuusuario/GaudIA.git
   cd GaudIA
   ```

2. **Abrir en Unity**:
   - Unity Hub > Add > Seleccionar carpeta GaudIA
   - Abrir con Unity 2021.3 LTS

3. **Configurar API Key de Convai**:
   - Window > Convai > Setup
   - Ingresar tu API Key
   - Guardar

4. **Verificar NPC**:
   - Prefabs > GaudiNPC_Prefab
   - Inspector > ConvaiNPC > Character ID
   - Asegurarse de que coincide con tu personaje en Convai

### Testing en Desktop (Prototipo)

1. Abrir escena: `Scenes/Convai Demo - All Features`
2. Play
3. Mirar al NPC
4. Presionar T o usar botón UI para hablar
5. Hablar al micrófono
6. Soltar T para que procese

### Building para Android

1. File > Build Settings
2. Platform: Android > Switch Platform
3. Add Open Scenes
4. Player Settings:
   - Minimum API Level: Android 7.0
   - Package Name: com.tucompany.gaudia
5. Build and Run

## Componentes Principales

### GaudiVoiceButton

Botón UI que reemplaza el input de teclado (tecla T) para dispositivos móviles.

**Características**:
- Feedback visual (color, escala)
- Feedback auditivo (opcional)
- Texto de estado dinámico
- Funciona con toque táctil y mouse

**Uso**:
```csharp
// Añadir a cualquier UI Button
button.AddComponent<GaudiVoiceButton>();
```

### GaudiNPCRotation

Hace que el NPC se gire suavemente hacia el jugador durante conversaciones.

**Características**:
- Rotación smooth configurable
- Threshold para evitar jitter
- Opción de rotar solo al hablar
- Lock de eje Y (horizontal only)

**Uso**:
```csharp
// Añadir al GameObject del NPC
npc.AddComponent<GaudiNPCRotation>();
```

### ARNPCPlacementController (Futuro)

Controlador para colocar el NPC en superficies AR detectadas.

**Estado**: Documentado en guía AR, pendiente de implementación

## Roadmap

### ✅ Fase 1: Prototipo Funcional (Completado)
- [x] Configuración de NPC con Convai
- [x] Sistema de input mediante botón UI
- [x] Rotación del NPC hacia player
- [x] Documentación completa

### 🚧 Fase 2: Integración AR (En Planificación)
- [ ] Instalación de AR Foundation
- [ ] Detección de planos
- [ ] Sistema de placement
- [ ] Optimización para móvil
- [ ] Build Android funcional

### 📋 Fase 3: Pulido y Optimización (Futuro)
- [ ] Optimización de assets 3D
- [ ] Modelo histórico de Gaudí
- [ ] Sistema de hints/tutorial
- [ ] Audio espacial
- [ ] Build iOS

### 🎯 Fase 4: Distribución (Futuro)
- [ ] Refinamiento de personalidad IA
- [ ] Analytics
- [ ] Material de marketing
- [ ] Publicación en stores

## Contribuir

### Reportar Issues

Si encuentras un bug o tienes una sugerencia:

1. Verifica que no exista un issue similar
2. Crea un nuevo issue con:
   - Descripción clara del problema
   - Pasos para reproducir
   - Screenshots/videos si aplica
   - Dispositivo y versión de Unity

### Pull Requests

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## Créditos

### Asset Utilizado
- [Convai NPC AI Engine](https://assetstore.unity.com/packages/tools/behavior-ai/npc-ai-engine-dialog-actions-voice-and-lipsync-convai-235621)

### Tecnologías
- Unity Technologies - Unity Engine
- Google - ARCore
- Apple - ARKit
- Convai - Conversational AI Platform

### Equipo
- Desarrollo: [Tu Nombre]
- Diseño de Experiencia: [Nombre]
- Contenido Histórico: [Nombre]

## Licencia

Este proyecto es un prototipo educativo. Ver archivo LICENSE para detalles.

### Terceros
- Convai SDK: [Licencia de Convai](https://docs.convai.com/)
- AR Foundation: [Unity Package License](https://unity.com/)

## Contacto

- **Email**: contacto@gaudia.com
- **Website**: https://gaudia.com
- **Twitter**: @GaudIA_AR

## Agradecimientos

- Museos que inspiran este proyecto
- Comunidad de desarrolladores AR de Unity
- Equipo de Convai por su SDK y soporte
- Antoni Gaudí, cuya obra sigue inspirando

---

**Nota**: Este es un proyecto en desarrollo activo. La funcionalidad y documentación pueden cambiar.

**Estado Actual**: Prototipo funcional en desktop. Integración AR en planificación.

Para más información, consulta la [documentación completa](Docs/).
