# GaudIA - Proyecto de Museo Interactivo con AR

## Visión del Proyecto

GaudIA es una aplicación de Realidad Aumentada (AR) diseñada para museos que permite a los visitantes conversar con representaciones virtuales de figuras históricas, comenzando con Antonio Gaudí. El proyecto busca revolucionar la experiencia museística mediante la combinación de IA conversacional avanzada y realidad aumentada móvil.

## Concepto Central

Los visitantes del museo podrán:
1. Usar sus dispositivos móviles (Android/iOS)
2. Detectar superficies planas en cualquier lugar del museo
3. Instanciar un avatar 3D de Antonio Gaudí en el espacio real
4. Mantener conversaciones naturales mediante voz
5. Aprender sobre la vida, obra y filosofía del arquitecto de manera interactiva

## Objetivos del Proyecto

### Objetivo Principal
Crear una experiencia educativa inmersiva que combine:
- Tecnología de IA conversacional (Convai)
- Realidad Aumentada (AR Foundation)
- Diseño de experiencia de usuario intuitivo
- Contenido histórico y educativo preciso

### Objetivos Secundarios

1. **Accesibilidad**
   - Aplicación móvil multiplataforma (Android/iOS)
   - Interfaz intuitiva para todos los públicos
   - No requiere conocimientos técnicos previos

2. **Inmersión**
   - Avatar realista con sincronización labial
   - Animaciones naturales y expresivas
   - Rotación del NPC hacia el interlocutor durante la conversación
   - Audio espacial (futuro)

3. **Educación**
   - Conversaciones contextuales sobre Gaudí y sus obras
   - Capacidad de responder preguntas específicas
   - Memoria conversacional (puede recordar temas previos)

4. **Escalabilidad**
   - Arquitectura preparada para añadir más personajes históricos
   - Sistema modular que permite actualizaciones de contenido
   - Posibilidad de crear experiencias para diferentes museos

5. **Performance**
   - Optimización para dispositivos móviles de gama media
   - Detección rápida y estable de superficies
   - Bajo consumo de batería y datos

## Arquitectura Tecnológica

### Stack Tecnológico

```
┌─────────────────────────────────────────────┐
│           Aplicación Unity (APK)            │
├─────────────────────────────────────────────┤
│  AR Foundation (Plane Detection, Anchors)   │
├─────────────────────────────────────────────┤
│  Convai SDK (NPC AI, Voice, Lip Sync)       │
├─────────────────────────────────────────────┤
│  UI Mobile-First (Botón de voz, HUD)        │
├─────────────────────────────────────────────┤
│  ARCore (Android) / ARKit (iOS)             │
└─────────────────────────────────────────────┘
           ↕ Internet (gRPC)
┌─────────────────────────────────────────────┐
│        Servidor Convai (stream.convai.com)  │
│  - Speech-to-Text                           │
│  - AI Processing (GPT-based)                │
│  - Text-to-Speech                           │
│  - Facial Animation Data                    │
└─────────────────────────────────────────────┘
```

### Componentes Principales

1. **AR Foundation**
   - Detección de planos horizontales (suelo)
   - Sistema de anclajes para estabilidad
   - Gestión de sesión AR
   - Camera tracking

2. **Convai NPC System**
   - ConvaiNPC configurado como Antonio Gaudí
   - Personalidad y conocimientos específicos
   - Sincronización labial en español
   - Sistema de acciones (gestos, animaciones)

3. **Mobile UI System**
   - Botón de habla (reemplazo de tecla T)
   - Indicador de NPC activo
   - Feedback visual de escucha/habla
   - Subtítulos opcionales
   - Botón de reposicionamiento

4. **NPC Behavior**
   - Rotación suave hacia el player
   - Animaciones idle naturales
   - Gestos durante la conversación
   - Expresiones faciales

## Fases del Proyecto

### Fase 1: Prototipo Funcional (Actual)
**Estado**: En desarrollo

**Objetivos**:
- [x] Configurar NPC con personalidad de Gaudí
- [x] Integrar Convai SDK en Unity
- [x] Configurar escena de prueba FPS
- [ ] Migrar input de teclado (T) a botón UI
- [ ] Implementar rotación del NPC hacia player
- [ ] Documentar sistema actual

**Entregables**:
- Prototipo PC funcional
- Documentación técnica completa
- Plan de integración AR

### Fase 2: Integración AR Foundation
**Estado**: Planificado

**Objetivos**:
- [ ] Instalar y configurar AR Foundation
- [ ] Implementar detección de planos
- [ ] Reemplazar FPS controller por AR Camera
- [ ] Sistema de instanciación de NPC en superficie detectada
- [ ] Adaptar UI para AR
- [ ] Optimizar rendimiento para móvil

**Entregables**:
- Build Android funcional
- Sistema de placement de NPC
- UI adaptada a AR

### Fase 3: Optimización y Pulido
**Estado**: Futuro

**Objetivos**:
- [ ] Optimización de assets 3D
- [ ] Compresión de audio
- [ ] Sistema de oclusión (NPC detrás de objetos)
- [ ] Iluminación adaptativa según entorno real
- [ ] Audio espacial
- [ ] Sistema de hints/tutorial

**Entregables**:
- Build iOS funcional
- Aplicación optimizada
- Tutorial integrado

### Fase 4: Contenido y Distribución
**Estado**: Futuro

**Objetivos**:
- [ ] Refinamiento de la personalidad de Gaudí
- [ ] Base de conocimientos ampliada
- [ ] Sistema de analytics
- [ ] Preparación para tiendas (Play Store, App Store)
- [ ] Marketing y documentación de usuario

**Entregables**:
- Aplicación publicada
- Material de marketing
- Guía de usuario

## Configuración del Personaje: Antonio Gaudí

### Personalidad en Convai

**Características Definidas**:
- Época: Barcelona, finales s. XIX - principios s. XX
- Estilo conversacional: Apasionado, detallista, místico
- Temas principales:
  - Arquitectura orgánica
  - Sagrada Familia
  - Park Güell
  - Naturaleza como inspiración
  - Filosofía del diseño
  - Relación con Eusebi Güell
  - Barcelona modernista

**Ejemplo de Prompts** (configurados en plataforma Convai):
```
Eres Antonio Gaudí, el famoso arquitecto catalán. Hablas con pasión sobre
tu trabajo, especialmente la Sagrada Familia. Ves la arquitectura como
una expresión de la naturaleza y de Dios. Eres amable pero profundo en tus
explicaciones. Usas metáforas naturales para explicar tus diseños.
```

### Avatar 3D

**Estado Actual**:
- Modelo con cuerpo femenino (placeholder temporal)
- Requiere: Modelo histórico de Gaudí con indumentaria de época

**Requisitos Técnicos**:
- Rigging humanoid compatible con Unity
- Blend shapes para lip sync (ARKit o visemas)
- LODs para optimización móvil
- Materiales optimizados para mobile

## Experiencia de Usuario

### Flujo Ideal

```
1. Usuario abre la app
   ↓
2. Tutorial rápido (primera vez)
   ↓
3. Cámara AR se activa
   ↓
4. Sistema detecta planos horizontales
   ↓
5. Usuario toca en el suelo donde quiere a Gaudí
   ↓
6. Avatar de Gaudí aparece en el espacio real
   ↓
7. Animación de "saludo" inicial
   ↓
8. UI muestra botón de "Hablar"
   ↓
9. Usuario presiona y mantiene el botón
   ↓
10. Indicador visual muestra que está escuchando
    ↓
11. Usuario habla: "¿Qué te inspiró para la Sagrada Familia?"
    ↓
12. Usuario suelta el botón
    ↓
13. Gaudí se gira suavemente hacia el usuario
    ↓
14. Gaudí responde con voz y lip sync
    ↓
15. Conversación continúa...
```

### UI Elements

**HUD Permanente**:
- Botón de voz (esquina inferior)
- Indicador de estado (escuchando/procesando/hablando)
- Nombre del personaje
- Botón de opciones/salir

**Opcionales**:
- Subtítulos (activables)
- Botón de reposicionamiento de NPC
- Historial de conversación
- Botones de preguntas sugeridas

## Casos de Uso en Museo

### Escenario 1: Visitante Individual
Un visitante llega a la sección de Gaudí del museo, saca su móvil, escanea el QR del museo que lo lleva a descargar la app, coloca a Gaudí virtual en el centro de la sala, y le pregunta sobre las obras expuestas.

### Escenario 2: Visita Guiada Aumentada
Un grupo de estudiantes usa la app simultáneamente. El profesor hace preguntas a Gaudí que todos pueden escuchar. Los estudiantes pueden acercarse individualmente después para preguntas personales.

### Escenario 3: Experiencia Familiar
Una familia con niños coloca a Gaudí a escala infantil (agachado), los niños pueden preguntarle cosas simples sobre colores y formas de sus edificios.

### Escenario 4: Investigación Profunda
Un estudiante de arquitectura mantiene una conversación de 30 minutos sobre técnicas estructurales específicas, aprovechando la memoria conversacional del sistema.

## Métricas de Éxito

### Técnicas
- Tiempo de detección de plano < 3 segundos
- Estabilidad del anchor > 95%
- Latencia de respuesta de voz < 2 segundos
- Frame rate > 30 FPS en dispositivos objetivo
- Tamaño de APK < 150 MB

### Experiencia
- Sesión promedio > 5 minutos
- Tasa de satisfacción > 4/5 estrellas
- Número de preguntas por sesión > 3
- Tasa de recomendación > 70%

### Educativas
- Retención de información > experiencia tradicional
- Engagement medible mediante analytics
- Feedback positivo de educadores

## Consideraciones Técnicas

### Conectividad
- Requiere conexión a Internet (para Convai API)
- Implementar sistema de cache para respuestas comunes
- Mensaje claro si no hay conexión

### Privacidad
- No almacenar conversaciones localmente
- Política de privacidad clara sobre uso de voz
- Cumplimiento GDPR

### Accesibilidad
- Subtítulos para personas con discapacidad auditiva
- Modo de alto contraste
- Soporte para voice-over (iOS)

### Multilenguaje (Futuro)
- Español (prioritario)
- Catalán (importante para contexto)
- Inglés (turismo internacional)
- Otros idiomas según demanda

## Riesgos y Mitigaciones

| Riesgo | Impacto | Probabilidad | Mitigación |
|--------|---------|--------------|------------|
| Latencia de red alta | Alto | Media | Cache de respuestas, feedback visual claro |
| Dispositivos incompatibles | Medio | Baja | Lista clara de requisitos, fallback a modo 2D |
| Consumo de datos excesivo | Medio | Media | Optimizar streaming, advertencia de uso de datos |
| Baja calidad de respuestas IA | Alto | Baja | Testing extensivo, refinamiento de prompts |
| Problemas de AR tracking | Alto | Media | Instrucciones claras, recalibración fácil |
| Modelo 3D no disponible a tiempo | Bajo | Baja | Priorizar funcionalidad sobre assets finales |

## Expansión Futura

### Más Personajes
- Pablo Picasso
- Salvador Dalí
- Joan Miró
- Frida Kahlo
- Leonardo da Vinci

### Características Avanzadas
- Modo multi-NPC (conversación entre dos personajes históricos)
- Integración con exhibits físicos mediante marcadores
- Experiencias narrativas guiadas
- Minijuegos educativos
- Sistema de logros

### Plataformas
- WebXR para acceso sin instalación
- VR para experiencias inmersivas de escritorio
- Instalaciones permanentes en museos

## Conclusión

GaudIA representa la convergencia de tecnología de punta (IA, AR) con educación y cultura. El proyecto no solo es técnicamente viable con las herramientas actuales (Unity, Convai, AR Foundation), sino que llena un nicho claro en la experiencia museística moderna.

La fase actual de prototipo establece las bases sólidas, y la hoja de ruta está clara hacia un producto final pulido y escalable. El enfoque en mobile-first desde el inicio asegura que la experiencia será accesible para el público general.

**Próximos Pasos Inmediatos**:
1. Completar migración de input a UI button
2. Implementar rotación del NPC
3. Documentar plan de AR Foundation
4. Comenzar integración AR básica
5. Testing en dispositivos reales
