# GAME DESIGN DOCUMENT (GDD)
# Eco de las Cenizas - Modo PvPvE Cross-Play (PC y Android)

**Referencia de Entrega:** `CODEX-UNITY-6000-5-10F1`
**Género:** Multijugador PvPvE Cooperativo y Competitivo 3D / Guerra de Clanes / Supervivencia Cross-Play
**Plataforma Objetivo:** PC Windows / Android (.apk / .aab) / iOS / Consolas de última generación
**Motor Gráfico:** Unity 6 (6000.5.10f1) URP - Universal Render Pipeline / Mirror Networking (96.11.2)
**Perspectiva:** Tercera Persona 3D (Acción, Supervivencia y Gestión de Megaciudad Vertical)
**Público Objetivo:** Jugadores de supervivencia cooperativa y estrategia multijugador (*Rust*, *Helldivers 2*, *Frostpunk*, *Division 2 Dark Zone*).

---

## 1. PREMISA Y VISIÓN GENERAL

En un mundo post-apocalíptico congelado por una niebla tóxica helada, los supervivientes habitan megaciudades verticales con calefacción geotérmica llamadas **'La Caldera'**. Todos los jugadores en una ciudad comparten el mismo reactor y las mismas murallas: **si la caldera de la ciudad se apaga o es destruida, todos los habitantes de esa ciudad pierden**.

El juego combina cooperación interna intensiva (gestión de recursos, reparación, defensa contra la Niebla Helada) con competición PvPvE entre facciones rivales por el control de la **Ciudad Presidencial Capital (0,0,0)** en un mapa persistente.

---

## 2. CORE LOOP (BUCLE PRINCIPAL DE JUEGO DE 3 FASES)

El juego se estructura en un ciclo dinámico continuo de 3 fases interconectadas:

```
+-----------------------------------------------------------------------------------+
| FASE 1: EXPLORACIÓN 3D EXTERIOR                                                  |
| Recolección del mineral radio-térmico 'Ignicita' en la Niebla Helada exterior.    |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| FASE 2: GESTIÓN DEL REACTOR CENTRAL E INVERNADEROS                              |
| Depósito de Ignicita en la caldera, mantenimiento de temperatura y alimentos.     |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
| FASE 3: DEFENSA DE MURALLAS EN TIEMPO REAL                                        |
| Combate táctico contra oleadas de 'Sombras Heladas' e incursiones enemigas PvP.  |
+-----------------------------------------------------------------------------------+
```

1. **Fase 1: Exploración 3D Exterior (Búsqueda de Ignicita)**
   - Los jugadores abandonan la seguridad de la muralla a través de la Esclusa (`CityAirlock`).
   - Soportan daño acumulativo por exposición a la Niebla Helada tóxica mientras extraen cristales de **Ignicita** y recursos industriales (**Acero**).
   - Combaten criaturas nativas (Sombras Heladas) y jugadores de ciudades rivales.

2. **Fase 2: Gestión del Reactor y Invernaderos**
   - Retorno a la ciudad para depositar la Ignicita en el contenedor global (`ReactorDepositContainer`).
   - El reactor consume Ignicita constantemente. Si el nivel llega a 0, la temperatura cae -5°C/min.
   - Si la temperatura desciende por debajo de **-10°C**, los **Invernaderos se deshabilitan** (deteniendo la producción de comida) y la velocidad de movimiento de los jugadores se reduce en un **20%**.

3. **Fase 3: Defensa de Murallas en Tiempo Real**
   - Ataques de oleadas de criaturas heladas atraídas por el calor del reactor y posibles ataques de clanes rivales en fases PvP.
   - Los jugadores deben operar torretas, reparar sectores de la muralla (`CityWallRepairPanel`) usando Acero y coordinar habilidades de clase para resistir.

---

## 3. CLASES INTERDEPENDIENTES Y MATRIZ DE FALLA EN CASCADA

Ningún jugador puede sobrevivir solo. Las 4 clases son strictly complementarias y la caída de una genera una reacción en cadena que destruye a la comunidad:

| Clase | Rol Principal | Habilidad Clave (Tecla Q / Botón Móvil) |
| :--- | :--- | :--- |
| **Ingeniero** | Mantenimiento de Murallas y Reactor | **Sello Térmico / Overdrive:** Optimiza la eficiencia del reactor e incrementa la velocidad de reparación de murallas. |
| **Explorador** | Recolección Extrema de Ignicita | **Gancho de Agarre / Escáner Térmico:** Movilidad vertical para extraer vetas de alta pureza y detectar amenazas en la niebla. |
| **Científico** | Botánica e Investigación Térmica | **Inyección Biológica / Bio-Cultivo:** Mantiene la fertilidad de los invernaderos e inyecta suero térmico anti-congelamiento a los aliados. |
| **Táctico** | Defensa Militar y Control de Zona | **Foco Incandescendente / Dispositivo de Escudo:** Detiene el avance de Sombras Heladas y resalta puntos débiles de los enemigos. |

### **Matriz de Falla en Cascada (Efecto Dominó):**

- **Si el Ingeniero falla/cae:** Las murallas no se reparen a tiempo y sufren brechas. El reactor pierde eficiencia, duplicando el consumo de Ignicita.
- **Si el Explorador falla/cae:** La ciudad se queda sin suministro de Ignicita. El reactor se apaga, la temperatura cae por debajo de -10°C y activa el debuff de congelamiento.
- **Si el Científico falla/cae:** Los invernaderos se congelan sin posibilidad de recuperación. La comida se agota, reduciendo la salud máxima y resistencia térmica de todos los ciudadanos.
- **Si el Táctico falla/cae:** Las hordas de Sombras Heladas rompen las defensas de las murallas sin oposición táctica, destruyendo las estructuras internas de la caldera.

---

## 4. SISTEMA DE VOTACIÓN DEL CONCEJO ('CONCEJO DE LA CALDERA')

Cuando los recursos comunitarios son escasos (alimentos, piezas de repuesto, catalizadores de Ignicita), el sistema de votación del **Concejo** permite tomar decisiones democráticas sobre la distribución:

- **Firma Única por Jugador (`netId`):** Cada jugador conectado tiene derecho a 1 voto firmado por su id de red único (`connectionId`/`netId`), procesado en el servidor (`CouncilVotingManager`).
- **Validación de Clase en Servidor:** Se verifica la clase real del jugador en el servidor para evitar falsificación de clientes o votos duplicados.
- **Opciones de Distribución:**
  1. *Priorizar Reactor:* Destinar recursos a la caldera (evita el congelamiento).
  2. *Priorizar Invernaderos:* Destinar recursos al suministro de alimentos y curación global.
  3. *Priorizar Murallas:* Reforzar defensas militares e instalar defensas automáticas.

---

## 5. REQUERIMIENTOS Y ESPECIFICACIONES TÉCNICAS

### **5.1. Arquitectura de Red y Servidor-Autoritativo**
- **Servidor Dedicado / Host Autoritativo (Mirror Networking):** Todos los estados de salud de muralla, nivel de combustible del reactor, inventario compartido y estado de votación residen exclusivamente en el servidor.
- **Comandos Validados por Distancia y Capacidad:** Operaciones como `RaidIgnicita`, `DepositIgnicita` y `RepairWall` ejecutan validaciones atómicas en servidor. Si el jugador está fuera de alcance o el contenedor está lleno/vacío, **el intento se cancela sin pérdidas de recursos**.
- **Aislamiento Multi-Ciudad (`GameManager`):** Cada ciudad (`cityID`) gestiona de forma aislada su propio reactor, muralla y almacén. La derrota por congelamiento (-20°C por 5 minutos) liquida únicamente a la ciudad afectada.

### **5.2. Calendario de Temporada de 14 Días y Asedio a la Capital**
```
 Días 1 - 3              Días 4 - 8             Días 9 - 12             Días 13 - 14
+-----------------------+----------------------+-----------------------+-----------------------+
| FASE 1: SETTLEMENT    | FASE 2: EXPANSION    | FASE 3: PRESIDENTIAL  | FASE 4: OVERLOAD WIPE |
| - Inmunidad de Saqueo | - Saqueo de Almacén  |   SIEGE (0,0,0)       | - Súper Tormenta      |
| - Fortificación       | - Captura de Ruinas  | - Hold 3h = Gobernador| - Entrega Cosméticos  |
|   de la Ciudad        |   y Nodos            | - Impuesto 5% Global  | - Reinicio Servidor   |
+-----------------------+----------------------+-----------------------+-----------------------+
```

- **Fase 1 (Días 1-3):** Inmunidad de saqueo entre ciudades. Foco en desarrollo interno.
- **Fase 2 (Días 4-8):** Habilitación de saqueo (`CityLootManager`), control de Nodos de Territorio y captura de Ruinas (4 Principales, 8 Secundarias) que proveen bonificaciones de atributos globales.
- **Fase 3 (Días 9-12):** Asedio a la **Ciudad Presidencial Capital (0,0,0)**. Bucle de presencia continua por 3 horas otorga el título de Gobernador y un impuesto del 5% sobre la cosecha global de Ignicita.
- **Fase 4 (Días 13-14):** Súper Tormenta Helada global (`GlobalEventManager`). Recompensas cosméticas persistentes y reinicio de temporada (*Wipe*).

---

## 6. PROMPTS PARA GENERACIÓN DE ASSETS DE ARTE IA

### **6.1. Arte Conceptual de Entorno (Midjourney / DALL-E 3)**
> `Concept art for a 3D cooperative survival video game, Unreal Engine 5 render. A massive vertical dieselpunk city named 'La Caldera' surrounded by an icy toxic blue fog. At the center, a gigantic geothermal reactor glowing with intense orange heat. Industrial steel structures, layered tier levels, greenhouse domes, high-contrast volumetric lighting, apocalyptic survival mood, hyper-detailed --ar 16:9`

### **6.2. Personaje 3D (Modelo de Explorador)**
> `3D video game character design, T-pose, full body. A rugged 'Explorer' class survivor wearing an insulated Arctic suit, heavy gas mask with orange glowing visor, grappling hook on belt, and a rusted backpack for gathering crystals. Low-poly stylized 3D model, clean topology for rigging, PBR materials --ar 1:1`

### **6.3. Enemigo: Sombras Heladas**
> `3D monster game asset, low-poly creature made of sharp crystalline ice and dark shadowy smoke, glowing blue core, aggressive stance, stylized dark fantasy, Unreal Engine material showcase --ar 1:1`

---

## 7. PROMPTS DE PROGRAMACIÓN E IMPLEMENTACIÓN TÉCNICA

Los scripts C# para Unity y Mirror están implementados en el repositorio en `Assets/Scripts/`:

1. **Lógica del Reactor Central (`ReactorManager.cs` & `ReactorDepositContainer.cs`):**
   - Consume Ignicita cada segundo en servidor.
   - Si Ignicita = 0, cae -5°C por minuto. Por debajo de -10°C desactiva invernaderos y aplica debuff de velocidad (-20%).
   - Permite depósitos atómicos desde el contenedor compartido conservando excesos.

2. **Salud Global de Murallas (`CityWallHealthSync.cs` & `CityWallRepairPanel.cs`):**
   - Sincronización de HP de 4 secciones de muralla mediante `[SyncVar]` visible para todos los clientes en la misma sesión.
   - Validación de costos de Acero antes de reparar.

3. **Generador de Escena y Prefabs (`Assets/Editor/BuildTestSceneAndPrefabs.cs`):**
   - MenuItem `EcoDeLasCenizas -> Build Real Test Scene and Prefabs` en Unity Editor.
   - Crea automáticamente `PlayerPrefab`, `ReactorPrefab`, `CityWallPrefab`, `ResourceNodePrefab` y la escena `TestScene.unity`.

---

## 8. NARRATIVA AMBIENTAL Y GUION DEL TUTORIAL

### **Monólogo Introductorio (Anciano Operador del Reactor Central):**

> *(Sonido de engranajes pesados y vapor a presión escape de la calandria. Un anciano con rostro marcado por hollín y máscara respiratoria dañada mira fijamente al jugador mientras sostiene una llave inglesa gigante).*
>
> *"Escúchame bien, novato... Mira a tu alrededor. Ves esas luces naranjas parpadeando en la niebla? Eso es lo único que separa tus pulmones de convertirse en cristales de hielo sólido. Esta megaciudad, 'La Caldera', no es un lugar para héroes solitarios ni lobos estúpidos.
>
> Aquí no hay 'yo'. Si tú fallas afuera en la niebla, el explorador no trae cristal. Si el explorador no trae cristal, mi reactor se apaga. Si mi reactor se apaga, los invernaderos del nivel 3 se congelan en minutos y nos quedamos sin comida. Y cuando el hambre aprieta, las murallas caen... y las Sombras Heladas devoran lo que queda.
>
> ¿Ves esa luz azul allá afuera rompiendo la pared? El nivel 2 de la ciudad se está congelando AHORA MISMO. Toma tu traje, cruza la esclusa y no regreses sin un núcleo de Ignicita. Trabaja con tu equipo o prepárate para congelarte junto a las cenizas de esta ciudad. ¡Muévanse!"*
